using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class NewOrEditService : ContentServiceBase, INewOrEditService
    {
        protected ActionTemplateBase _template;
        protected IOfflineRequestsService _offlineStoreService;
        protected IFieldGroupDataService _fieldGroupDataService;
        protected IJSProcessor _jsProcessor;
        
        protected Filter _templateFilter;
        protected bool _isNewMode = true;
        protected bool _isInBackground = false;
        protected bool _executeTriggersInSequence = false;
        protected TableInfo _tableInfo;
        public FieldGroupComponent FieldComponent => _fieldGroupComponent;
        public Filter TemplateFilter => _templateFilter;
        public bool IsNewMode => _isNewMode;
        public bool IsInBackground => _isInBackground;
        public bool ExecuteTriggersInSequence => _executeTriggersInSequence;

        public NewOrEditService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IOfflineRequestsService offlineStoreService,
            IFilterProcessor filterProcessor,
            IFieldGroupDataService fieldGroupDataService,
            IJSProcessor jsProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineStoreService;
            _fieldGroupDataService = fieldGroupDataService;
            _jsProcessor = jsProcessor;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug($"Start NewOrEditService.PrepareContentAsync for action: {_action?.ActionUnitName}");

            ViewReference vr = _action.ViewReference;
            if (vr.IsOrganizerAction())
            {
                OrganizerActionTemplate template = new OrganizerActionTemplate(vr);
                _template = template;
                _isNewMode = false;
                string expandName = string.IsNullOrWhiteSpace(_action.ResolvedExpandName) ? _action.SourceInfoArea : _action.ResolvedExpandName;
                string fieldControlName = await ResolveExpand(expandName, _action.SourceInfoArea, cancellationToken);
                FieldControl fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Edit", cancellationToken);
                if (fieldControl == null)
                {
                    fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Details", cancellationToken);
                }

                if (fieldControl != null)
                {
                    _infoArea = _configurationService.GetInfoArea(_action.SourceInfoArea);
                    _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                    _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
                }


                await ResolveCopySourceFieldGroupName(template.CopySourceFieldGroupName(), cancellationToken);
                await ResolveTemplateFilter(template.TemplateFilterName(), cancellationToken);
            }
            else if (vr.IsDocmentUploadAction())
            {
                await ProcessFileUploadActionTemplate(vr, cancellationToken);
            }
            else if (vr.IsNewInBackgroundAction())
            {
                await NewInBackgroundActionTemplate(vr, cancellationToken);
            }
            else
            {
                await ProcessTemplate(vr, cancellationToken);
            }

            if(_isNewMode == false)
            {
                await FetchRecord(_action.RecordId, cancellationToken);
            }
            
        }

        protected virtual async Task ProcessTemplate(ViewReference vr, CancellationToken cancellationToken)
        {
            EditViewTemplate template = new EditViewTemplate(vr);
            _template = template;
            _isNewMode = template.IsNewAction();

            string expandName = template.ExpandName();
            if (string.IsNullOrEmpty(expandName))
            {
                expandName = string.IsNullOrWhiteSpace(_action.ResolvedExpandName) ? _action.InfoAreaUnitName : _action.ResolvedExpandName;
                if (string.IsNullOrEmpty(expandName))
                {
                    expandName = _action.SourceInfoArea;
                }
            }

            string configName = template.ConfigName();

            string fieldControlName = await ResolveExpand(expandName, configName, cancellationToken);

            FieldControl fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Edit", cancellationToken);
            if (fieldControl == null)
            {
                fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Details", cancellationToken);
            }

            if (fieldControl != null)
            {
                string infoAreaId = GetInfoAreaId(fieldControl, template);

                _infoArea = _configurationService.GetInfoArea(infoAreaId);
                _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
            }

            await ResolveCopySourceFieldGroupName(template.CopySourceFieldGroupName(), cancellationToken);
            AddAdditionalParameters();
            await ResolveTemplateFilter(template.TemplateFilterName(), cancellationToken);
        }

        private async Task ProcessFileUploadActionTemplate(ViewReference vr, CancellationToken cancellationToken)
        {
            FileUploadActionTemplate template = new FileUploadActionTemplate(vr);
            _template = template;
            _isNewMode = false;

            string expandName = template.UploadFieldsName();
            if (string.IsNullOrEmpty(expandName))
            {
                return;
            }

            string fieldControlName = await ResolveExpand(expandName, expandName, cancellationToken);

            FieldControl fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Edit", cancellationToken);
            if (fieldControl == null)
            {
                fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Details", cancellationToken);
            }

            if (fieldControl != null)
            {
                string infoAreaId = fieldControl.InfoAreaId;

                _infoArea = _configurationService.GetInfoArea(infoAreaId);
                _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
            }

            await ResolveTemplateFilter(template.TemplateFilterName(), cancellationToken);
        }

        private async Task NewInBackgroundActionTemplate(ViewReference vr, CancellationToken cancellationToken)
        {
            NewInBackgroundActionTemplate template = new NewInBackgroundActionTemplate(vr);
            _template = template;
            _isNewMode = true;
            _isInBackground = true;

            string expandName = template.InfoAreaId();
            if (string.IsNullOrEmpty(expandName))
            {
                return;
            }

            string fieldControlName = await ResolveExpand(expandName, expandName, cancellationToken);

            FieldControl fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Edit", cancellationToken);
            if (fieldControl == null)
            {
                fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Details", cancellationToken);
            }

            if (fieldControl != null)
            {
                string infoAreaId = fieldControl.InfoAreaId;

                _infoArea = _configurationService.GetInfoArea(infoAreaId);
                _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
            }

            await ResolveCopySourceFieldGroupName(template.CopySourceFieldGroupName(), cancellationToken);
            await ResolveTemplateFilter(template.TemplateFilterName(), cancellationToken);
        }

        protected async Task ResolveTemplateFilter(string templateFilterName, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(templateFilterName))
            {
                var filters = await _filterProcessor.RetrieveFilterDetails(new List<string> { templateFilterName }, cancellationToken, true);
                if (filters.Count > 0)
                {
                    _templateFilter = filters[0];
                }
            }
        }

        protected async Task ResolveCopySourceFieldGroupName(string fieldGroupName, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_action.RecordId))
            {
                _filterProcessor.SetAdditionalFilterParams(await _fieldGroupDataService.GetSourceFieldGroupData(_action.RecordId,
                    fieldGroupName, DetermineRequestMode(), cancellationToken));
            }
        }

        protected void AddAdditionalParameters()
        {
            if (_action.AdditionalArguments != null)
            {
                _filterProcessor.SetAdditionalFilterParams(_action.AdditionalArguments);
            }
        }

        private async Task<string> ResolveExpand(string expandName, string defaultConfigName, CancellationToken cancellationToken)
        {
            Expand expand = _configurationService.GetExpand(expandName);
            string fieldControlName = defaultConfigName;
            if (expand != null && !string.IsNullOrEmpty(expand.FieldGroupName))
            {
                fieldControlName = expand.FieldGroupName;
            }

            if (expand != null && !string.IsNullOrEmpty(expand.HeaderGroupName))
            {
                string headerName = expand.HeaderGroupName + ".New";
                Header header = await _configurationService.GetHeader(headerName, cancellationToken);
                _headerComponent.InitializeContext(header, _action);
            }

            return fieldControlName;
        }

        protected string GetInfoAreaId(FieldControl fieldControl, ActionTemplateBase template)
        {
            string infoAreaId = fieldControl.InfoAreaId;
            if (string.IsNullOrEmpty(infoAreaId))
            {
                infoAreaId = template.InfoArea();
            }

            return infoAreaId;
        }

        public async Task<ModifyRecordResult> Save(List<PanelData> inputPanels, CancellationToken cancellationToken, string RecordId = null, List<OfflineRecordLink> recordLinks = null)
        {
            Dictionary<string, string> templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_templateFilter, cancellationToken);
            OfflineRequest offlineRequest;
            bool isNew = _isNewMode && string.IsNullOrEmpty(RecordId);

            if (recordLinks == null)
            {
                recordLinks = new List<OfflineRecordLink>();
                var parentLink = _action?.GetLinkRequest();
                if (parentLink != null)
                {
                    recordLinks.Add(parentLink);
                }
            }

            if (isNew)
            {
                offlineRequest = await _offlineStoreService.CreateSaveRequest(_template, _fieldGroupComponent.FieldControl, _tableInfo, inputPanels, templateFilterValues, recordLinks, cancellationToken);
                return await ProcessOfflineRequest(offlineRequest, cancellationToken);
            }
            else
            {
                List<string> recordIds = new List<string>();
                if (!string.IsNullOrWhiteSpace(RecordId))
                {
                    recordIds.Add(RecordId.FormatedRecordId(InfoAreaId));
                }
                else
                {
                    if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
                    {
                        DataRow dataRow = _rawData.Result.Rows[0];
                        foreach(DataColumn column in _rawData.Result.Columns)
                        {
                            if(column.ColumnName.ToLower().Contains("recid"))
                            {
                                if(column.ColumnName.Contains("_"))
                                {
                                    string[] parts = column.ColumnName.Split('_');
                                    recordIds.Add(dataRow.GetColumnValue(column.ColumnName, " -1").FormatedRecordId(parts[0]));
                                }
                                else
                                {
                                    recordIds.Add(dataRow.GetColumnValue(column.ColumnName, " -1").FormatedRecordId(InfoAreaId));
                                }
                            }
                        }
                    }
                }

                offlineRequest = await _offlineStoreService.CreateUpdateRequest(_template, _fieldGroupComponent.FieldControl, inputPanels, recordIds, cancellationToken);
                return await ProcessOfflineRequest(offlineRequest, cancellationToken);
            }
        }

        private async Task<ModifyRecordResult> ProcessOfflineRequest(OfflineRequest offlineRequest, CancellationToken cancellationToken)
        {
            ModifyRecordResult modifyRecordResult = null;
            if (offlineRequest.IsValid())
            {
                modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);
            }

            if (modifyRecordResult != null && !modifyRecordResult.HasSaveErrors())
            {
                _logService.LogDebug("New records has been saved.");
                await _offlineStoreService.Delete(offlineRequest, cancellationToken);
            }
            else
            {
                if (offlineRequest.IsValid())
                {
                    await _offlineStoreService.Update(offlineRequest, cancellationToken);
                    throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
                }
                else
                {
                    throw new CrmException("Could not build the update request!", CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
                }
            }

            return modifyRecordResult;
        }

        public async Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken)
        {
            List<PanelData> result = new List<PanelData>();

            if(_isInBackground)
            {
                PanelData pd = new PanelData
                {
                    RecordId = string.Empty,
                    RecordInfoArea = _infoArea.UnitName,
                    action = _action,
                    Fields = new List<ListDisplayField>()
                };
                result.Add(pd);
                return result;
            }

            Dictionary<string, string> templateFilterValues = new Dictionary<string, string>();
            DataRow dataRow = null;
            if (_isNewMode || _action.ViewReference.IsDocmentUploadAction())
            {
                templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_templateFilter, cancellationToken);
            }
            else
            {
                if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
                {
                    dataRow = _rawData.Result.Rows[0];
                }
            }

            if (_fieldGroupComponent.HasTabs())
            {
                foreach (FieldControlTab panel in _fieldGroupComponent.FieldControl.Tabs.OrderBy(t => t.OrderId))
                {
                    var panelType = panel.GetEditPanelType();
                    if (panelType != PanelType.NotSupported && !panel.IsHeaderPanel())
                    {
                        PanelData pd = new PanelData
                        {
                            Label = panel.Label,
                            Type = panelType,
                            RecordId = IsNewMode? string.Empty : _action.RecordId,
                            PanelTypeKey = panel.Type,
                            RecordInfoArea = _infoArea.UnitName,
                            action = _action
                        };

                        List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
                        ListDisplayRow outRow = await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, dataRow, templateFilterValues,
                            _action.SourceInfoArea, _action.RecordId, panel.GetEditPanelType(), cancellationToken);
                        if(outRow.Fields.Count > 0)
                        {
                            pd.Fields = outRow.Fields;
                            result.Add(pd);
                        }
                    }
                }
            }

            return result;
        }

        public bool IsMandatoryDataReady(List<PanelData> inputPanels)
        {
            foreach(PanelData panel in inputPanels)
            {
                foreach(ListDisplayField field in panel.Fields)
                {
                    if (!field.IsMandatoryDataReady())
                    {
                        if (!inputPanels.Any(p => p.Fields.Any(
                            f => f.EditData.HasData
                            && f.Config.FieldConfig.FieldId == field.Config.FieldConfig.FieldId
                            && f.Config.FieldConfig.InfoAreaId == field.Config.FieldConfig.InfoAreaId
                            )))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool HasHeaderTitle()
        {
            return _headerComponent.Header != null && !string.IsNullOrEmpty(_headerComponent.Header.Label);
        }

        public string GetInfoArea()
        {
            return _fieldGroupComponent?.FieldControl?.InfoAreaId;
        }

        public async Task<UserAction> SavedAction(string recordId, CancellationToken cancellationToken)
        {
            if (!_template.IgnoreSavedAction())
            {
                string savedActionName = _template.SavedAction();
                if (string.IsNullOrEmpty(savedActionName))
                {
                    // By default if no action is defined we display the newly created record
                    // or go back if is in edit mode.
                    if (_isNewMode)
                    {
                        return new UserAction
                        {
                            RecordId = recordId,
                            InfoAreaUnitName = GetInfoAreaId(_fieldGroupComponent.FieldControl, _template),
                            ActionUnitName = "SHOWRECORD",
                            ActionType = UserActionType.ShowRecord
                        };
                    }
                    else
                    {
                        return new UserAction
                        {
                            RecordId = recordId,
                            InfoAreaUnitName = GetInfoAreaId(_fieldGroupComponent.FieldControl, _template),
                            ActionUnitName = "Back",
                            ActionType = UserActionType.NavigateBack
                        };
                    }
                }
                else
                {
                    return await _userActionBuilder.ResolveSavedAction(_configurationService, savedActionName, recordId, GetInfoAreaId(_fieldGroupComponent.FieldControl, _template), cancellationToken);
                }
            }

            return null;
        }

        public virtual async Task FetchRecord(string recordId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(recordId) && _fieldGroupComponent.FieldControl != null && _fieldGroupComponent.FieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fields = _fieldGroupComponent.GetQueryFields(_fieldGroupComponent.FieldControl.Tabs);
                if (fields.Count > 0)
                    _rawData = await _crmDataService.GetRecord(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fields, RecordId = recordId },
                        DetermineRequestMode());

            }
        }

        private string GetClientFilterName()
        {
            string filterName = _action.ViewReference.GetArgumentValue("ClientRightsFilterName");
            if (string.IsNullOrWhiteSpace(filterName))
            {
                filterName = _fieldGroupComponent?.GetClientFilterName(_isNewMode);
            }

            return filterName;
        }

        private async Task<Filter> PrepareClientFilter(string filterName, Dictionary<string, string> valuesWithFunctionName, CancellationToken token)
        {
            _filterProcessor.SetAdditionalFilterParams(valuesWithFunctionName);
            return await _filterProcessor.RetrieveFilterDetails(filterName, token);
        }

        public async Task<List<EditFieldConstraintViolation>> ProcessClientFilter(List<PanelData> inputPanels, CancellationToken token)
        {
            List<EditFieldConstraintViolation> violations = new List<EditFieldConstraintViolation>();

            string clientFilterName = GetClientFilterName();
           
            Dictionary<string, string> valuesByFieldId = new Dictionary<string, string>();
            Dictionary<string, string> valuesByFunctionName = new Dictionary<string, string>();
            Dictionary<string, string> allValues = new Dictionary<string, string>();

            foreach (PanelData panel in inputPanels)
            {
                foreach (ListDisplayField field in panel.Fields)
                {
                    if (!field.IsMandatoryDataReady() && !field.Config.PresentationFieldAttributes.Hide)
                    {
                        if (!inputPanels.Any(p => p.Fields.Any(
                            f => f.EditData.HasData
                            && f.Config.FieldConfig.FieldId == field.Config.FieldConfig.FieldId
                            && f.Config.FieldConfig.InfoAreaId == field.Config.FieldConfig.InfoAreaId
                            )))
                        {
                            violations.Add(new EditFieldConstraintViolation(EditFieldConstraintViolation.ViolationType.MustField, null, field));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(clientFilterName))
                    {
                        string fieldIdentification = field.Config.FieldConfig.FieldIdentification();
                        if (valuesByFieldId.ContainsKey(fieldIdentification))
                        {
                            valuesByFieldId[fieldIdentification] = field.EditData.ValueForFilter();
                        }
                        else
                        {
                            valuesByFieldId.Add(fieldIdentification, field.EditData.ValueForFilter());
                        }

                        var function = field.Config.FieldConfig.Function;
                        if (!string.IsNullOrWhiteSpace(function))
                        {
                            function = "New" + function;
                            if (valuesByFunctionName.ContainsKey(function))
                            {
                                valuesByFunctionName[function] = field.EditData.ValueForFilter();
                            }
                            else
                            {
                                valuesByFunctionName.Add(function, field.EditData.ValueForFilter());
                            }
                        }

                        string key = field.Config.FieldConfig.InfoAreaId + "." + field.Config.FieldConfig.FieldId;
                        if (allValues.ContainsKey(key))
                        {
                            allValues[key] = field.EditData.ValueForFilter();
                        }
                        else
                        {
                            allValues.Add(key, field.EditData.ValueForFilter());
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(clientFilterName))
            {
                Filter clientFilter = await PrepareClientFilter(clientFilterName, valuesByFunctionName, token);
                foreach (PanelData panel in inputPanels)
                {
                    foreach (ListDisplayField field in panel.Fields)
                    {
                        string verificationResult = await _filterProcessor.CheckFieldClientFilter(field, clientFilter, allValues, token);
                        if (verificationResult != null)
                        {
                            if (violations.Find(v => v.Type == EditFieldConstraintViolation.ViolationType.ClientConstraint && v.LocalizedDescription.Equals(verificationResult)) == null)
                            {
                                violations.Add(new EditFieldConstraintViolation(EditFieldConstraintViolation.ViolationType.ClientConstraint, verificationResult, field));
                            }
                        }
                    }
                }
            }
        
            return violations;
        }

        public async Task<List<EditTriggerItem>> GetTheEditTriggers(IFilterProcessor filterProcessor, CancellationToken token)
        {
            string editTriggerName = _action.ViewReference.GetArgumentValue("EditTriggerFilter");
            if (string.IsNullOrWhiteSpace(editTriggerName))
            {
                editTriggerName = _fieldGroupComponent?.GetEditTriggerFilter(_isNewMode);
            }

            var executeTriggersInSequenceValue = _fieldGroupComponent?.FieldControl?.Attributes.Where(a => a.Key.Equals("ExecuteTriggersInSequence")).FirstOrDefault()?.Value;
            _executeTriggersInSequence = executeTriggersInSequenceValue == "true" ? true : false;

            List<EditTriggerItem> EditTriggers = null;
            if (!string.IsNullOrWhiteSpace(editTriggerName))
            {
                EditTriggers = new List<EditTriggerItem>();
                var editTriggers = editTriggerName.Split(';');
                int orderKey = 0;
                foreach (var triggerName in editTriggers)
                {
                    if (!string.IsNullOrWhiteSpace(triggerName))
                    {
                        var editTriggerItem = new EditTriggerItem();
                        editTriggerItem.Name = triggerName;
                        editTriggerItem.Order = orderKey++;
                        int triggerKey = 0;
                        var filterUnits = await _filterProcessor.PrepareEditTriggerUnits(triggerName, token);
                        if(filterUnits!=null && filterUnits.Count> 0)
                        {
                            foreach(var trigger in filterUnits)
                            {
                                trigger.Key = triggerKey++;
                                editTriggerItem.TriggerUnits.Add(trigger);
                            }

                        }
                        EditTriggers.Add(editTriggerItem);
                    }
                    
                }
                // Prepare the JavaScript Processor
                await _jsProcessor.InitJSRuntime(token);
            }
            
            return EditTriggers;
        }

        public async Task<Dictionary<string, string>> ProcessTrigger(EditTriggerItem trigger, Dictionary<string, string> AllParameters, Dictionary<string, string> inputParams, CancellationToken token)
        {
            var evaluationResults = new Dictionary<string, string>();

            foreach (var triggerRule in trigger?.TriggerUnits)
            {
                if (triggerRule.VariableParameters != null && triggerRule.VariableParameters.Any(vPar => inputParams.Keys.Any(inPar => inPar == vPar)) && triggerRule.Evaluations?.Count > 0)
                {
                    var keys = new List<string>(triggerRule.Evaluations.Keys);
                    foreach (string key in keys)
                    {
                        var evaluationCode = triggerRule.Evaluations[key];
                        var evaluatedValue = await _jsProcessor.ExecuteJSEvaluation(evaluationCode, triggerRule, AllParameters, token);
                        if (!evaluatedValue.Equals("null"))
                        {
                            if (!evaluationResults.ContainsKey(key))
                            {
                                evaluationResults.Add(key, evaluatedValue);
                            }
                            else
                            {
                                evaluationResults[key] = evaluatedValue;
                            }
                        }
                    }
                }
            }

            return evaluationResults;
        }

        private RequestMode DetermineRequestMode()
        {
            if (_sessionContext.IsInOfflineMode || !_sessionContext.HasNetworkConnectivity)
            {
                return RequestMode.Offline;
            }

            if (_action.IsRecordRetrievedOnline)
            {
                return RequestMode.Best;
            }

            return _template.GetRequestMode();
        }


        public async Task<int> GetParentCode(ListDisplayField parentField, CancellationToken token)
        {
            List<SelectableFieldValue> allowedValues = null;

            if (parentField.Config.AllowedValues != null && parentField.Config.AllowedValues.Count > 0)
            {
                allowedValues = parentField.Config.AllowedValues;
            }
            else
            {
                allowedValues = await _fieldGroupComponent.ExtractAllowedCatalogValues(parentField.Config.PresentationFieldAttributes.FieldInfo, token);
            }

            if(allowedValues != null)
            { 
                var item = allowedValues.Find(a => a.DisplayValue.Equals(parentField.Data.StringData, StringComparison.CurrentCultureIgnoreCase));

                if (item != null)
                {
                    if(int.TryParse(item.RecordId, out int parentCode))
                    {
                        return parentCode;
                    }
                }
            }

            return -1;
        }
}
}
