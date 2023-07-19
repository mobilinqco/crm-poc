using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.ContactTimes;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class ContactTimesContentService : ContentServiceBase, IContactTimesContentService
    {
        private readonly IOfflineRequestsService _offlineStoreService;

        private ActionTemplateBase _actionTemplate;
        private Expand _expand;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private string _infoAreaName = "CT";

        private List<string> _weekDayNames = new List<string>();

        private string _typeFieldName = "";
        private string _weekDayIdFieldName = "";
        private string _morningFromFieldName = "";
        private string _morningToFieldName = "";
        private string _afternoonFromFieldName = "";
        private string _afternoonToFieldName = "";

        private FieldInfo _typeFieldInfo;
        private FieldInfo _weekDayFieldInfo;
        private FieldInfo _morningFromFieldInfo;
        private FieldInfo _morningToFieldInfo;
        private FieldInfo _afternoonFromFieldInfo;
        private FieldInfo _afternoonToFieldInfo;

        private List<SelectableFieldValue> _typeSelectableFieldValues = new List<SelectableFieldValue>();

        private Dictionary<string, ContactTimesDay> _contactTimesDays = new Dictionary<string, ContactTimesDay>();

        private List<ContactTimesType> _contactTimesTypes = new List<ContactTimesType>();
        private Dictionary<string, ContactTimesType> _contactTimesTypesCodeDict = new Dictionary<string, ContactTimesType>();

        private List<ContactTimesDataGridEntry> _contactTimesDataGridEntries = new List<ContactTimesDataGridEntry>();

        private bool _hasData = false;
        public bool HasData
        {
            get => _hasData;
            set => _hasData = value;
        }

        public ContactTimesContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IOfflineRequestsService offlineRequestsService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineRequestsService;
        }

        public void SetWeekDayNames(List<string> weekDayNames)
        {
            _weekDayNames.AddRange(weekDayNames);
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            ViewReference vr = _action.ViewReference;
            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken);
            }

            if (vr.IsContactTimesEditAction())
            {
                await ProcessContactTimesEditActionTemplate(vr, cancellationToken);
            }
            else
            {
                await ProcessRecordViewActionTemplate(vr, cancellationToken);
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private async Task ProcessContactTimesEditActionTemplate(ViewReference viewReference, CancellationToken cancellationToken)
        {
            ContactTimesEditActionTemplate recordView = new ContactTimesEditActionTemplate(viewReference);
            _actionTemplate = recordView;

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(recordView.SearchList(), cancellationToken).ConfigureAwait(false);

            _infoArea = _configurationService.GetInfoArea(searchAndList.InfoAreaId);

            string infoAreaUnitName = searchAndList.InfoAreaId;
            if (_infoArea != null)
            {
                infoAreaUnitName = _infoArea.UnitName;
            }

            _fieldControl = await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".List", cancellationToken);

            _tableInfo = await _configurationService.GetTableInfoAsync(infoAreaUnitName, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                if (fieldDefinitions.Count > 0)
                {
                    await GetFieldNamesAndInfos(fieldDefinitions, cancellationToken);

                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: DetermineRequestMode(_actionTemplate));

                    InitiliseContactTimesDays();

                    BuildContactTimesTypes();
                }
            }
        }

        private async Task ProcessRecordViewActionTemplate(ViewReference viewReference, CancellationToken cancellationToken)
        {
            RecordViewTemplate recordView = new RecordViewTemplate(viewReference);
            _actionTemplate = recordView;

            _expand = _configurationService.GetExpand(_infoAreaName);
            _infoArea = _configurationService.GetInfoArea(_expand.InfoAreaId);
            _fieldControl = await _configurationService.GetFieldControl(_expand.FieldGroupName + ".List", cancellationToken);
            _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                if (fieldDefinitions.Count > 0)
                {
                    await GetFieldNamesAndInfos(fieldDefinitions, cancellationToken);

                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: DetermineRequestMode(_actionTemplate));

                    InitiliseContactTimesDays();

                    BuildContactTimesDataGridEntries();
                }
            }
        }

        private async Task GetFieldNamesAndInfos(List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {
                FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, field.FieldId);

                if (field.Function.ToLower().Equals("type"))
                {
                    _typeFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                    _typeFieldInfo = fieldInfo;
                    await GetContactTimesTypeCatalogs(fieldInfo, cancellationToken);
                }
                if (field.Function.ToLower().Equals("dayofweek"))
                {
                    _weekDayIdFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                    _weekDayFieldInfo = fieldInfo;
                }
                if (field.Function.ToLower().Equals("from"))
                {
                    _morningFromFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                    _morningFromFieldInfo = fieldInfo;
                }
                if (field.Function.ToLower().Equals("to"))
                {
                    _morningToFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                    _morningToFieldInfo = fieldInfo;
                }
                if (field.Function.ToLower().Equals("afternoonfrom"))
                {
                    _afternoonFromFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                    _afternoonFromFieldInfo = fieldInfo;
                }
                if (field.Function.ToLower().Equals("afternoonto"))
                {
                    _afternoonToFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                    _afternoonToFieldInfo = fieldInfo;
                }
            }
        }

        private async Task GetContactTimesTypeCatalogs(FieldInfo fieldInfo, CancellationToken cancellationToken)
        {
            _typeSelectableFieldValues.AddRange(await _fieldGroupComponent.ExtractAllowedCatalogValues(fieldInfo, cancellationToken));
        }

        private void InitiliseContactTimesDays()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
            {
                int weekDayIdOffset = 1;

                // Nasty solution to check if week day ids start from 0 or 1
                foreach (DataRow row in _rawData.Result.Rows)
                {
                    if (int.TryParse(row[_weekDayIdFieldName].ToString(), out int weekDayId) && weekDayId == 0)
                    {
                        weekDayIdOffset = 0;
                        break;
                    }
                }

                foreach (DataRow row in _rawData.Result.Rows)
                {
                    string recId = row["recid"].ToString();

                    string typeCode = row[_typeFieldName].ToString();

                    if (!int.TryParse(row[_weekDayIdFieldName].ToString(), out int weekDayId))
                    {
                        continue;
                    }

                    DateTime morningFrom = DateTime.ParseExact(row[_morningFromFieldName].ToString(), "HHmm", null);
                    DateTime morningTo = DateTime.ParseExact(row[_morningToFieldName].ToString(), "HHmm", null);
                    DateTime afternoonFrom = DateTime.ParseExact(row[_afternoonFromFieldName].ToString(), "HHmm", null);
                    DateTime afternoonTo = DateTime.ParseExact(row[_afternoonToFieldName].ToString(), "HHmm", null);

                    string key = string.Format("{0}_{1}", typeCode, _weekDayNames[weekDayId - weekDayIdOffset]);

                    if(!_contactTimesDays.ContainsKey(key))
                    {
                        _contactTimesDays[key] = new ContactTimesDay(recId, typeCode, weekDayId, _weekDayNames[weekDayId - weekDayIdOffset], morningFrom, morningTo, afternoonFrom, afternoonTo);
                    }
                }
            }
        }

        private void BuildContactTimesTypes()
        {
            foreach (SelectableFieldValue selectableFieldValue in _typeSelectableFieldValues)
            {
                List<ContactTimesDay> contactTimesDays = new List<ContactTimesDay>();

                for (int i = 0; i < _weekDayNames.Count; i++)
                {
                    string key = string.Format("{0}_{1}", selectableFieldValue.RecordId, _weekDayNames[i]);

                    if (!_contactTimesDays.ContainsKey(key))
                    {
                        contactTimesDays.Add(new ContactTimesDay(null, selectableFieldValue.RecordId, i + 1, _weekDayNames[i], DateTime.ParseExact("0000", "HHmm", null),
                            DateTime.ParseExact("0000", "HHmm", null), DateTime.ParseExact("0000", "HHmm", null), DateTime.ParseExact("0000", "HHmm", null)));
                    }
                    else
                    {
                        contactTimesDays.Add(_contactTimesDays[key]);
                    }
                }

                ContactTimesType contactTimesType = new ContactTimesType(selectableFieldValue.RecordId, selectableFieldValue.DisplayValue, contactTimesDays);

                _contactTimesTypes.Add(contactTimesType);
                _contactTimesTypesCodeDict[selectableFieldValue.RecordId] = contactTimesType;
            }
        }

        private void BuildContactTimesDataGridEntries()
        {
            for (int i = 0; i < _weekDayNames.Count; i++)
            {
                List<string> orderedContactTimesTypeNames = new List<string>();
                Dictionary<string, string> typeNameTimeIntervalsStringDict = new Dictionary<string, string>();

                foreach (SelectableFieldValue selectableFieldValue in _typeSelectableFieldValues)
                {
                    string key = string.Format("{0}_{1}", selectableFieldValue.RecordId, _weekDayNames[i]);

                    orderedContactTimesTypeNames.Add(selectableFieldValue.DisplayValue);

                    if (!_contactTimesDays.ContainsKey(key))
                    {
                        typeNameTimeIntervalsStringDict[selectableFieldValue.DisplayValue] = "-";
                    }
                    else
                    {
                        typeNameTimeIntervalsStringDict[selectableFieldValue.DisplayValue] = _contactTimesDays[key].GetTimeIntervalsString();
                        HasData = true;
                    }
                }

                _contactTimesDataGridEntries.Add(new ContactTimesDataGridEntry(_weekDayNames[i], i, orderedContactTimesTypeNames, typeNameTimeIntervalsStringDict));
            }
        }

        public List<ContactTimesType> GetContactTimesTypes()
        {
            List<ContactTimesType> contactTimesTypes = new List<ContactTimesType>();
            foreach(ContactTimesType contactTimesType in _contactTimesTypes)
            {
                contactTimesTypes.Add(contactTimesType.Clone());
            }
            return contactTimesTypes;
        }

        public ContactTimesIntervalSelectionData GetContactTimesIntervalSelectionData(ContactTimesDay selectedContactTimesDay)
        {
            if(_contactTimesTypesCodeDict.ContainsKey(selectedContactTimesDay.TypeCode))
            {
                ContactTimesType contactTimesType = _contactTimesTypesCodeDict[selectedContactTimesDay.TypeCode];

                List<ContactTimesDayAbbreviation> contactTimesDayAbbreviations = new List<ContactTimesDayAbbreviation>();

                for (int i = 0; i < contactTimesType.ContactTimesDays.Count; i++)
                {
                    // Also includes selectedContactTimesDay self IsEqual comparison
                    contactTimesDayAbbreviations.Add(new ContactTimesDayAbbreviation(contactTimesType.ContactTimesDays[i].WeekDayId, _weekDayNames[i],
                        selectedContactTimesDay.IsEqual(contactTimesType.ContactTimesDays[i])));
                }

                return new ContactTimesIntervalSelectionData(selectedContactTimesDay.TypeCode, contactTimesType.TypeName, contactTimesDayAbbreviations,
                    selectedContactTimesDay.MorningFromDateTime, selectedContactTimesDay.MorningToDateTime, selectedContactTimesDay.AfternoonFromDateTime,
                    selectedContactTimesDay.AfternoonToDateTime);
            }

            return null;
        }

        public void UpdateContactTimesTypeData(ContactTimesIntervalSelectionData contactTimesIntervalSelectionData)
        {
            if(_contactTimesTypesCodeDict.ContainsKey(contactTimesIntervalSelectionData.TypeCode))
            {
                ContactTimesType contactTimesType = _contactTimesTypesCodeDict[contactTimesIntervalSelectionData.TypeCode];

                foreach (ContactTimesDayAbbreviation contactTimesDayAbbreviation in contactTimesIntervalSelectionData.ContactTimesDayAbbreviations)
                {
                    if (contactTimesDayAbbreviation.IsSelected)
                    {
                        contactTimesType.ContactTimesDays[contactTimesDayAbbreviation.WeekDayId - 1].UpdateDateTimes(contactTimesIntervalSelectionData.MorningFromDateTime,
                            contactTimesIntervalSelectionData.MorningToDateTime, contactTimesIntervalSelectionData.AfternoonFromDateTime, contactTimesIntervalSelectionData.AfternoonToDateTime);
                    }
                }
            }
        }

        public void UpdateContactTimesDayData(ContactTimesDay contactTimesDay)
        {
            if (_contactTimesTypesCodeDict.ContainsKey(contactTimesDay.TypeCode))
            {
                ContactTimesType contactTimesType = _contactTimesTypesCodeDict[contactTimesDay.TypeCode];

                contactTimesType.ContactTimesDays[contactTimesDay.WeekDayId - 1].UpdateDateTimes(contactTimesDay.MorningFromDateTime,
                    contactTimesDay.MorningToDateTime, contactTimesDay.AfternoonFromDateTime, contactTimesDay.AfternoonToDateTime);
            }
        }

        public List<ContactTimesDataGridEntry> GetContactTimesDataGridDays()
        {
            return _contactTimesDataGridEntries;
        }

        public async Task<ModifyRecordResult> Save(ContactTimesDay contactTimesDay, CancellationToken cancellationToken)
        {
            PanelData panelData = new PanelData() { RecordId = contactTimesDay.RecordId };
            FieldControlTab panel = _fieldGroupComponent.FieldControl.Tabs[0];
            List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, null, null, panel.GetEditPanelType(), cancellationToken);

            Dictionary<int, string> currentFieldValues = BuildContactTimesDayCurrentFieldValues(contactTimesDay);
            Dictionary<int, string> oldFieldValues = BuildContactTimesDayOldFieldValues(contactTimesDay);

            if (outRow.Fields.Count > 0)
            {
                foreach (ListDisplayField listDisplayField in outRow.Fields)
                {
                    listDisplayField.EditData.ChangeOfflineRequest = new OfflineRecordField()
                    {
                        FieldId = listDisplayField.Config.FieldConfig.FieldId,
                        NewValue = currentFieldValues[listDisplayField.Config.FieldConfig.FieldId],
                        OldValue = oldFieldValues[listDisplayField.Config.FieldConfig.FieldId],
                        Offline = 0
                    };
                    listDisplayField.Data.StringData = oldFieldValues[listDisplayField.Config.FieldConfig.FieldId];
                    listDisplayField.EditData.DefaultStringValue = oldFieldValues[listDisplayField.Config.FieldConfig.FieldId];
                    listDisplayField.EditData.StringValue = currentFieldValues[listDisplayField.Config.FieldConfig.FieldId];
                }
                panelData.Fields = outRow.Fields;
            }

            List<PanelData> inputPanels = new List<PanelData>() { panelData };

            List<OfflineRecordLink> recordLinks = new List<OfflineRecordLink>();

            string parentLinkId = _action.GetLinkId();

            if (!int.TryParse(parentLinkId, out int intParentLinkId))
            {
                intParentLinkId = 0;
            }

            var parentLinkInfo = _tableInfo.GetLinkInfo(_action.SourceInfoArea, intParentLinkId);

            if (parentLinkInfo != null)
            {
                var parentLink = new OfflineRecordLink()
                {
                    InfoAreaId = _action.SourceInfoArea,
                    LinkId = parentLinkInfo.LinkId,
                    RecordId = _action.RecordId
                };

                recordLinks.Add(parentLink);
            }
            else
            {
                recordLinks.Add(_action.GetLinkRequest());
            }

            OfflineRequest offlineRequest;

            if (contactTimesDay.RecordId == null)
            {
                offlineRequest = await _offlineStoreService.CreateSaveRequest(_actionTemplate, _fieldGroupComponent.FieldControl, _tableInfo, inputPanels, null, recordLinks, cancellationToken);
            }
            else
            {
                offlineRequest = await _offlineStoreService.CreateUpdateRequest(_actionTemplate, _fieldGroupComponent.FieldControl, inputPanels, new List<string> { contactTimesDay.RecordId }, cancellationToken);
            }


            ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

            if (!modifyRecordResult.HasSaveErrors())
            {
                if (contactTimesDay.RecordId == null)
                {
                    _logService.LogDebug("New record has been saved.");
                }
                else
                {
                    _logService.LogDebug("Records has been updated.");
                }

                await _offlineStoreService.Delete(offlineRequest, cancellationToken);
            }
            else
            {
                await _offlineStoreService.Update(offlineRequest, cancellationToken);
                throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
            }

            return modifyRecordResult;
        }

        private Dictionary<int, string> BuildContactTimesDayCurrentFieldValues(ContactTimesDay contactTimesDay)
        {
            Dictionary<int, string> currentFieldValues = new Dictionary<int, string>
            {
                [_typeFieldInfo.FieldId] = contactTimesDay.TypeCode,
                [_weekDayFieldInfo.FieldId] = contactTimesDay.WeekDayId.ToString(),
                [_morningFromFieldInfo.FieldId] = contactTimesDay.MorningFromDateTime.ToString("HHmm"),
                [_morningToFieldInfo.FieldId] = contactTimesDay.MorningToDateTime.ToString("HHmm"),
                [_afternoonFromFieldInfo.FieldId] = contactTimesDay.AfternoonFromDateTime.ToString("HHmm"),
                [_afternoonToFieldInfo.FieldId] = contactTimesDay.AfternoonToDateTime.ToString("HHmm")
            };

            return currentFieldValues;
        }

        private Dictionary<int, string> BuildContactTimesDayOldFieldValues(ContactTimesDay contactTimesDay)
        {
            Dictionary<int, string> oldFieldValues = new Dictionary<int, string>
            {
                [_typeFieldInfo.FieldId] = contactTimesDay.TypeCode,
                [_weekDayFieldInfo.FieldId] = contactTimesDay.WeekDayId.ToString(),
                [_morningFromFieldInfo.FieldId] = contactTimesDay.MorningFromDateTime.ToString("HHmm"),
                [_morningToFieldInfo.FieldId] = contactTimesDay.MorningToDateTime.ToString("HHmm"),
                [_afternoonFromFieldInfo.FieldId] = contactTimesDay.AfternoonFromDateTime.ToString("HHmm"),
                [_afternoonToFieldInfo.FieldId] = contactTimesDay.AfternoonToDateTime.ToString("HHmm")
            };

            return oldFieldValues;
        }

        public async Task<ModifyRecordResult> Delete(string recordId, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = await _offlineStoreService.CreateDeleteRequest(_actionTemplate, _fieldControl.InfoAreaId, recordId);

            ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

            if (!modifyRecordResult.HasSaveErrors())
            {
                _logService.LogDebug("Record has been deleted.");
                await _offlineStoreService.Delete(offlineRequest, cancellationToken);
            }
            else
            {
                await _offlineStoreService.Update(offlineRequest, cancellationToken);
                throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
            }

            return modifyRecordResult;
        }
    }
}
