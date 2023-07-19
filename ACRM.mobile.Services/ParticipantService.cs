using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services
{
    public class ParticipantService : ContentServiceBase, IParticipantService
    {
        protected ISerialEntryEditService _ChildEditServic;
        private FieldInfo accfieldInfo = null;
        private FieldInfo reqfieldInfo = null;
        List<CrmRep> crmReps;
        public List<PopupListItem> Requirements { get; set; }
        public List<PopupListItem> Acceptance { get; set; }
        public ParticipantService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            ISerialEntryEditService ChildEditServic) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _ChildEditServic = ChildEditServic;
        }
        public ISerialEntryEditService ChildEditServic
        {
            get
            {
                return _ChildEditServic;
            }
        }
        public FieldInfo AcceptanceFieldInfo
        {
            get
            {
                return accfieldInfo;
            }
        }
        public FieldInfo RequirementFieldInfo
        {
            get
            {
                return reqfieldInfo;
            }
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            crmReps = await _fieldGroupComponent.RepServiceobj.GetAllCrmReps(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<ParticipantData>> BuildParticipants(PanelData data, CancellationToken cancellationToken)
        {
            List<ParticipantData> rows = new List<ParticipantData>();
            List<string> repIds = new List<string>();

            FieldControl _listFieldControl = null;
            var field = data.Fields.Where(a => a.Config.PresentationFieldAttributes.FieldInfo.IsParticipant).FirstOrDefault();

            if (field != null)
            {
                if (string.IsNullOrWhiteSpace(field.Data.StringData))
                {
                    var repid = CrmRep.FormatToAureaRepId(_sessionContext.User.SessionInformation.RepIdStr());

                    var rep = crmReps.Where(r => r.Id == repid).FirstOrDefault();
                    if (rep != null)
                    {
                        rows.Add(new ParticipantData(rep)
                        {
                            AllowDelete = false
                        });
                    }
                }
                else
                {
                    rows = ParticipantsFromString(field.Data.StringData);
                }

                var tabTypeParts = data.PanelTypeKey.Split('_');

                string linkParticipantsName = null;
                if (tabTypeParts.Length > 1)
                {
                    linkParticipantsName = tabTypeParts[1];
                    var configParts = linkParticipantsName.Split('#');
                    if (configParts.Length > 1)
                    {
                        linkParticipantsName = configParts[0];
                    }
                }
                var _searchAndList = await _configurationService.GetSearchAndList(linkParticipantsName, cancellationToken).ConfigureAwait(false);
                if (_searchAndList != null)
                {
                    _listFieldControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".List", cancellationToken);
                }

                if (_listFieldControl?.Tabs.Count > 0)
                {
                    _infoArea = _configurationService.GetInfoArea(_listFieldControl.InfoAreaId);
                    TableInfo tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                    List<FieldControlField> fields = _listFieldControl.Tabs[0].GetQueryFields();
                    var accField = fields.Where(a => a.Function.Equals("Acceptance")).FirstOrDefault();
                    var reqField = fields.Where(a => a.Function.Equals("Requirement")).FirstOrDefault();

                    if (accField != null)
                    {
                        accfieldInfo = await _configurationService.GetFieldInfo(tableInfo, accField, cancellationToken).ConfigureAwait(false);
                    }
                    if (accField != null)
                    {
                        reqfieldInfo = await _configurationService.GetFieldInfo(tableInfo, reqField, cancellationToken).ConfigureAwait(false);
                    }


                    var infoAreaID = _configurationService.GetInfoArea("ID");

                    foreach (var Participant in rows)
                    {
                        var Rep = crmReps.Where(a => a.Id.Equals(Participant.RepIdString, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (Rep != null)
                        {
                            Participant.Name = Rep.Name;
                            if (accfieldInfo != null)
                            {
                                Participant.AcceptanceDisplayText = await _fieldGroupComponent.CatalogComponentObject.GetCatalogValue(accfieldInfo, Participant.AcceptanceText, cancellationToken).ConfigureAwait(false);

                            }
                            if (accfieldInfo != null)
                            {
                                Participant.RequirementDisplayText = await _fieldGroupComponent.CatalogComponentObject.GetCatalogValue(reqfieldInfo, Participant.RequirementText, cancellationToken).ConfigureAwait(false);

                            }
                            Participant.LeftMarginColor = PageAccentColor(infoAreaID);
                            Participant.ImageSource = _imageResolverComponent.InoAreaImageTuple(infoAreaID, _configurationService);

                        }

                    }
                    Requirements = await GetRequirementCatalogs(cancellationToken);
                    Acceptance = await GetAcceptanceCatalogs(cancellationToken);
                    _fieldGroupComponent.InitializeContext(_listFieldControl, tableInfo);
                    if (_fieldGroupComponent.HasTabs())
                    {

                        ParentLink parentLink = new ParentLink
                        {
                            LinkId = -1,
                            ParentInfoAreaId = data.RecordInfoArea,
                            RecordId = data.RecordId,
                        };

                        var RawData = await _crmDataService.GetData(cancellationToken,
                            new DataRequestDetails
                            {
                                TableInfo = tableInfo,
                                Fields = fields,
                                SortFields = _listFieldControl.SortFields
                            },
                            parentLink, 100, RequestMode.Best);

                        if (RawData.Result != null)
                        {
                            var tasks = RawData.Result.Rows.Cast<DataRow>().Select(async row => await GetParticipantRow(fields, _fieldGroupComponent, row, cancellationToken));
                            var items = await Task.WhenAll(tasks).ConfigureAwait(false);
                            if (items != null && items.Length > 0)
                            {
                                rows.AddRange(items);
                            }
                        }
                    }
                }
            }

            return rows;
        }

        public async Task<List<ParticipantData>> BuildRepParticipants(PanelData data, CancellationToken cancellationToken)
        {
            List<ParticipantData> rows = new List<ParticipantData>();
            List<string> repIds = new List<string>();

            FieldControl _listFieldControl = null;
            var field = data.Fields.Where(a => a.Config.PresentationFieldAttributes.FieldInfo.IsParticipant).FirstOrDefault();

            if (field != null)
            {
                if (string.IsNullOrWhiteSpace(field.Data.StringData))
                {
                    var repid = CrmRep.FormatToAureaRepId(_sessionContext.User.SessionInformation.RepIdStr());

                    var rep = crmReps.Where(r => r.Id == repid).FirstOrDefault();
                    if (rep != null)
                    {
                        rows.Add(new ParticipantData(rep)
                            {
                                AllowDelete = false
                            });
                    }
                }
                else
                {
                    rows = ParticipantsFromString(field.Data.StringData);
                }

                var tabTypeParts = data.PanelTypeKey.Split('_');

                string linkParticipantsName = null;
                if (tabTypeParts.Length > 1)
                {
                    linkParticipantsName = tabTypeParts[1];
                    var configParts = linkParticipantsName.Split('#');
                    if (configParts.Length > 1)
                    {
                        linkParticipantsName = configParts[0];
                    }
                }
                var _searchAndList = await _configurationService.GetSearchAndList(linkParticipantsName, cancellationToken).ConfigureAwait(false);
                if (_searchAndList != null)
                {
                    _listFieldControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".List", cancellationToken);
                }

                if (_listFieldControl?.Tabs.Count > 0)
                {
                    _infoArea = _configurationService.GetInfoArea(_listFieldControl.InfoAreaId);
                    TableInfo tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                    List<FieldControlField> fields = _listFieldControl.Tabs[0].GetQueryFields();
                    var accField = fields.Where(a => a.Function.Equals("Acceptance")).FirstOrDefault();
                    var reqField = fields.Where(a => a.Function.Equals("Requirement")).FirstOrDefault();

                    if (accField != null)
                    {
                        accfieldInfo = await _configurationService.GetFieldInfo(tableInfo, accField, cancellationToken).ConfigureAwait(false);
                    }
                    if (accField != null)
                    {
                        reqfieldInfo = await _configurationService.GetFieldInfo(tableInfo, reqField, cancellationToken).ConfigureAwait(false);
                    }

                }
                var infoAreaID = _configurationService.GetInfoArea("ID");
                Requirements = await GetRequirementCatalogs(cancellationToken);

                foreach (var Participant in rows)
                {
                    var Rep = crmReps.Where(a => a.Id.Equals(Participant.RepIdString, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (Rep != null)
                    {
                        Participant.Name = Rep.Name;
                        if (accfieldInfo != null)
                        {
                            Participant.AcceptanceDisplayText = await _fieldGroupComponent.CatalogComponentObject.GetCatalogValue(accfieldInfo, Participant.AcceptanceText, cancellationToken).ConfigureAwait(false);

                        }
                        if (accfieldInfo != null)
                        {
                            Participant.Requirements = Requirements;
                            Participant.RequirementDisplayText = await _fieldGroupComponent.CatalogComponentObject.GetCatalogValue(reqfieldInfo, Participant.RequirementText, cancellationToken).ConfigureAwait(false);

                        }
                        Participant.LeftMarginColor = PageAccentColor(infoAreaID);
                        Participant.ImageSource = _imageResolverComponent.InoAreaImageTuple(infoAreaID, _configurationService);

                    }

                }
            }

            return rows;
        }

        public async Task<List<ParticipantData>> BuildLinkParticipants(PanelData data, CancellationToken cancellationToken)
        {
            List<ParticipantData> participants = new List<ParticipantData>();

            List<ListDisplayRow> records = new List<ListDisplayRow>();
            FieldControl _listFieldControl = null;

            var tabTypeParts = data.PanelTypeKey.Split('_');

            string linkParticipantsName = null;
            if (tabTypeParts.Length > 1)
            {
                linkParticipantsName = tabTypeParts[1];
                var configParts = linkParticipantsName.Split('#');
                if (configParts.Length > 1)
                {
                    linkParticipantsName = configParts[0];
                }
            }



            var _searchAndList = await _configurationService.GetSearchAndList(linkParticipantsName, cancellationToken).ConfigureAwait(false);
            if (_searchAndList != null)
            {
                _listFieldControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".List", cancellationToken);
            }

            if (_listFieldControl?.Tabs.Count > 0)
            {
                _ChildEditServic.FieldGroupName = await GetLinkParticiepentExpand(_searchAndList, cancellationToken);
                await _ChildEditServic.PrepareContentAsync(cancellationToken);
                string infoAreaId = _listFieldControl.InfoAreaId;
                _infoArea = _configurationService.GetInfoArea(_listFieldControl.InfoAreaId);
                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                List<FieldControlField> fields = _listFieldControl.Tabs[0].GetQueryFields();
                var accField = fields.Where(a => a.Function.Equals("Acceptance")).FirstOrDefault();
                var reqField = fields.Where(a => a.Function.Equals("Requirement")).FirstOrDefault();

                if (accField != null)
                {
                    accfieldInfo = await _configurationService.GetFieldInfo(tableInfo, accField, cancellationToken).ConfigureAwait(false);
                }

                if (accField != null)
                {
                    reqfieldInfo = await _configurationService.GetFieldInfo(tableInfo, reqField, cancellationToken).ConfigureAwait(false);
                }

                Requirements = await GetRequirementCatalogs(cancellationToken);
                Acceptance = await GetAcceptanceCatalogs(cancellationToken);
                _fieldGroupComponent.InitializeContext(_listFieldControl, tableInfo);
                if (!string.IsNullOrWhiteSpace(data.RecordId))
                {
                    if (_fieldGroupComponent.HasTabs())
                    {

                        ParentLink parentLink = new ParentLink
                        {
                            LinkId = -1,
                            ParentInfoAreaId = data.RecordInfoArea,
                            RecordId = data.RecordId,
                        };

                        var RawData = await _crmDataService.GetData(cancellationToken,
                            new DataRequestDetails
                            {
                                TableInfo = tableInfo,
                                Fields = fields,
                                SortFields = _listFieldControl.SortFields
                            },
                            parentLink, 100, RequestMode.Best);

                        if (RawData.Result != null)
                        {
                            var tasks = RawData.Result.Rows.Cast<DataRow>().Select(async row => await GetParticipantRow(fields, _fieldGroupComponent, row, cancellationToken));
                            var items = await Task.WhenAll(tasks).ConfigureAwait(false);
                            if (items != null && items.Length > 0)
                            {
                                participants.AddRange(items);
                            }
                        }
                    }

                }
            }

            return participants;
        }

        private async Task<string> GetLinkParticiepentExpand(SearchAndList searchAndList, CancellationToken cancellationToken)
        {
            string FieldGroupName = string.Empty;
            FieldControl fieldControl = await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".Edit", cancellationToken);
            if (await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".Edit", cancellationToken) != null ||
                await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".Details", cancellationToken) != null)
            {
                FieldGroupName = searchAndList.FieldGroupName;
            }
            else if (await _configurationService.GetFieldControl(searchAndList.InfoAreaId + ".Edit", cancellationToken) != null ||
                await _configurationService.GetFieldControl(searchAndList.InfoAreaId + ".Details", cancellationToken) != null)
            {
                FieldGroupName = searchAndList.InfoAreaId;
            }
            return FieldGroupName;
        }

        private async Task<ParticipantData> GetParticipantRow(List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, DataRow row, CancellationToken cancellationToken)
        {
            ParticipantData participant = null;
            if (row != null)
            {
                var recordId = row.GetColumnValue("recid", "-1");
                var requirementText = await row.GetColumnRawValue("Requirement", fieldDefinitions, fieldGroupComponent, cancellationToken);
                var acceptanceText = await row.GetColumnRawValue("Acceptance", fieldDefinitions, fieldGroupComponent, cancellationToken);

                (string name, string linkRecord, string linkInfoArea) = await GetPrticepantLinkDetails(row, fieldDefinitions, fieldGroupComponent, cancellationToken);
                participant = new ParticipantData(name, recordId, requirementText, acceptanceText, linkRecord, linkInfoArea);
                if (!string.IsNullOrEmpty(linkInfoArea))
                {
                    var infoArea = _configurationService.GetInfoArea(linkInfoArea);
                    participant.LeftMarginColor = PageAccentColor(infoArea);
                    participant.ImageSource = _imageResolverComponent.InoAreaImageTuple(infoArea, _configurationService);
                    participant.Acceptance = Acceptance;
                    participant.Requirements = Requirements;
                    participant.AllowDelete = true;
                    participant.LoadIntex();
                    var panel = await _ChildEditServic.GetPanelAsync(row, cancellationToken);
                    if (panel != null)
                    {
                        participant.Panels = new List<PanelData>();
                        participant.Panels.Add(panel);
                    }
                }

            }
            return participant;
        }

        private async Task<(string name, string linkRecord, string linkInfoArea)> GetPrticepantLinkDetails(DataRow row, List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, CancellationToken cancellationToken)
        {
            string name = string.Empty;
            string linkRecord = string.Empty;
            string linkInfoArea = string.Empty;
            Dictionary<string, List<FieldControlField>> infoAreaFields = new Dictionary<string, List<FieldControlField>>();
            foreach (var field in fieldDefinitions)
            {
                if (infoAreaFields.ContainsKey(field.InfoAreaId))
                {
                    infoAreaFields[field.InfoAreaId].Add(field);
                }
                else
                {
                    infoAreaFields.Add(field.InfoAreaId, new List<FieldControlField> { field });
                }
            }

            foreach (var infoareaid in infoAreaFields.Keys)
            {
                string linkFullRecordId = row.GetColumnValue($"{infoareaid}_0_recid", string.Empty);
                if (!string.IsNullOrWhiteSpace(linkFullRecordId))
                {
                    linkInfoArea = infoareaid;
                    linkRecord = linkFullRecordId.CleanRecordId();
                    StringBuilder stringName = new StringBuilder();
                    foreach (var field in infoAreaFields[infoareaid])
                    {
                        if (field != null)
                        {
                            var strFielddata = await fieldGroupComponent.ExtractFieldRawValue(field, row, cancellationToken);
                            if (!string.IsNullOrWhiteSpace(strFielddata) && !strFielddata.Equals(linkFullRecordId))
                            {
                                if (stringName.Length > 0)
                                {
                                    stringName.Append(" ");
                                }
                                stringName.Append(strFielddata);
                            }
                        }
                    }
                    name = stringName.ToString();
                    return (name, linkRecord, linkInfoArea);
                }
            }

            return (name, linkRecord, linkInfoArea);
        }

        protected List<ParticipantData> ParticipantsFromString(string dataString)
        {

            var participantStrings = dataString.Split(';');
            var participantArray = new List<ParticipantData>(participantStrings.Length);

            foreach (var participantString in participantStrings)
            {
                if (!string.IsNullOrEmpty(participantString))
                {
                    var particepentObj = new ParticipantData(participantString);
                    if(particepentObj != null)
                    {
                        var rep = crmReps.Where(x => x.Id.Equals(particepentObj.Key)).FirstOrDefault();
                        if (rep != null)
                        {
                            particepentObj.SetRepLinks(rep.RecordIdentification);
                            participantArray.Add(particepentObj);
                        }
                    }
                    
                }
            }

            return participantArray;
        }

        public async Task<List<PopupListItem>> GetRequirementCatalogs(CancellationToken cancellationToken)
        {
            if (reqfieldInfo != null)
            {
                var catalogItems = await _fieldGroupComponent.CatalogComponentObject.GetCatalogDisplayListAsync(reqfieldInfo, cancellationToken);

                if (catalogItems != null && catalogItems.Count > 0)
                {
                    var popupItems = new List<PopupListItem>();
                    foreach (var item in catalogItems)
                    {

                        popupItems.Add(new PopupListItem
                        {
                            RecordId = item.RecordId,
                            DisplayText = item.DisplayValue,
                            ImageSource = (string.Empty, item.IconValue),
                            OrginalObject = item
                        }
                        );
                    }

                    return popupItems;

                }
            }
            return null;
        }

        public async Task<List<PopupListItem>> GetAcceptanceCatalogs(CancellationToken cancellationToken)
        {
            if (accfieldInfo != null)
            {
                var catalogItems = await _fieldGroupComponent.CatalogComponentObject.GetCatalogDisplayListAsync(accfieldInfo, cancellationToken);

                if (catalogItems != null && catalogItems.Count > 0)
                {
                    var popupItems = new List<PopupListItem>();
                    foreach (var item in catalogItems)
                    {
                        popupItems.Add(new PopupListItem
                        {
                            RecordId = item.RecordId,
                            DisplayText = item.DisplayValue,
                            ImageSource = (string.Empty, item.IconValue),
                            OrginalObject = item
                        }
                        );
                    }

                    return popupItems;

                }
            }
            return null;
        }

        public async Task<List<SelectableFieldValue>> GetRepList(CancellationToken cancellationToken)
        {
            List<SelectableFieldValue> allowedValues = new List<SelectableFieldValue>();
            foreach (CrmRep rep in crmReps)
            {
                allowedValues.Add(new SelectableFieldValue
                {
                    RecordId = rep.Id,
                    DisplayValue = rep.Name
                });
            }
            return allowedValues;
        }

        public async Task<ParticipantData> GetParticipant(SelectableFieldValue selectedRep, CancellationToken token)
        {
            ParticipantData participant = null;
            if(selectedRep==null)
            {
                return participant;
            }
            var rep = crmReps.Where(r => r.Id == selectedRep.RecordId).FirstOrDefault();
            if (rep != null)
            {
                participant = new ParticipantData(rep);
                participant.Requirements = Requirements;
            }
            return participant;
        }

        public async Task<string> GetFieldText(ListDisplayField field, CancellationToken token)
        {
            var infoAreaID = field.Config.RecordSelectorAction?.ViewReference?.GetArgumentValue("InfoArea");
            if (!string.IsNullOrEmpty(infoAreaID))
            {
                var infoAreaobj =  await _configurationService.GetTableInfoAsync(infoAreaID,
                    token).ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(infoAreaobj.Name)? infoAreaID : infoAreaobj.Name;
            }
            return string.Empty;
        }
    }
}
