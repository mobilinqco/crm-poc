using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Messages;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Localization;
using ACRM.mobile.Services;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class NewOrEditPageViewModel : NavigationBarBaseViewModel
    {
        private readonly INewOrEditService _newOrEditService;
        private readonly IRightsProcessor _rightsProcessor;
        private readonly ModifyRecordActivity _modifyRecordActivity;
        private UserAction _userAction;
        private List<string> ParentCalogs = new List<string>();
        private List<EditTriggerItem> EditTriggers;
        private Dictionary<string, string> EditParameterCache;
        private Dictionary<string, string> FieldKeyFunctionCache;
        private bool _hasUiDataChanged = false;

        public ICommand OnCancelCommand => new Command(async () => await OnCancel());
        public ICommand OnSaveCommand => new Command(async () => await OnSave());

        private Color _infoAreaColor = Color.LightGray;
        public Color InfoAreaColor
        {
            get => _infoAreaColor;
            set
            {
                _infoAreaColor = value;
                RaisePropertyChanged(() => InfoAreaColor);
            }
        }

        private string _cancelButtonTitle;
        public string CancelButtonTitle
        {
            get
            {
                return _cancelButtonTitle;
            }

            set
            {
                _cancelButtonTitle = value;
                RaisePropertyChanged(() => CancelButtonTitle);
            }
        }

        private string _saveButtonTitle;
        public string SaveButtonTitle
        {
            get
            {
                return _saveButtonTitle;
            }

            set
            {
                _saveButtonTitle = value;
                RaisePropertyChanged(() => SaveButtonTitle);
            }
        }

        private bool _isErrorMessageVisible = false;
        public bool IsErrorMessageVisible
        {
            get => _isErrorMessageVisible;
            set
            {
                _isErrorMessageVisible = value;
                RaisePropertyChanged(() => IsErrorMessageVisible);
            }
        }

        private bool _isSaveButtonEnabled = false;
        public bool IsSaveButtonEnabled
        {
            get => _isSaveButtonEnabled;
            set
            {
                _isSaveButtonEnabled = value;
                RaisePropertyChanged(() => IsSaveButtonEnabled);
            }
        }

        private bool _hasEditRights = true;
        public bool HasEditRights
        {
            get => _hasEditRights;
            set
            {
                _hasEditRights = value;
                RaisePropertyChanged(() => HasEditRights);
            }
        }

        private string _rightsMessage;
        public string RightsMessage
        {
            get => _rightsMessage;
            set
            {
                _rightsMessage = value;
                RaisePropertyChanged(() => RightsMessage);
            }
        }

        public ObservableCollection<EditFieldConstraintViolation> PageEditErrors { get; set; }
        private bool _isSaved = false;
        private List<OfflineRecordLink> _links = new List<OfflineRecordLink>();

        public NewOrEditPageViewModel(INewOrEditService newOrEditService, IRightsProcessor rightsProcessor, ModifyRecordActivity modifyRecordActivity)
        {
            _newOrEditService = newOrEditService;
            _rightsProcessor = rightsProcessor;
            _modifyRecordActivity = modifyRecordActivity;
            IsLoading = true;
            IsErrorMessageVisible = false;
            RegisterMessage(WidgetEventType.RecordSelectorTapped, "EditRecordSelectorTapped", OnRecordSelectorTapped);
            RegisterMessage(WidgetEventType.DateTimeEntryTapped, "DateTimeEntryTapped", OnDateTimeEntryTapped);
            RegisterMessage(WidgetEventType.UIDataChanged, "UIDataChanged", UIDataChangedHandler);

            RegisterMessage(WidgetEventType.RecordSelectorTapped, "LinkParticipant", OnRecordSelectorTapped);
            RegisterMessage(WidgetEventType.AddParentCatalog, null, AddParentCatalogEventHandler);
            MessagingCenter.Subscribe<BaseViewModel, RecordSelectedMessage>(this, InAppMessages.RecordSelected, async (sender, arg) => await OnRecordSelected(sender, arg));
            MessagingCenter.Subscribe<BaseViewModel, DateTimePopupData>(this, InAppMessages.DateTimeSelected, async (sender, arg) => await OnDateTimeSelected(sender, arg));

            CancelButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);
            SaveButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSave);
            PageEditErrors = new ObservableCollection<EditFieldConstraintViolation>();            
        }

        private async Task HandleEditTriggerChangeEvent(WidgetMessage arg)
        {
            try
            {
                if (arg != null)
                {
                    string data = string.Empty;
                    if (arg.Data is SelectableFieldValue item)
                    {
                        if (arg.ControlKey.EndsWith(".Value"))
                        {
                            data = item.DisplayValue;
                        }
                        else
                        {
                            data = item.RecordId;
                        }
                    }
                    else
                    {
                        data = arg.Data?.ToString();
                    }
                    var inputParams = new Dictionary<string, string> { { arg.ControlKey, data } };
                    await ProcessEditTriggerEventInOrder(inputParams, 0);

                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error Processing EditTrigger (Function : {arg.ControlKey}, Data : {arg.Data}, Message : {ex.Message}.");
            }

        }

        private async Task ProcessEditTriggerEventInOrder(Dictionary<string, string> inputParams, int orderkey)
        {
            if(inputParams == null || inputParams.Keys.Count == 0)
            {
                return;
            }

            var trigger = EditTriggers.Where(Tr => Tr.TriggerUnits.Any(TrUnit => TrUnit.VariableParameters.Any(vPar => inputParams.Keys.Any(inPar => inPar == vPar))) && Tr.Order >= orderkey).OrderBy(a => a.Order).FirstOrDefault();
            if (trigger != null && inputParams!=null)
            {
                foreach (var param in inputParams.Keys)
                {
                    if (EditParameterCache.ContainsKey(param))
                    {
                        EditParameterCache[param] = inputParams[param];
                    }
                }

                Dictionary<string, string> evaluationResult = await _newOrEditService.ProcessTrigger(trigger, EditParameterCache, inputParams, _cancellationTokenSource.Token);

                Dictionary<string, string> outputParams = new Dictionary<string, string>();

                foreach (var key in evaluationResult?.Keys)
                {
                    await PublishMessage(new WidgetMessage
                    {
                        EventType = WidgetEventType.SetValueByFieldId,
                        ControlKey = key,
                        Data = evaluationResult[key],
                        SubData = true
                    }, MessageDirections.ToChildren);

                    if(FieldKeyFunctionCache.ContainsKey(key))
                    {
                        var function = FieldKeyFunctionCache[key];
                        if (!string.IsNullOrWhiteSpace(function) && EditParameterCache.ContainsKey(function))
                        {
                            EditParameterCache[function] = evaluationResult[key];
                            outputParams.Add(function, evaluationResult[key]);
                         }
                    }
                }

                if(outputParams?.Keys.Count > 0 && _newOrEditService.ExecuteTriggersInSequence)
                {
                    await ProcessEditTriggerEventInOrder(outputParams, orderkey + 1);
                }
            }

        }

        private Task AddParentCatalogEventHandler(WidgetMessage arg)
        {
            if (arg.Data !=null && arg.Data is string catalogId && !ParentCalogs.Contains(catalogId))
            {
                ParentCalogs.Add(catalogId);
            }

            return Task.CompletedTask;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsBackButtonVisible = true;
            IsLoading = true;
            IsSaveButtonEnabled = false;
            if (navigationData is UserAction ua)
            {
                IsBusy = true;
                _userAction = ua;

                var (status, result, message) = await _rightsProcessor.EvaluateRightsFilter(_userAction, _cancellationTokenSource.Token);
                if (status)
                {
                    if (!result)
                    {
                        IsBusy = false;
                        IsLoading = false;
                        IsSaveButtonEnabled = false;
                        HasEditRights = false;
                        RightsMessage = message;
                        return;

                    }
                }

                _newOrEditService.SetSourceAction(ua);
                await _newOrEditService.PrepareContentAsync(_cancellationTokenSource.Token);

                var parentLink = _userAction?.GetLinkRequest();
                if (parentLink != null)
                {
                    _links.Add(parentLink);
                }

                if (_newOrEditService.IsInBackground)
                {
                    await PrepareAndSave();
                }
                else
                {

                    await UpdateBindingsAsync();
                    IsSaveButtonEnabled = true;
                }
                IsBusy = false;
            }
            IsLoading = false;
            await base.InitializeAsync(navigationData);
        }

        private async Task PrepareAndSave()
        {
            if (_newOrEditService.HasHeaderTitle())
            {
                PageTitle = _newOrEditService.PageTitle();
            }
            else
            {
                PageTitle = _localizationController.GetFormatedString(LocalizationKeys.TextGroupBasic,
                    _newOrEditService.IsNewMode ? LocalizationKeys.KeyBasicNewInfoArea : LocalizationKeys.KeyBasicEditModeTitle,
                    _newOrEditService.PageTitle());
            }

            var pnls = await _newOrEditService.PanelsAsync(_cancellationTokenSource.Token);
            IsErrorMessageVisible = false;
            string recordId = string.Empty;
            string infoAreaId = string.Empty;
            try
            {
                _isSaved = false;
                var result = await _newOrEditService.Save(pnls, _cancellationTokenSource.Token, null, _links);
                recordId = result.SavedRecord.RecordId;
                infoAreaId = result.SavedRecord.InfoAreaId;

                if (!string.IsNullOrWhiteSpace(recordId) && !string.IsNullOrWhiteSpace(infoAreaId))
                {
                    _isSaved = true;
                    _hasUiDataChanged = false;
                }
            }
            catch (CrmException ex)
            {
                if (ex.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                {
                    _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                }

                _logService.LogError("Error saving the record.");

                EditFieldConstraintViolation error = new EditFieldConstraintViolation(EditFieldConstraintViolation.ViolationType.SaveError, null, null);
                error.ErrorLabel = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                error.LocalizedDescription = $"{ex.Content}";
                PageEditErrors.Add(error);
                IsErrorMessageVisible = true;
            }

            if (_isSaved)
            {

                UserAction saveAction = await _newOrEditService.SavedAction(recordId, _cancellationTokenSource.Token);
                await _navigationController.NavigateAsyncForActionAndRemoveParent(saveAction, _cancellationTokenSource.Token);
                await _navigationController.ForceRefreshForParent();
            }

        }

        private async Task OnRecordSelectorTapped(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is ListDisplayField field)
            {
                await OnRecordSelectorEntry(field, widgetMessage.ControlKey);
            }
        }

        private async Task OnDateTimeEntryTapped(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is DateTimePopupData filedData)
            {
                IsLoading = true;
                await _navigationController.DisplayPopupAsync<DateTimeSelectorPageViewModel>(filedData);
                IsLoading = false;
            }
        }

        private async Task UIDataChangedHandler(WidgetMessage widgetMessage)
        {
            _hasUiDataChanged = true;
        }


        private async Task UpdateBindingsAsync()
        {
            if (_newOrEditService.HasHeaderTitle())
            {
                PageTitle = _newOrEditService.PageTitle();
            }
            else
            {
                PageTitle = _localizationController.GetFormatedString(LocalizationKeys.TextGroupBasic,
                    _newOrEditService.IsNewMode ? LocalizationKeys.KeyBasicNewInfoArea : LocalizationKeys.KeyBasicEditModeTitle,
                    _newOrEditService.PageTitle());
            }
            InfoAreaColor = Color.FromHex(_newOrEditService.PageAccentColor());
            var pnls = await _newOrEditService.PanelsAsync(_cancellationTokenSource.Token);
            await LoadEditTrigger(pnls);
            Widgets = await pnls.BuildWidgetsAsyc(this, _cancellationTokenSource);
            // Intalize Parent Catalog fields

            foreach (var catlaogs in ParentCalogs)
            {
                await PublishMessage(new WidgetMessage
                {
                    ControlKey = catlaogs,
                    Data = null,
                    EventType = WidgetEventType.InitalizeParentCatalog
                }, MessageDirections.ToChildren);
            }

            // Intalize the selected items for child catalogs
            await PublishMessage(new WidgetMessage {
             ControlKey = null,
             EventType = WidgetEventType.InitalizeSelectedItem}, MessageDirections.ToChildren);
        }

        private async Task LoadEditTrigger(List<PanelData> panel)
        {
            IFilterProcessor filterProcessor = AppContainer.Resolve<IFilterProcessor>();
            EditParameterCache = new Dictionary<string, string>();
            FieldKeyFunctionCache = new Dictionary<string, string>();
            EditTriggers = await _newOrEditService.GetTheEditTriggers(filterProcessor,_cancellationTokenSource.Token);

            if (EditTriggers?.Count > 0)
            {
                // Load the intial Param Values
                foreach (var trigger in EditTriggers)
                {
                    foreach (var triggerUnit in trigger?.TriggerUnits)
                    {
                        foreach (var param in triggerUnit?.VariableParameters)
                        {
                            if (!EditParameterCache.ContainsKey(param))
                            {
                                EditParameterCache.Add(param, string.Empty);
                            }
                        }

                        foreach (var param in triggerUnit?.Evaluations.Keys)
                        {
                            if (!FieldKeyFunctionCache.ContainsKey(param))
                            {
                                FieldKeyFunctionCache.Add(param, string.Empty);
                            }
                        }
                    }

                }

                var keys = new List<string>(EditParameterCache.Keys);
                foreach (var key in keys)
                {
                    var value = panel.GetFunctionRawValue(key);
                    if (EditParameterCache.ContainsKey(key))
                    {
                        EditParameterCache[key] = value;
                    }
                }

                keys = new List<string>(FieldKeyFunctionCache.Keys);
                foreach (var key in keys)
                {
                    var value = panel.GetFunctionFromFieldId(key);
                    if (FieldKeyFunctionCache.ContainsKey(key))
                    {
                        FieldKeyFunctionCache[key] = value;
                    }
                }

                if (EditParameterCache?.Keys?.Count > 0)
                {
                    foreach (var functions in EditParameterCache.Keys)
                    {
                        RegisterMessageIfNotExist(WidgetEventType.ValueChanged, functions, HandleEditTriggerChangeEvent);
                    }
                }
            }

        }

        private void ResolveViolationDescription(EditFieldConstraintViolation violation)
        {
            var localizedDescription = violation.LocalizedDescription;
            if (string.IsNullOrEmpty(violation.ErrorLabel))
            {
                if (violation.Type == EditFieldConstraintViolation.ViolationType.MustField)
                {
                    violation.ErrorLabel = violation.Field.Config.PresentationFieldAttributes.Label().Trim('\r', '\n')
                        + ": " + _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                            LocalizationKeys.KeyErrorsEditMandatoryFieldNotSet)
                        .Trim('\r', '\n');
                    
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(localizedDescription))
                    {
                        violation.ErrorLabel = localizedDescription.Trim('\r', '\n'); ;
                    }
                    else
                    {
                        violation.ErrorLabel = violation.Field.Config.PresentationFieldAttributes.Label().Trim('\r', '\n')
                            + ": " + _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                                LocalizationKeys.KeyErrorsClientConstraintError)
                            .Trim('\r', '\n');
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(localizedDescription))
            {
                violation.LocalizedDescription = violation.Field.Config
                    .PresentationFieldAttributes.Label()
                    .Trim('\r', '\n') + "("
                    + violation.Field.Config.FieldConfig.FieldIdentification()
                    + ")";
            }
        }

        private async Task OnSave()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                IsSaveButtonEnabled = false;
                IsErrorMessageVisible = false;
                PageEditErrors.Clear();
                await PublishMessage(new WidgetMessage
                {
                    EventType = WidgetEventType.ClearFieldValidationError,
                    ControlKey = "*",
                    Data = "*"
                }, MessageDirections.ToChildren);

                List<PanelData> inputPanels = Widgets.GetPanelDatas();
                List<EditFieldConstraintViolation> violations = await _newOrEditService.ProcessClientFilter(inputPanels, _cancellationTokenSource.Token);

                if (violations.Count > 0)
                {
                    foreach (var violation in violations)
                    {
                        ResolveViolationDescription(violation);
                        PageEditErrors.Add(violation);

                        await PublishMessage(new WidgetMessage
                        {
                            EventType = WidgetEventType.FieldValidationError,
                            ControlKey = $"{violation.Field.Config.FieldConfig?.InfoAreaId}_{violation.Field.Config.FieldConfig?.FieldId}",
                            Data = $"{violation.Field.Config.FieldConfig?.InfoAreaId}_{violation.Field.Config.FieldConfig?.FieldId}"
                        }, MessageDirections.ToChildren);
                    }

                    IsErrorMessageVisible = true;
                }
                else
                {
                    IsErrorMessageVisible = false;
                    string recordId = string.Empty;
                    string infoAreaId = string.Empty;

                    try
                    {
                        if (_newOrEditService.IsNewMode && inputPanels.HasData() || inputPanels.HasChanges())
                        {
                            var result = await _newOrEditService.Save(inputPanels, _cancellationTokenSource.Token, null, _links);
                            recordId = result.SavedRecord.RecordId;
                            infoAreaId = result.SavedRecord.InfoAreaId;
                        }
                        else
                        {
                            recordId = _userAction.RecordId;
                            infoAreaId = _newOrEditService.InfoAreaId;
                        }

                        if (!string.IsNullOrWhiteSpace(recordId) && !string.IsNullOrWhiteSpace(infoAreaId))
                        {
                            await UploadImages(recordId, infoAreaId, _cancellationTokenSource.Token);
                            await SaveChildWidgets(recordId, _cancellationTokenSource.Token);
                            _isSaved = true;
                            _hasUiDataChanged = false;
                        }
                    }
                    catch (CrmException ex)
                    {
                        if (ex.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                        {
                            _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                        }

                        _logService.LogError("Error saving the record.");

                        EditFieldConstraintViolation error = new EditFieldConstraintViolation(EditFieldConstraintViolation.ViolationType.SaveError, null, null);
                        error.ErrorLabel = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                        error.LocalizedDescription = $"{ex.Content}";
                        PageEditErrors.Add(error);
                        IsErrorMessageVisible = true;
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError("Error saving the record to the network.");

                        EditFieldConstraintViolation error = new EditFieldConstraintViolation(EditFieldConstraintViolation.ViolationType.SaveError, null, null);
                        error.ErrorLabel = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                        error.LocalizedDescription = $"{ex.Message}";
                        PageEditErrors.Add(error);
                        IsErrorMessageVisible = true;
                    }


                    if (!IsErrorMessageVisible)
                    {
                        if (string.IsNullOrWhiteSpace(recordId))
                        {
                            recordId = _userAction.RecordId;
                        }

                        UserAction saveAction = await _newOrEditService.SavedAction(recordId, _cancellationTokenSource.Token);
                        if (saveAction.ActionType == UserActionType.ModifyRecord)
                        {
                            if (!_modifyRecordActivity.IsModifyRecodServiceBusy)
                            {
                                await _modifyRecordActivity.PrepareModifyRecordServiceAsync(_cancellationTokenSource.Token);
                                await _modifyRecordActivity.HandleModifyRecord(saveAction, RefreshAsync, true, _cancellationTokenSource.Token);
                            }
                        }
                        else
                        {
                            await _navigationController.NavigateAsyncForActionAndRemoveParent(saveAction, _cancellationTokenSource.Token);
                            await _navigationController.ForceRefreshForParent();
                        }

                    }
                }

                IsSaveButtonEnabled = true;
                IsLoading = false;
            }
        }

        private async Task SaveChildWidgets(string RecordId, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(RecordId))
            {
                await PublishMessage(
                    new WidgetMessage
                    {
                        EventType = WidgetEventType.SaveChild,
                        ControlKey = string.Empty,
                        Data = RecordId
                    }, MessageDirections.ToChildren);
            }
        }

        private async Task UploadImages(string RecordId,string InfoAreaId, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(RecordId))
            {
                var listDocModels = GetDocModels();
                if(listDocModels!=null && listDocModels.Count > 0 && _userAction!=null)
                {
                    foreach(var docModel in listDocModels)
                    {
                        await docModel.SetUserAction(new UserAction
                        {
                            ActionDisplayName = _userAction.ActionDisplayName,
                            ActionUnitName = _userAction.ActionUnitName,
                            SourceInfoArea = InfoAreaId,
                            InfoAreaUnitName = _userAction?.InfoAreaUnitName,
                            RecordId = RecordId,
                            SubActionUnitId = -1,
                            ViewReference = _userAction.ViewReference,
                        });
                        var result = await docModel.FileUploadCommandHandler();
                        if (!result)
                        {
                            await _dialogContorller.ShowAlertAsync(
                            docModel.Message,
                            "",
                            _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                        }
                    }
                }
            }
            
        }

        private List<DocumentUploadPageViewModel> GetDocModels()
        {
            List<DocumentUploadPageViewModel> listDocModelss = new List<DocumentUploadPageViewModel>();
            foreach (var widget in Widgets)
            {
                if (widget != null && widget.Widgets != null)
                {
                    foreach (var controlWidget in widget.Widgets)
                    {
                        if (controlWidget != null && (controlWidget is ImageControlModel imgmodel))
                        {
                            var docModel = imgmodel.DocuemntModel;
                            if (docModel != null && (docModel is DocumentUploadPageViewModel model))
                            {
                                listDocModelss.Add(model);
                            }
                        }
                    }
                }
            }

            return listDocModelss;
        }

        private async Task OnCancel()
        {
            await _navigationController.BackAsync();
        }

        public override async Task<bool> CanClose()
        {
            if (_hasUiDataChanged)
            {
                bool shouldDiscard = await _dialogContorller.ShowConfirm(
                    _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesReallyAbortAndLoseChanges),
                    _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesYouMadeChanges),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicYes),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel));
                if (!shouldDiscard)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task OnRecordSelectorEntry(ListDisplayField field, string ownerKey)
        {
            if (field.Config.RecordSelectorAction != null)
            {
                UserAction ua = field.Config.RecordSelectorAction;
                ua.TargetLinkInfoAreaId = _newOrEditService.GetInfoArea();

                var inputObject = new RecordSelectorInput
                {
                    UIParams = GetCurrentParams(),
                    UserAction = ua,
                    OwnerKey = ownerKey
                };
                IsLoading = true;
                await _navigationController.DisplayPopupAsync<RecordSelectorPageViewModel>(inputObject);
                IsLoading = false;
            }
        }

        private Dictionary<string,string> GetCurrentParams()
        {
            var currentParrems = new Dictionary<string, string>();
            List<PanelData> inputPanels = Widgets.GetPanelDatas();
            foreach(var panel in inputPanels)
            {
                foreach(var field in panel.Fields)
                {
                    var function = field.Config.FieldConfig.Function;
                    if (!string.IsNullOrWhiteSpace(function))
                    {
                        if(currentParrems.ContainsKey(function))
                        {
                            currentParrems[function] = field.EditData.SelectedRawValue();
                        }
                        else
                        {
                            currentParrems.Add(function, field.EditData.SelectedRawValue());
                        }
                    }
                }
            }
            return currentParrems;
        }

        private void AddRecordLink(RecordSelectedMessage message)
        {
            if(message.LinkDetails != null)
            {
                foreach(OfflineRecordLink link in _links)
                {
                    if(string.Equals(message.LinkDetails.ParentInfoAreaId, link.InfoAreaId)
                        && link.HasLinkIdEqualWith(message.LinkDetails.LinkId))
                    {
                        link.RecordId = message.LinkDetails.RecordId;
                        return;
                    }
                }

                _links.Add(new OfflineRecordLink {
                    InfoAreaId = message.LinkDetails.ParentInfoAreaId,
                    LinkId = message.LinkDetails.LinkId,
                    RecordId = message.LinkDetails.RecordId
                });

            }
        }

        private async Task OnDateTimeSelected(BaseViewModel caller, DateTimePopupData message)
        {
            await PublishMessage(
                new WidgetMessage
                {
                    EventType = WidgetEventType.DateTimeSelected,
                    ControlKey = message.FieldIdentification,
                    Data = message
                }, MessageDirections.ToChildren);
        }

        private async Task OnRecordSelected(BaseViewModel caller, RecordSelectedMessage message)
        {
            RecordSelectorTemplate rs = message.RecordSelectorAction.RecordSelector;

            if(!string.IsNullOrEmpty(message.OwnerKey) && message.OwnerKey.StartsWith("LinkParticipant"))
            {
                await PublishMessage(
                            new WidgetMessage
                            {
                                EventType = WidgetEventType.LinkParticipantSelected,
                                ControlKey = message.OwnerKey,
                                Data = message.SelectedRow
                            }, MessageDirections.ToChildren);
            }
            else if (rs != null)
            {
                await PublishMessage(
                    new WidgetMessage
                    {
                        EventType = WidgetEventType.SetLinkedRecordSelectorParentId,
                        ControlKey = message.SelectedRow.InfoAreaId,
                        Data = message.SelectedRow.RecordId
                    }, MessageDirections.ToChildren);

                AddRecordLink(message);

                List<string> clearValues = rs.ClearFields();
                string targetPrefix = rs.TargetPrefix();
                if (clearValues != null)
                {
                    foreach (var functions in clearValues)
                    {
                        _ = PublishMessage(
                            new WidgetMessage
                            {
                                EventType = WidgetEventType.ClearFieldValues,
                                ControlKey = functions,
                                Data = null
                            }, MessageDirections.ToChildren);
                    }
                }

                foreach (ListDisplayField selField in message.SelectedRow.Fields)
                {
                    if (selField.Data.ColspanData != null && selField.Data.ColspanData.Count > 0)
                    {
                        foreach (ListDisplayField colSpanField in selField.Data.ColspanData)
                        {
                            await ExtractFieldValue(message, targetPrefix, colSpanField);
                        }
                    }
                    else
                    {
                        await ExtractFieldValue(message, targetPrefix, selField);
                    }
                }
            }
        }

        private async Task ExtractFieldValue(RecordSelectedMessage message, string targetPrefix, ListDisplayField selField)
        {
            if (!string.IsNullOrEmpty(selField.Config.FieldConfig.Function))
            {
                var fun = selField.Config.FieldConfig.Function;
                if (!string.IsNullOrEmpty(targetPrefix))
                {
                    fun = $"{targetPrefix}{fun}";
                }

                string newValue = _localizationController.GetLocalizedValue(selField);
                SelectedRecordFieldData selectedData = new SelectedRecordFieldData();
                selectedData.StringValue = newValue;
                selectedData.SelectedValue = new SelectableFieldValue { DisplayValue = newValue, RecordId = message.SelectedRow.RecordId };
                selectedData.InfoAreaId = selField.Config.FieldConfig.InfoAreaId;
                selectedData.ParentCode = await GetParentCode(message, selField);
                selectedData.SelectedField = message.SelectedRow;

                await PublishMessage(
                new WidgetMessage
                {
                    EventType = WidgetEventType.SetSelectedResult,
                    ControlKey = fun,
                    Data = selectedData
                }, MessageDirections.ToChildren);

            }
        }

        private async Task<int> GetParentCode(RecordSelectedMessage message, ListDisplayField selField)
        {
            if(selField.Config.PresentationFieldAttributes.FieldInfo.Ucat > 0)
            {
                // Search first in the list of fields retrieved from the record selector
                foreach (ListDisplayField field in message.SelectedRow.Fields)
                {
                    if (selField.Data.ColspanData != null && selField.Data.ColspanData.Count > 0)
                    {
                        foreach (ListDisplayField colSpanField in selField.Data.ColspanData)
                        {
                            if(colSpanField != selField
                                && colSpanField.Config.PresentationFieldAttributes.FieldInfo.FieldId
                                == selField.Config.PresentationFieldAttributes.FieldInfo.Ucat)
                            {
                                return await _newOrEditService.GetParentCode(colSpanField, _cancellationTokenSource.Token);
                            }
                        }
                    }
                    else
                    {
                        if (field != selField
                                && field.Config.PresentationFieldAttributes.FieldInfo.FieldId
                                == selField.Config.PresentationFieldAttributes.FieldInfo.Ucat)
                        {
                            return await _newOrEditService.GetParentCode(field, _cancellationTokenSource.Token);
                        }
                    }
                }

                // If Parent catalog not found search in the list of fields from the panels
                List<PanelData> inputPanels = Widgets.GetPanelDatas();
                foreach (var panel in inputPanels)
                {
                    foreach (var field in panel.Fields)
                    {
                        if (field.Config.PresentationFieldAttributes.FieldInfo.FieldId != selField.Config.PresentationFieldAttributes.FieldInfo.FieldId
                                && field.Config.PresentationFieldAttributes.FieldInfo.FieldId
                                == selField.Config.PresentationFieldAttributes.FieldInfo.Ucat)
                        {
                            return await _newOrEditService.GetParentCode(field, _cancellationTokenSource.Token);
                        }
                    }
                }
            }
            return -1;
        }
    }
}
