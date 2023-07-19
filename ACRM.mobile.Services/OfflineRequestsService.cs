using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services
{
    public class OfflineRequestsService : IOfflineRequestsService
    {
        protected IOfflineRequestsUnitOfWork _offlineRequestsUnitOfWork;
        protected readonly ICrmDataFieldResolver _crmDataFieldResolver;
        protected readonly ISessionContext _sessionContext;

        public OfflineRequestsService(IOfflineRequestsUnitOfWork offlineRequestsUnitOfWork,
            ICrmDataFieldResolver crmDataFieldResolver,
            ISessionContext sessionContext)
        {
            _offlineRequestsUnitOfWork = offlineRequestsUnitOfWork;
            _crmDataFieldResolver = crmDataFieldResolver;
            _sessionContext = sessionContext;
        }

        public async Task<List<OfflineRequest>> GetAllRequests(CancellationToken cancellationToken)
        {
            return await _offlineRequestsUnitOfWork.GetAllRequests(cancellationToken);
        }

        public async Task Delete(OfflineRequest offlineRequest, CancellationToken cancellationToken)
        {
            _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Remove(offlineRequest);
            _offlineRequestsUnitOfWork.Save();
        }

        public async Task Update(OfflineRequest offlineRequest, CancellationToken cancellationToken)
        {
            _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Update(offlineRequest);
            _offlineRequestsUnitOfWork.Save();
        }

        public async Task<OfflineRequest> CreateDeleteRequest(ActionTemplateBase template, string infoAreaId, string recordId)
        {
            OfflineRequest offlineRequest = new OfflineRequest()
            {
                RequestType = "Records",
                SyncDate = DateTime.Now.ToString(CrmConstants.ExecutionDateTimeFormat),
                JsonData = template?.ToJsonString(),
                ProcessType = "EditRecord",
                AppVersion = "3.0.0",
                Draft = 0,
                DetailsLine = template?.ConfigName()
            };

            await Task.Run(() =>
            {
                OfflineRecord offlineRecord = new OfflineRecord()
                {
                    RecordId = recordId,
                    InfoAreaId = infoAreaId,
                    Mode = "Delete"
                };

                offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Add(offlineRequest);
                _offlineRequestsUnitOfWork.Save();
            });

            return offlineRequest;
        }

        private string GetRecordId(List<string> recordIds, string infoAreaId)
        {
            foreach(string recId in recordIds)
            {
                if(recId.IsRecordIdFormated())
                {
                    return recId;
                }
            }

            return string.Empty;
        }

        public async Task<OfflineRequest> CreateUpdateRequest(ActionTemplateBase template, FieldControl fieldControl, List<PanelData> inputPanels, List<string> recordIds, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = CreateRequest(template);
            await Task.Run(() =>
            {
                bool isSaveNeeded = false;
                string recordId = GetRecordId(recordIds, fieldControl.InfoAreaId);
                if (!string.IsNullOrWhiteSpace(recordId))
                { 
                    (List<OfflineRecordField> recordFields, List<OfflineRecordLink> recordLinks) = ExtractNonEmptyFields(fieldControl, inputPanels, false, fieldControl.InfoAreaId);

                    if (!string.IsNullOrWhiteSpace(recordId) && (recordFields.Count > 0 || recordLinks.Count > 0))
                    {

                        OfflineRecord offlineRecord = new OfflineRecord()
                        {
                            RecordId = recordId,
                            InfoAreaId = fieldControl.InfoAreaId,
                            Mode = "Update"
                        };

                        offlineRecord.RecordFields = recordFields;
                        offlineRecord.RecordLinks = recordLinks;
                        offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                        isSaveNeeded = true;
                    }   
                }
               
                if (inputPanels.HasEditPanelChild())
                {
                    foreach (var tab in fieldControl.Tabs)
                    {
                        if (tab.GetEditPanelType() == PanelType.EditPanelChildren && !string.IsNullOrWhiteSpace(tab.TabFieldsInfoAreaId()))
                        {
                            string infoAreaId = tab.TabFieldsInfoAreaId();
                            recordId = GetRecordId(recordIds, infoAreaId);
                            if (!string.IsNullOrWhiteSpace(recordId))
                            {
                                (List<OfflineRecordField> recordFields, List<OfflineRecordLink> recordLinks) = ExtractNonEmptyFields(fieldControl, inputPanels, false, infoAreaId);

                                if (recordFields.Count > 0 || recordLinks.Count > 0)
                                {
                                    OfflineRecord offlineRecord = new OfflineRecord()
                                    {
                                        RecordId = recordId,
                                        InfoAreaId = infoAreaId,
                                        Mode = "Update"
                                    };

                                    offlineRecord.RecordFields = recordFields;
                                    offlineRecord.RecordLinks = recordLinks;

                                    if (offlineRequest.Records == null)
                                    {
                                        offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                                    }
                                    else
                                    {
                                        offlineRequest.Records.Add(offlineRecord);
                                    }
                                    isSaveNeeded = true;
                                }
                            }
                        }
                    }
                }

                if(isSaveNeeded)
                {
                    _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Add(offlineRequest);
                    _offlineRequestsUnitOfWork.Save();
                }
            });

            return offlineRequest;
        }

        public async Task<OfflineRequest> CreateUpdateRequest(ActionTemplateBase template, TableInfo tableInfo, string recordId, 
            Dictionary<string, string> templateFilterValues, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = CreateRequest(template);
            await Task.Run(() =>
            {

                OfflineRecord offlineRecord = new OfflineRecord()
                {
                    RecordId = recordId,
                    InfoAreaId = tableInfo.InfoAreaId,
                    Mode = "Update"
                };

                List<OfflineRecordField> templateExtraFields = ExtractTemplateFilterNotIncludedFields(tableInfo, templateFilterValues);

                offlineRecord.RecordFields = templateExtraFields;
                offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Add(offlineRequest);
                _offlineRequestsUnitOfWork.Save();
            });

            return offlineRequest;
        }

        public async Task<bool> CreateDocumentUploadRequest(DocumentUpload uploadRequest, ViewReference ViewReference, FieldControl fieldControl, List<PanelData> inputPanels, Dictionary<string, string> templateFilterValues, OfflineRecordLink ParenLink, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = new OfflineRequest()
            {
                RequestType = "DocumentUpload",
                SyncDate = DateTime.Now.ToString(CrmConstants.ExecutionDateTimeFormat),
                JsonData = ViewReference?.ToJsonString(),
                ProcessType = "DocumentUpload",
                AppVersion = "3.0.0",
                DetailsLine = fieldControl.UnitName
            };

            await Task.Run(() =>
            {

                OfflineRecord offlineRecord = new OfflineRecord()
                {
                    RecordId = "-1",
                    InfoAreaId = fieldControl.InfoAreaId,
                    Mode = "Update"
                };

                (List<OfflineRecordField> recordFields, List<OfflineRecordLink> recordLinks) = ExtractNonEmptyFields(fieldControl, inputPanels, true, fieldControl.InfoAreaId);
                List<OfflineRecordField> templateExtraFields = ExtractTemplateFilterNotIncludedFields(fieldControl.InfoAreaId, null, inputPanels, templateFilterValues, recordFields);
                if (templateExtraFields.Count > 0)
                {
                    recordFields.AddRange(templateExtraFields);
                }
                if (ParenLink != null && !recordLinks.Any(l => l.InfoAreaId.Equals(ParenLink.InfoAreaId) && l.LinkId.Equals(ParenLink.LinkId)))
                {
                    recordLinks.Add(ParenLink);
                }

                offlineRecord.RecordFields = recordFields;
                offlineRecord.RecordLinks = recordLinks;
                offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                offlineRequest.DocumentUploads = new List<DocumentUpload>() { uploadRequest };
                _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Add(offlineRequest);
                _offlineRequestsUnitOfWork.Save();
            });

            return true;
        }

        public async Task<OfflineRequest> CreateModifyRequest(ActionTemplateBase template, TableInfo tableInfo, string recordId, Dictionary<string, string> templateFilterValues, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = CreateRequest(template);

            await Task.Run(() =>
            {
                OfflineRecord offlineRecord = new OfflineRecord()
                {
                    RecordId = recordId,
                    InfoAreaId = tableInfo.InfoAreaId,
                    Mode = "Update"
                };

                List<OfflineRecordField> recordFields = ExtractTemplateFilterFields(tableInfo, templateFilterValues);

                //TODO recordLinks and ComputeLinks
                List<OfflineRecordLink> recordLinks = new List<OfflineRecordLink>();
                /*
                if (ParenLink != null && !recordLinks.Any(l => l.InfoAreaId.Equals(ParenLink.InfoAreaId) && l.LinkId.Equals(ParenLink.LinkId)))
                {
                    recordLinks.Add(ParenLink);
                }
                */

                offlineRecord.RecordFields = recordFields;
                offlineRecord.RecordLinks = recordLinks;
                offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Add(offlineRequest);
                _offlineRequestsUnitOfWork.Save();
            });

            return offlineRequest;
        }

        public async Task<OfflineRequest> CreateSaveRequest(ActionTemplateBase template, FieldControl fieldControl, TableInfo tableInfo,
            List<PanelData> inputPanels, Dictionary<string, string> templateFilterValues,
            List<OfflineRecordLink> parenntRecordLinks, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = CreateRequest(template);

            await Task.Run(() =>
            {
                OfflineRecord offlineRecord = CreateOfflineRecord(template, fieldControl, tableInfo, inputPanels, templateFilterValues, parenntRecordLinks);
                OfflineRecord childOfflineRecord = CreateChildOfflineRecord(fieldControl,
                    new OfflineRecordLink
                    {
                        InfoAreaId = tableInfo.InfoAreaId,
                        LinkId = -1,
                        RecordId = offlineRecord.RecordId
                    },
                    inputPanels,
                    templateFilterValues);

                offlineRequest.Records = new List<OfflineRecord>() { offlineRecord };
                if (childOfflineRecord != null)
                {
                    offlineRequest.Records.Add(childOfflineRecord);
                }
                _offlineRequestsUnitOfWork.GenericRepository<OfflineRequest>().Add(offlineRequest);
                _offlineRequestsUnitOfWork.Save();
            });

            return offlineRequest;
        }

        private OfflineRecord CreateChildOfflineRecord(FieldControl fieldControl, OfflineRecordLink parentLink, List<PanelData> inputPanels, Dictionary<string, string> templateFilterValues)
        {
            OfflineRecord childOfflineRecord = null;
            if (inputPanels.HasEditPanelChild())
            {
                foreach (var tab in fieldControl.Tabs)
                {
                    if (tab.GetEditPanelType() == PanelType.EditPanelChildren && !string.IsNullOrWhiteSpace(tab.TabFieldsInfoAreaId()))
                    {
                        string childInfoAreaId = tab.TabFieldsInfoAreaId();
                        string recordId = $"new{_offlineRequestsUnitOfWork.MaxRequestId():d8}{_offlineRequestsUnitOfWork.MaxRecordId():d4}";

                        (List<OfflineRecordField> recordFields, List<OfflineRecordLink> recordLinks) = ExtractNonEmptyFields(fieldControl, inputPanels, true, childInfoAreaId);
                        List<OfflineRecordField> templateExtraFields = ExtractTemplateFilterNotIncludedFields(childInfoAreaId, null, inputPanels, templateFilterValues, recordFields);
                        if (templateExtraFields.Count > 0)
                        {
                            recordFields.AddRange(templateExtraFields);
                        }

                        if (recordFields.Count > 0 || recordLinks.Count > 0)
                        {
                            childOfflineRecord = new OfflineRecord()
                            {
                                RecordId = recordId,
                                InfoAreaId = childInfoAreaId,
                                Mode = "New"
                            };

                            childOfflineRecord.RecordFields = recordFields;
                            childOfflineRecord.RecordLinks = new List<OfflineRecordLink> { parentLink };
                            if (recordLinks != null && recordLinks.Count > 0)
                            {
                                childOfflineRecord.RecordLinks.AddRange(recordLinks);
                            }
                        }
                    }
                }
            }

            return childOfflineRecord;
        }

        private OfflineRecord CreateOfflineRecord(ActionTemplateBase template, FieldControl fieldControl, TableInfo tableInfo, List<PanelData> inputPanels, Dictionary<string, string> templateFilterValues, List<OfflineRecordLink> parenntRecordLinks)
        {
            string recordId = $"new{_offlineRequestsUnitOfWork.MaxRequestId():d8}{_offlineRequestsUnitOfWork.MaxRecordId():d4}";
            OfflineRecord offlineRecord = new OfflineRecord()
            {
                RecordId = recordId,
                InfoAreaId = fieldControl.InfoAreaId,
                Mode = "New"
            };

            (List<OfflineRecordField> recordFields, List<OfflineRecordLink> recordLinks) = ExtractNonEmptyFields(fieldControl, inputPanels, true, fieldControl.InfoAreaId);
            List<OfflineRecordField> templateExtraFields = ExtractTemplateFilterNotIncludedFields(fieldControl.InfoAreaId, tableInfo, inputPanels, templateFilterValues, recordFields);
            if (templateExtraFields.Count > 0)
            {
                recordFields.AddRange(templateExtraFields);
            }

            if (template != null && template.ShouldComputeLinks())
            {
                ComputeLinks(fieldControl, recordFields, recordLinks);
            }

            if (parenntRecordLinks != null && parenntRecordLinks.Count > 0)
            {
                foreach (var link in parenntRecordLinks)
                {
                    if (link != null && !recordLinks.Any(l => l.IsSameInfoArea(link.InfoAreaId) && l.HasLinkIdEqualWith(link.LinkId)))
                    {
                        recordLinks.Add(link);
                    }
                }
            }

            offlineRecord.RecordFields = recordFields;
            offlineRecord.RecordLinks = recordLinks;
            return offlineRecord;
        }

        private void ComputeLinks(FieldControl fieldControl, List<OfflineRecordField> recordFields, List<OfflineRecordLink> recordLinks)
        {
            foreach (LinkInfo link in _crmDataFieldResolver.TableInfoForInfoArea(fieldControl.InfoAreaId).Links)
            {
                if (!link.IsGenericLink()
                    && link.HasColumn()
                    && !CheckIfLinkWasAdded(recordLinks, link))
                {
                    LinkInfo targetLinkInfo = _crmDataFieldResolver.GetIdentLinkInfo(link.TargetInfoAreaId);
                    if (targetLinkInfo != null && targetLinkInfo.LinkFields != null && targetLinkInfo.LinkFields.Count() == 2)
                    {
                        LinkFields linkedFieldOne = link.LinkFieldWithTargetFieldIndex(targetLinkInfo.FirstField().DestFieldId);
                        LinkFields linkedFieldTwo = link.LinkFieldWithTargetFieldIndex(targetLinkInfo.SecondField().DestFieldId);

                        int intStatNo = -1, intRecordNo = -1;
                        if (linkedFieldOne != null && linkedFieldTwo != null)
                        {
                            string fieldValue = GetValueForLinkField(recordFields, linkedFieldOne);
                            if (string.IsNullOrWhiteSpace(fieldValue) || !int.TryParse(fieldValue, out intStatNo))
                            {
                                // here we may need to try get staNo in a different way.
                            }
                            fieldValue = GetValueForLinkField(recordFields, linkedFieldTwo);
                            if (string.IsNullOrWhiteSpace(fieldValue) || !int.TryParse(fieldValue, out intRecordNo))
                            {
                                // here we may need to try get recordno in a different way.
                            }

                            if (intStatNo >= 0 && intRecordNo >= 0)
                            {
                                bool ignore = false;
                                if (link.LinkFields != null)
                                {
                                    foreach (LinkFields field in link.LinkFields)
                                    {
                                        if (field.DestFieldId < 0 && !string.IsNullOrEmpty(field.DestValue))
                                        {
                                            fieldValue = GetValueForLinkField(recordFields, field);
                                            
                                            if (!string.Equals(fieldValue, field.DestValue))
                                            {
                                                ignore = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (!ignore)
                                    {
                                        string linkRecordId = null;
                                        if (_sessionContext.OfflineStationNumber > 0 && intStatNo == _sessionContext.OfflineStationNumber)
                                        {
                                            linkRecordId = $"{link.TargetInfoAreaId}.new{intRecordNo >> 16:x8}{intRecordNo & 65535:x4}";
                                        }
                                        else
                                        {
                                            linkRecordId = $"{link.TargetInfoAreaId}.x{intStatNo:x8}{intRecordNo:x8}";
                                        }

                                        recordLinks.Add(
                                            new OfflineRecordLink
                                            {
                                                InfoAreaId = link.TargetInfoAreaId,
                                                LinkId = link.LinkId,
                                                RecordId = linkRecordId
                                            });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetValueForLinkField(List<OfflineRecordField> fields, LinkFields linkField)
        {
            foreach (OfflineRecordField field in fields)
            {
                if (field.FieldId == linkField.SourceFieldId)
                {
                    return field.NewValue;
                }
            }
            return null;
        }

        private static OfflineRequest CreateRequest(ActionTemplateBase template)
        {
            return new OfflineRequest()
            {
                RequestType = "Records",
                SyncDate = DateTime.Now.ToString(CrmConstants.ExecutionDateTimeFormat),
                JsonData = template?.ToJsonString(),
                ProcessType = "EditRecord",
                AppVersion = "3.0.0",
                Draft = 0,
                DetailsLine = template?.ConfigName()
            };
        }

        private List<OfflineRecordField> ExtractTemplateFilterFields(TableInfo tableInfo, Dictionary<string, string> templateFilterValues)
        {
            List<OfflineRecordField> recordFields = new List<OfflineRecordField>();

            if (templateFilterValues == null || templateFilterValues.Keys.Count == 0)
            {
                return recordFields;
            }

            List<string> filterKeys = templateFilterValues.Keys.ToArray().ToList();
            foreach (string filterKey in filterKeys)
            {
                (string infoArea, int fieldId) = GetFieldInfo(filterKey);
                FieldInfo fieldInfo = tableInfo.Fields.Find(f => f.FieldId == fieldId);

                if (infoArea == tableInfo.InfoAreaId)
                {
                    AddTemplateFieldToRecordFields(fieldId, fieldInfo, templateFilterValues, recordFields, filterKey);
                }
            }

            return recordFields;
        }

        private (string, int) GetFieldInfo(string filterKey)
        {
            var ids = filterKey.Split('_');
            string infoAreaId = ids[0];
            int fieldId = int.Parse(ids[1]);
            return (infoAreaId, fieldId);
        }

        private void AddTemplateFieldToRecordFields(int fieldId, FieldInfo fieldInfo, Dictionary<string, string> templateFilterValues, List<OfflineRecordField> recordFields, string filterKey)
        {
            string value = string.Copy(templateFilterValues[filterKey]);

            if (fieldInfo != null && fieldInfo.IsDate && !string.IsNullOrWhiteSpace(value))
            {
                if (DateTime.TryParse(value, out DateTime dtResult))
                {
                    value = dtResult.ToCrmDateString();
                }
                else
                {
                    value = string.Empty;
                }
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                recordFields.Add(new OfflineRecordField()
                {
                    FieldId = fieldId,
                    NewValue = value,
                    Offline = 0
                });
            }
            else
            {
                templateFilterValues.Remove(filterKey);
            }
        }

        private List<OfflineRecordField> ExtractTemplateFilterNotIncludedFields(string infoAreaId, TableInfo tableInfo, List<PanelData> inputPanels, 
            Dictionary<string, string> templateFilterValues, List<OfflineRecordField> existingRecordFields)
        {
            List<OfflineRecordField> recordFields = new List<OfflineRecordField>();

            if(templateFilterValues == null || templateFilterValues.Keys.Count == 0)
            {
                return recordFields;
            }

            List<string> filterKeys = templateFilterValues.Keys.ToArray().ToList();

            foreach (PanelData panel in inputPanels)
            {
                foreach (ListDisplayField field in panel.Fields)
                {
                    FieldInfo fieldInfo = field.Config.PresentationFieldAttributes.FieldInfo;
                    string key = $"{fieldInfo.TableInfoInfoAreaId}_{fieldInfo.FieldId}";

                    if (filterKeys.Contains(key))
                    {
                        if (infoAreaId.Equals(fieldInfo.TableInfoInfoAreaId))
                        {
                            if (existingRecordFields.Find(rf => rf.FieldId == fieldInfo.FieldId) == null)
                            {
                                string value = templateFilterValues[key];
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    if (fieldInfo.IsDate || fieldInfo.IsTime)
                                    {
                                        value = value.Replace("-", "").Replace(":", "");
                                    }

                                    recordFields.Add(new OfflineRecordField()
                                    {
                                        FieldId = fieldInfo.FieldId,
                                        NewValue = value,
                                        Offline = 0
                                    });
                                }
                            }
                        }
                        filterKeys.Remove(key);
                    }
                }
            }

            foreach (string filterKey in filterKeys)
            {
                (string fieldInfoArea, int fieldId) = GetFieldInfo(filterKey);

                if (string.Equals(fieldInfoArea, infoAreaId))
                {
                    FieldInfo fieldInfo = null;
                    if (tableInfo != null)
                    {
                        fieldInfo = tableInfo.Fields.Find(f => f.FieldId == fieldId);
                    }
                    
                    AddTemplateFieldToRecordFields(fieldId, fieldInfo, templateFilterValues, recordFields, filterKey);
                }
            }

            return recordFields;
        }

        private List<OfflineRecordField> ExtractTemplateFilterNotIncludedFields(TableInfo tableInfo, Dictionary<string, string> templateFilterValues)
        {
            List<OfflineRecordField> recordFields = new List<OfflineRecordField>();

            if (templateFilterValues == null || templateFilterValues.Keys.Count == 0)
            {
                return recordFields;
            }

            List<string> filterKeys = templateFilterValues.Keys.ToArray().ToList();

            foreach (string filterKey in filterKeys)
            {
                (string infoArea, int fieldId) = GetFieldInfo(filterKey);

                if (infoArea == tableInfo.InfoAreaId)
                {
                    FieldInfo fieldInfo = null;
                    if (tableInfo != null)
                    {
                        fieldInfo = tableInfo.Fields.Find(f => f.FieldId == fieldId);
                    }

                    AddTemplateFieldToRecordFields(fieldId, fieldInfo, templateFilterValues, recordFields, filterKey);
                }
            }

            return recordFields;
        }

        private (List<OfflineRecordField>, List<OfflineRecordLink>) ExtractNonEmptyFields(FieldControl fieldControl, List<PanelData> inputPanels, bool isNewRecord, string recordInfoArea)
        {
            List<OfflineRecordField> recordFields = new List<OfflineRecordField>();
            List<OfflineRecordLink> recordLinks = new List<OfflineRecordLink>();
            foreach (PanelData panel in inputPanels)
            {
                foreach (ListDisplayField field in panel.Fields)
                {
                    bool locked = isNewRecord ? field.Config.PresentationFieldAttributes.LockedOnNew : field.Config.PresentationFieldAttributes.LockedOnUpdate;
                    if(panel.FourceUpdate)
                    {
                        locked = false;
                    }

                    if (field.EditData.ChangeOfflineRequest == null || locked)
                    {
                        continue;
                    }
                    else if (field.EditData.ChangeOfflineRequest is OfflineRecordField offRequest && recordInfoArea.Equals(field.Config.FieldConfig.InfoAreaId))
                    {
                        recordFields.Add(offRequest);
                    }
                    else if (field.EditData.ChangeOfflineRequest is OfflineRecordLink offLinkRequest)
                    {
                        recordLinks.Add(offLinkRequest.Clone());
                    }
                }
            }

            return (recordFields, recordLinks);
        }

        private bool CheckIfLinkWasAdded(List<OfflineRecordLink> recordLinks, LinkInfo link)
        {
            foreach (OfflineRecordLink recordLink in recordLinks)
            {
                if (recordLink.LinkId == link.LinkId && recordLink.InfoAreaId == link.TargetInfoAreaId)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
