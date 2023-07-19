using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using Newtonsoft.Json;

namespace ACRM.mobile.Services.SubComponents
{
    public class FieldGroupComponent
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogService _logService;
        private readonly IRepService _repService;
        private readonly CatalogComponent _catalogComponent;
        private readonly FieldDataProcessor _fieldDataProcessor;

        private FieldControl _fieldControl;
        private TableInfo _tableInfo;
        private string _serverTimeZone;

        public FieldControl FieldControl
        {
            get
            {
                return _fieldControl;
            }
        }

        public CatalogComponent CatalogComponentObject
        {
            get
            {
                return _catalogComponent;
            }
        }

        public IRepService RepServiceobj
        {
            get
            {
                return _repService;
            }
        }

        public TableInfo TableInfo
        {
            get
            {
                return _tableInfo;
            }
        }

        public FieldGroupComponent(IConfigurationService configurationService,
            CatalogComponent catalogComponent,
            FieldDataProcessor fieldDataProcessor,
            IRepService repService,
            ILogService logService)
        {
            _configurationService = configurationService;
            _fieldDataProcessor = fieldDataProcessor;
            _catalogComponent = catalogComponent;
            _logService = logService;
            _repService = repService;
        }

        public void InitializeContext(FieldControl fieldControl, TableInfo tableInfo)
        {
            _fieldControl = fieldControl;
            _tableInfo = tableInfo;
            _serverTimeZone = _configurationService.GetServerTimezone();
        }

        public bool HasTabs()
        {
            return _fieldControl != null && FieldControl.Tabs != null && FieldControl.Tabs.Count > 0;
        }

        public async Task<DeviceCalendarEvent> GetCalendarEvent(List<FieldControlField> fieldDefinitions, DataRow row, CancellationToken cancellationToken)
        {
            DeviceCalendarEvent dce = new DeviceCalendarEvent();
            dce.RecordId = row.GetColumnValue("recid", "-1");
            dce.CalendarId = "CrmCalendar";
            _logService.LogDebug($"Start Extract Calendar Event for RecordId {dce.RecordId}");

            string startDate = string.Empty;
            string endDate = string.Empty;

            foreach (FieldControlField fd in fieldDefinitions)
            {
                if (!string.IsNullOrEmpty(fd.Function))
                {
                    FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fd, cancellationToken).ConfigureAwait(false);
                    if (fieldInfo != null)
                    {
                        PresentationFieldAttributes pfa = new PresentationFieldAttributes(fd, fieldInfo, _serverTimeZone, EditModes.DetailsOrAll);
                        string fieldName = fd.QueryFieldName(fd.InfoAreaId != _tableInfo.InfoAreaId);
                        string fieldValue = await _fieldDataProcessor.ExtractDisplayValue(row, fieldInfo, pfa, fieldName, cancellationToken).ConfigureAwait(false);
                        AssignValueToEventData(dce, ref startDate, ref endDate, fd, fieldValue);
                    }
                }
            }

            // Added some guards for the start and end date because in case the 
            // dates are not correctly configured in the FieldControl we can end up with 
            // strange performance problems and unexpected crashes
            if (string.IsNullOrWhiteSpace(startDate))
            {
                dce.StartDate = DateTime.Now;
            }
            else
            {
                dce.StartDate = CrmDateStrToDateTime(startDate);
            }

            if (string.IsNullOrWhiteSpace(endDate))
            {
                dce.EndDate = dce.StartDate.AddHours(1);
            }
            else 
            { 
                dce.EndDate = CrmDateStrToDateTime(endDate);
            }

            dce.Color = "#ff0000ff";
            dce.IsCrmEvent = true;

            return dce;
        }

        public async Task<FieldInfo> GetCalendarEventStartDateField(List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            foreach (FieldControlField fd in fieldDefinitions)
            {
                if (!string.IsNullOrEmpty(fd.Function) && fd.Function.Equals("Date"))
                {
                    return await _configurationService.GetFieldInfo(_tableInfo, fd, cancellationToken).ConfigureAwait(false);
                }
            }

            return null;
        }

        private static void AssignValueToEventData(DeviceCalendarEvent dce, ref string startDate, ref string endDate, FieldControlField fd, string fieldValue)
        {
            switch (fd.Function)
            {
                case "Date":
                    startDate = fieldValue + startDate;
                    break;
                case "Time":
                    startDate = startDate + " " + fieldValue;
                    break;
                case "EndDate":
                    endDate = fieldValue + endDate;
                    break;
                case "EndTime":
                    endDate = endDate + " " + fieldValue;
                    break;
                case "Subject":
                    dce.Title = fieldValue;
                    break;
                case "Status":
                    dce.SetEventStatus(fieldValue);
                    break;

                case "PersonLabel":
                    dce.PersonLabel = fieldValue;
                    break;
                case "CompanyLabel":
                    dce.CompanyLabel = fieldValue;
                    break;
                case "RepLabel":
                    dce.RepLabel = fieldValue;
                    break;
                case "RepId":
                    dce.RepId = fieldValue;
                    break;
                case "Type":
                    dce.Type = fieldValue;
                    break;
                default:
                    break;
            }
        }

        internal async Task<string> ExtractFieldRawValue(FieldControlField fieldDefinitions, DataRow row, CancellationToken cancellationToken)
        {
            string fieldName = fieldDefinitions.QueryFieldName(fieldDefinitions.InfoAreaId != _tableInfo.InfoAreaId);
            string fieldValue = row.GetColumnValue(fieldName);
            return fieldValue;
        }

        private DateTime CrmDateStrToDateTime(string dateTimeStr)
        {
            DateTime dateValue;
            CultureInfo enUS = new CultureInfo("en-US");
            if (!string.IsNullOrEmpty(dateTimeStr))
            {
                if (DateTime.TryParseExact(dateTimeStr, "yyyyMMdd HHmm", enUS, DateTimeStyles.None, out dateValue))
                {
                    return dateValue.InClientTimeZone(_serverTimeZone);
                }
            }

            return DateTime.MinValue;
        }


        public async Task<ListDisplayRow> ExtractDisplayRow(List<FieldControlField> fieldDefinitions, DataRow row, bool areSectionsEnabled, CancellationToken cancellationToken)
        {
            ListDisplayRow outRow = new ListDisplayRow();
            bool isSectionFieldPopulated = false;
            outRow.RecordId = row.GetColumnValue("recid", "-1");
            outRow.InfoAreaId = _tableInfo?.InfoAreaId;
            outRow.RawInfoAreaId = row.GetRawInfoArea();
            _logService.LogDebug($"Start ExtractDisplayRow for RecordId {outRow.RecordId}");
            ListDisplayField currentLdf = null;
            ListDisplayField lastDateLdf = null;

            foreach (FieldControlField fd in fieldDefinitions)
            {
                FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fd, cancellationToken).ConfigureAwait(false);

                if (fieldInfo != null)
                {
                    PresentationFieldAttributes pfa = new PresentationFieldAttributes(fd, fieldInfo, _serverTimeZone, EditModes.DetailsOrAll);
                    if (!fd.HasImageAttribute())
                    { 
                        bool isLinkedField = fd.InfoAreaId != _tableInfo.InfoAreaId;
                        string fieldName = fd.QueryFieldName(isLinkedField);
                        string fieldValue = await _fieldDataProcessor.ExtractDisplayValue(row, fieldInfo, pfa, fieldName, cancellationToken).ConfigureAwait(false);
                        CrmFieldLinkData linkedData = null;



                        if (areSectionsEnabled && pfa.IsSectionField() && !isSectionFieldPopulated)
                        {
                            outRow.SectionGroupingValue = CreateListDisplayField(fd, pfa, fieldValue, linkedData);
                            isSectionFieldPopulated = true;
                        }

                        if (isLinkedField)
                        {
                            linkedData = ExtractLinkedData(row, fd);
                        }

                        if (ParseColspan(pfa))
                        {
                            ListDisplayField ldf = CreateListDisplayField(fd, pfa, fieldValue, linkedData);

                            ldf.ColspanCreationFieldCounter = pfa.FieldCount;
                            ldf.parentLdf = currentLdf;
                            currentLdf = ldf;
                        }

                        if (currentLdf == null)
                        {
                            ListDisplayField ldf = new ListDisplayField
                            {
                                Data = new CrmFieldDisplayData { StringData = fieldValue, LinkedFieldData = linkedData },
                                Config = new CrmFieldConfiguration((FieldControlField)fd.Clone(), (PresentationFieldAttributes)pfa.Clone())
                            };
                            outRow.Fields.Add(ldf);
                        }
                        else
                        {
                            if (currentLdf.ColspanCreationFieldCounter > 0)
                            {
                                ListDisplayField ldf = new ListDisplayField
                                {
                                    Data = new CrmFieldDisplayData { StringData = fieldValue, LinkedFieldData = linkedData },
                                    Config = new CrmFieldConfiguration((FieldControlField)fd.Clone(), (PresentationFieldAttributes)pfa.Clone()),
                                };
                                currentLdf.Data.ColspanData.Add(ldf);
                                currentLdf.ColspanCreationFieldCounter--;
                            }

                            currentLdf = SetCurrentLdf(outRow, currentLdf);
                        }

                        if (pfa.IsTime && lastDateLdf != null)
                        {
                            lastDateLdf.Config.PresentationFieldAttributes.RelatedTimeValue = fieldValue;
                            if (lastDateLdf.Data?.ColspanData?.Count > 0)
                            {
                                lastDateLdf.Data.ColspanData[0].Config.PresentationFieldAttributes.RelatedTimeValue = fieldValue;
                            }
                        }

                        if (pfa.IsDate)
                        {
                            lastDateLdf = currentLdf;
                        }
                        else
                        {
                            lastDateLdf = null;
                        }
                    }
                }
            }

            _logService.LogDebug($"End ExtractDisplayRow for RecordId {outRow.RecordId}");
            return outRow;
        }

        private static ListDisplayField CreateListDisplayField(FieldControlField fd, PresentationFieldAttributes pfa, string fieldValue, CrmFieldLinkData linkedData)
        {
            return new ListDisplayField
            {
                Data = new CrmFieldDisplayData
                {
                    ColspanData = new List<ListDisplayField>(),
                    LinkedFieldData = linkedData,
                    StringData = fieldValue
                },
                Config = new CrmFieldConfiguration((FieldControlField)fd.Clone(), (PresentationFieldAttributes)pfa.Clone()),
                ColspanCreationFieldCounter = pfa.FieldCount
            };
        }

        private static ListDisplayField SetCurrentLdf(ListDisplayRow outRow, ListDisplayField currentLdf)
        {
            if (currentLdf.ColspanCreationFieldCounter == 0)
            {
                if (currentLdf.parentLdf == null)
                {
                    outRow.Fields.Add(currentLdf);
                    currentLdf = null;
                }
                else
                {
                    currentLdf.parentLdf.Data.ColspanData.Add(currentLdf);
                    currentLdf.parentLdf.ColspanCreationFieldCounter--;
                    currentLdf = SetCurrentLdf(outRow, currentLdf.parentLdf);

                }
            }

            return currentLdf;
        }

        public async Task<string> ExtractFieldValue(FieldControlField fieldDefinitions, DataRow row, CancellationToken cancellationToken)
        {
            var fieldValue = string.Empty;

            _logService.LogDebug($"Start ExtractFieldValue for FieldId {fieldDefinitions?.FieldId}");

            if (fieldDefinitions != null)
            {
                FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fieldDefinitions, cancellationToken).ConfigureAwait(false);

                if (fieldInfo != null)
                {
                    PresentationFieldAttributes pfa = new PresentationFieldAttributes(fieldDefinitions, fieldInfo, _serverTimeZone, EditModes.DetailsOrAll);
                    if (fieldInfo != null && !fieldDefinitions.HasImageAttribute())
                    {
                        string fieldName = fieldDefinitions.QueryFieldName(fieldDefinitions.InfoAreaId != _tableInfo.InfoAreaId);
                        fieldValue = await _fieldDataProcessor.ExtractDisplayValue(row, fieldInfo, pfa, fieldName, cancellationToken).ConfigureAwait(false);

                    }
                }
            }
            _logService.LogDebug($"End ExtractFieldValue for FieldId {fieldDefinitions?.FieldId}");

            return fieldValue;
        }

        public async Task<string> QueryFieldName(FieldControlField fieldDefinitions, CancellationToken cancellationToken)
        {
            var fieldName = string.Empty;

            _logService.LogDebug($"Start ExtractFieldValue for FieldId {fieldDefinitions?.FieldId}");

            if (fieldDefinitions != null)
            {
                FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fieldDefinitions, cancellationToken).ConfigureAwait(false);

                if (fieldInfo != null)
                {
                    PresentationFieldAttributes pfa = new PresentationFieldAttributes(fieldDefinitions, fieldInfo, _serverTimeZone, EditModes.DetailsOrAll);
                    if (fieldInfo != null && !fieldDefinitions.HasImageAttribute())
                    {
                        return fieldDefinitions.QueryFieldName(fieldDefinitions.InfoAreaId != _tableInfo.InfoAreaId);

                    }
                }
            }
            _logService.LogDebug($"End ExtractField Query Name for FieldId {fieldDefinitions?.FieldId}");

            return fieldName;
        }


        private CrmFieldLinkData ExtractLinkedData(DataRow row, FieldControlField fd)
        {
            string linkedRecId = fd.QueryLinkedRecordIdName();
            string recId = row.GetColumnValue(linkedRecId);
            if(!string.IsNullOrWhiteSpace(recId))
            {
                return new CrmFieldLinkData { InfoAreaId = fd.InfoAreaId, RecordId = recId };
            }

            return new CrmFieldLinkData { IsIncomplete = true };
        }

        private async Task<CrmFieldEditData> ExtractEditValue(DataRow row, Dictionary<string, string> templateFilterValues, FieldInfo fieldInfo, PresentationFieldAttributes pfa, string fieldName, CancellationToken cancellationToken)
        {
            CrmFieldEditData editField = new CrmFieldEditData();

            string fieldValue = row.GetColumnValue(fieldName);
            string displayValue = string.Empty;

            if (templateFilterValues != null && templateFilterValues.ContainsKey($"{fieldInfo.TableInfoInfoAreaId}_{fieldInfo.FieldId}"))
            {
                fieldValue = templateFilterValues[$"{fieldInfo.TableInfoInfoAreaId}_{fieldInfo.FieldId}"];
                editField.wasSetByFilter = !string.IsNullOrWhiteSpace(fieldValue);
            }

            editField.RawValue = fieldValue;

            if (editField.wasSetByFilter && (fieldInfo.IsDate || fieldInfo.IsTime))
            {
                editField.RawValue = fieldValue.Replace("-", "").Replace(":", "");
            }

            if (fieldInfo.IsCatalog)
            {
                
                if (row != null)
                {
                    displayValue = await _catalogComponent.GetStringValueForCatalogField(row, fieldName, fieldInfo, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    displayValue = await _catalogComponent.GetStringValueForCatalogField(fieldValue, fieldName, fieldInfo, cancellationToken).ConfigureAwait(false);
                }
                if (!string.IsNullOrEmpty(displayValue))
                {
                    editField.DefaultSelectedValue = new SelectableFieldValue { DisplayValue = displayValue, RecordId = fieldValue };
                }
                return editField;
            }

            if (!string.IsNullOrWhiteSpace(fieldInfo.RepMode))
            {
                if (!fieldInfo.IsParticipant)
                {
                    displayValue = await RepServiceobj.GetRepName(fieldValue, cancellationToken).ConfigureAwait(false);
                    editField.DefaultSelectedValue = new SelectableFieldValue { DisplayValue = displayValue, RecordId = UtilityExtensions.FormattedRepId(fieldValue) };
                    return editField;
                }
            }

            if (fieldInfo.IsBoolean)
            {
                displayValue = _fieldDataProcessor.ResolveBoolValue(fieldValue, fieldInfo, pfa);
                editField.DefaultSelectedValue = new SelectableFieldValue { DisplayValue = displayValue, RecordId = fieldValue };
                return editField;
            }

            if (fieldInfo.IsDate && row != null && !string.IsNullOrEmpty(fieldValue) && fieldValue.Count() == 8)
            {
                editField.DefaultStringValue = $"{fieldValue.Substring(0, 4)}-{fieldValue.Substring(4, 2)}-{fieldValue.Substring(6, 2)}";
                return editField;
            }

            if (fieldInfo.IsTime && row != null && !string.IsNullOrEmpty(fieldValue) && fieldValue.Count() == 4)
            {
                editField.DefaultStringValue = $"{fieldValue.Substring(0, 2)}:{fieldValue.Substring(2, 2)}";
                return editField;
            }

            if (fieldInfo.IsPercent
                && fieldInfo.IsReal)
            {
                if (!string.IsNullOrWhiteSpace(fieldValue)
                    && double.TryParse(fieldValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double result))
                {
                    if (result < 0)
                    {
                        result = result * 100;
                    }

                    editField.DefaultStringValue = $"{result}"; ;
                    return editField;
                }
            }


            editField.DefaultStringValue = fieldValue;
            return editField;
        }

        public async Task<List<SelectableFieldValue>> ExtractAllowedCatalogValues(FieldInfo fieldInfo, CancellationToken cancellationToken)
        {
            return await _catalogComponent.GetCatalogDisplayListAsync(fieldInfo, cancellationToken).ConfigureAwait(false);
        }

        private async Task<List<SelectableFieldValue>> ExtractAllowedValues(FieldInfo fieldInfo, CancellationToken cancellationToken)
        {
            if (fieldInfo.IsCatalog)
            {
                return await ExtractAllowedCatalogValues(fieldInfo, cancellationToken).ConfigureAwait(false);
            }

            List<SelectableFieldValue> allowedValues = new List<SelectableFieldValue>();
            if (!string.IsNullOrWhiteSpace(fieldInfo.RepMode))
            {
                List<CrmRep> crmReps = await _repService.GetAllCrmReps(cancellationToken).ConfigureAwait(false);
                foreach(CrmRep rep in crmReps)
                {
                    allowedValues.Add(new SelectableFieldValue { RecordId = rep.Id, DisplayValue = rep.Name });
                }
            }

            return allowedValues;
        }

        private async Task<UserAction> ExtractRecordSelector(FieldInfo fieldInfo, PresentationFieldAttributes pfa, CancellationToken cancellationToken)
        {
            if (pfa.HasSelectFunction())
            {
                RecordSelector recordSelector = null;
                Menu recordSelectorMenuAction = null;

                string selectFunction = pfa.SelectFunction();

                if (selectFunction.Trim(' ').StartsWith("{"))
                {
                    try
                    {
                        recordSelector = JsonConvert.DeserializeObject<RecordSelector>(selectFunction);
                    }
                    catch (Exception e)
                    {
                        _logService.LogDebug($"Error converting select function {e}. Select function content: {selectFunction}");
                    }
                }

                if (recordSelector == null)
                {
                    recordSelectorMenuAction = await _configurationService.GetMenu(selectFunction, cancellationToken).ConfigureAwait(false);
                }
                else if (!string.IsNullOrEmpty(recordSelector.ContextMenu))
                {
                    recordSelectorMenuAction = await _configurationService.GetMenu(recordSelector.ContextMenu, cancellationToken).ConfigureAwait(false);
                }

                if ((recordSelector != null && recordSelector.ShouldUserRecordSelectorView())
                    || (recordSelectorMenuAction != null
                        && recordSelectorMenuAction.ViewReference != null))
                {
                    RecordSelectorTemplate rs = new RecordSelectorTemplate(recordSelector,
                        recordSelectorMenuAction != null ? recordSelectorMenuAction.ViewReference : null);

                    return new UserAction
                    {
                        ActionDisplayName = "RecordSelector",
                        ActionType = UserActionType.RecordSelector,
                        RecordSelector = rs,
                        ViewReference = recordSelectorMenuAction != null ? recordSelectorMenuAction.ViewReference : null
                    };
                }
            }

            return null;
        }

        public async Task<ListDisplayRow> ExtractEditRow(List<FieldControlField> fieldDefinitions, DataRow row,
            Dictionary<string, string> templateFilterValues, PanelType panelType, CancellationToken cancellationToken)
        {
            return await ExtractEditRow(fieldDefinitions, row, templateFilterValues, string.Empty, string.Empty, panelType, cancellationToken);
        }

        public async Task<ListDisplayRow> ExtractEditRow(List<FieldControlField> fieldDefinitions, DataRow row,
          Dictionary<string, string> templateFilterValues, string parentInfoArea, string parentRecordId, PanelType panelType, CancellationToken cancellationToken)
        {
            ListDisplayRow outRow = new ListDisplayRow();

            int colspan = 0;
            List<ListDisplayField> colspanValues = new List<ListDisplayField>();
            PresentationFieldAttributes colspanPfa = null;
            FieldControlField colspanFd = null;
            UserAction colspanRsa = null;

            foreach (FieldControlField fd in fieldDefinitions)
            {
                FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fd, cancellationToken).ConfigureAwait(false);

                if (fieldInfo != null)
                {
                    PresentationFieldAttributes pfa = new PresentationFieldAttributes(fd,
                        fieldInfo,
                        _serverTimeZone,
                        row != null ? EditModes.Update : EditModes.New);

                    string fieldName = fd.QueryFieldName(fd.InfoAreaId != _tableInfo.InfoAreaId);
                    CrmFieldEditData fieldEditData = await ExtractEditValue(row, templateFilterValues, fieldInfo, pfa, fieldName, cancellationToken).ConfigureAwait(false);
                    string fieldValue = fieldEditData.DefaultSelectedValue != null ? fieldEditData.DefaultSelectedValue.DisplayValue : fieldEditData.DefaultStringValue;
                    List<SelectableFieldValue> allowedValues = await ExtractAllowedValues(fieldInfo, cancellationToken).ConfigureAwait(false);
                    UserAction recordSelectorAction = await ExtractRecordSelector(fieldInfo, pfa, cancellationToken).ConfigureAwait(false);

                    if (fd.InfoAreaId == _tableInfo.InfoAreaId || recordSelectorAction != null || colspan > 0 || panelType == PanelType.EditPanelChildren)
                    {
                        if (colspan == 0 && ParseColspan(pfa))
                        {
                            colspan = pfa.FieldCount;
                            colspanPfa = pfa;
                            colspanFd = fd;
                            colspanRsa = recordSelectorAction;
                            colspanValues.Clear();
                        }

                        if (colspan > 0)
                        {
                            ListDisplayField ldf = new ListDisplayField
                            {
                                Data = new CrmFieldDisplayData { StringData = fieldValue },
                                EditData = fieldEditData,
                                Config = new CrmFieldConfiguration((FieldControlField)fd.Clone(), (PresentationFieldAttributes)pfa.Clone(),
                                            allowedValues, recordSelectorAction)
                            };

                            if(colspanValues.Count>0 && ldf.Config.PresentationFieldAttributes.IsTime)
                            {
                                if (colspanValues[0].Config.PresentationFieldAttributes.IsDate)
                                {
                                    colspanValues[0].Config.PresentationFieldAttributes.RelatedTimeValue = ldf.EditData.RawValue;
                                }
                            }

                            colspanValues.Add(ldf);
                            colspan--;
                        }


                        if (colspan == 0)
                        {
                            if (colspanPfa == null)
                            {
                                ListDisplayField ldf = new ListDisplayField
                                {
                                    Data = new CrmFieldDisplayData { StringData = fieldValue },
                                    EditData = fieldEditData,
                                    Config = new CrmFieldConfiguration((FieldControlField)fd.Clone(), (PresentationFieldAttributes)pfa.Clone(),
                                                allowedValues, recordSelectorAction)
                                };
                                outRow.Fields.Add(ldf);
                            }
                            else
                            {
                                ListDisplayField ldf = new ListDisplayField
                                {
                                    Data = new CrmFieldDisplayData { ColspanData = new List<ListDisplayField>(colspanValues) },
                                    EditData = fieldEditData,
                                    Config = new CrmFieldConfiguration(colspanFd, colspanPfa,
                                                allowedValues, colspanRsa)
                                };
                                outRow.Fields.Add(ldf);
                                colspanPfa = null;
                                colspanFd = null;
                                colspanRsa = null;
                                colspanValues.Clear();
                            }
                        }
                    }
                }
            }

            return outRow;
        }

        public FieldControlTab OrganizerHeaderSubLabel()
        {
            foreach (FieldControlTab tab in _fieldControl.Tabs)
            {
                if (tab.GetPanelType() == PanelType.OrganizerHeaderSubLabel)
                {
                    return tab;
                }
            }
            return null;
        }

        public FieldControlField HeaderImageField()
        {
            foreach (FieldControlTab tab in _fieldControl.Tabs)
            {
                foreach (FieldControlField field in tab.Fields)
                {
                    if (field.HasImageAttribute())
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public List<FieldControlField> GetQueryFields(List<FieldControlTab> controlTabs)
        {
            List<FieldControlField> fields = new List<FieldControlField>();

            foreach (FieldControlTab tab in controlTabs)
            {
                fields.AddRange(tab.Fields);
            }

            return fields;
        }

        private bool ParseColspan(PresentationFieldAttributes pfa)
        {
            if (pfa.HasColspan)
            {
                try
                {
                    pfa.ParseColspan();
                    return true;
                }
                catch (Exception e)
                {
                    _logService.LogError($"{"Unable to parse Colspan field attribute: " + pfa.RawColspanValue + ". Error: " + e.GetType().Name + " : " + e.Message}");
                }
            }
            return false;
        }


        public Dictionary<string, FieldControlField> GetFieldsFunctions(List<FieldControlTab> controlTabs)
        {
            Dictionary<string, FieldControlField> functions = new Dictionary<string, FieldControlField>();

            foreach (FieldControlTab tab in controlTabs)
            {
                foreach(FieldControlField field in tab.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(field.Function))
                    {
                        if (!functions.ContainsKey(field.Function))
                        {
                            functions.Add(field.Function, field);
                        }
                    }
                }
            }

            return functions;
        }


        public async Task<Dictionary<string, string>> ExtractDictionary(List<FieldControlField> fieldDefinitions, DataRow row, CancellationToken cancellationToken)
        {
            Dictionary<string, string> functionNameValues = new Dictionary<string, string>();
            try
            {
                foreach (FieldControlField fd in fieldDefinitions)
                {
                    if (!string.IsNullOrWhiteSpace(fd.Function))
                    {
                        FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fd, cancellationToken).ConfigureAwait(false);

                        if (fieldInfo != null)
                        {
                            string fieldName = fd.QueryFieldName(fd.InfoAreaId != _tableInfo.InfoAreaId);
                            string fieldValue = row.GetColumnValue(fieldName);
                            functionNameValues[fd.Function] = fieldValue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logService.LogError($"There was an error extracting to a dictionary: {e.Message}");
            }

            return functionNameValues;
        }

        public string GetClientFilterName(bool isNew)
        {
            string filterName = string.Empty;

            if (isNew)
            {
                filterName = FieldControl?.Attributes.Where(a => a.Key.Equals("ClientFilterForNew")).FirstOrDefault()?.Value;
            }
            else
            {
                filterName = FieldControl?.Attributes.Where(a => a.Key.Equals("ClientFilterForUpdate")).FirstOrDefault()?.Value;
            }

            if (string.IsNullOrWhiteSpace(filterName))
            {
                filterName = FieldControl?.Attributes.Where(a => a.Key.Equals("ClientFilter")).FirstOrDefault()?.Value;
            }

            return filterName;
        }

        public string GetEditTriggerFilter(bool isNew)
        {
            string filterName = string.Empty;

            if (isNew)
            {
                filterName = FieldControl?.Attributes.Where(a => a.Key.Equals("EditTriggerFilterForNew")).FirstOrDefault()?.Value;
            }
            else
            {
                filterName = FieldControl?.Attributes.Where(a => a.Key.Equals("EditTriggerFilterForUpdate")).FirstOrDefault()?.Value;
            }

            if (string.IsNullOrWhiteSpace(filterName))
            {
                filterName = FieldControl?.Attributes.Where(a => a.Key.Equals("EditTriggerFilter")).FirstOrDefault()?.Value;
            }

            return filterName;
        }
    }
}
