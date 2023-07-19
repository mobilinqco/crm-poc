using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.Utils
{
	public class ModifyRecordActivity
	{
        private readonly IModifyRecordService _modifyRecordService;
        private readonly IUserActionBuilder _userActionBuilder;
        private readonly IRightsProcessor _rightsProcessor;
        private readonly ILogService _logService;
        private readonly IDialogContorller _dialogContorller;
        private readonly INavigationController _navigationController;
        private readonly ILocalizationController _localizationController;
        private BackgroundSyncManager _backgroundSyncManager;

        public bool IsModifyRecodServiceBusy = false;

        public ModifyRecordActivity(IModifyRecordService modifyRecordService,
            IUserActionBuilder userActionBuilder,
            IRightsProcessor rightsProcessor,
            ILogService logService)

        {
			_modifyRecordService = modifyRecordService;
            _userActionBuilder = userActionBuilder;
            _rightsProcessor = rightsProcessor;
            _logService = logService;
            _backgroundSyncManager = AppContainer.Resolve<BackgroundSyncManager>();
            _dialogContorller = AppContainer.Resolve<IDialogContorller>();
            _navigationController = AppContainer.Resolve<INavigationController>();
            _localizationController = AppContainer.Resolve<ILocalizationController>();
        }

        public async Task PrepareModifyRecordServiceAsync(CancellationToken cancellationToken)
        {
            await _modifyRecordService.PrepareContentAsync(cancellationToken);
        }

        private async Task<UserAction> ModifyRecord(UserAction userAction, CancellationToken cancellationToken)
        {
            await _modifyRecordService.ModifyRecord(userAction, cancellationToken);
            return await _modifyRecordService.SavedAction(cancellationToken);
        }

        public async Task HandleModifyRecord(UserAction userAction, Func<SyncResult, Task> refreshFunction, bool shouldRemoveParent, CancellationToken cancellationToken)
        {
            try
            {
                IsModifyRecodServiceBusy = true;
                var rightsFilter = userAction?.GetRightsFilter();
                ParentLink rootRecord = _userActionBuilder.GetRootRecord(userAction);
                if (!string.IsNullOrWhiteSpace(rightsFilter))
                {
                    (bool result, string message) = await _rightsProcessor.EvaluateRightsFilter(rightsFilter, rootRecord?.RecordId, cancellationToken, true);
                    if (!result)
                    {
                        await _dialogContorller.ShowAlertAsync(
                            string.IsNullOrWhiteSpace(message) ? _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsRightsFilter) : message,
                            _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                            _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                        IsModifyRecodServiceBusy = false;
                        return;
                    }
                }

                UserAction savedAction = await ModifyRecord(userAction, cancellationToken);
                if (savedAction != null)
                {
                    IsModifyRecodServiceBusy = false;
                    if(shouldRemoveParent)
                    {
                        await _navigationController.NavigateAsyncForActionAndRemoveParent(savedAction, cancellationToken);
                    }
                    else
                    {
                        await _navigationController.NavigateAsyncForAction(savedAction, cancellationToken);
                    }
                    
                }
                else
                {
                    if(refreshFunction != null)
                    {
                        IsModifyRecodServiceBusy = false;
                        await refreshFunction(new SyncResult());
                    }

                    if(shouldRemoveParent)
                    {
                        await _navigationController.BackAsync();
                    }
                }
            }
            catch (CrmException e)
            {
                if (e.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                {
                    _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                }

                _logService.LogError("An error has occurred while modifying record.");
                await _dialogContorller.ShowAlertAsync(e.Content,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
            catch (Exception e)
            {
                _logService.LogError("An error has occurred while modifying record.");
                await _dialogContorller.ShowAlertAsync(e.Message,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
            IsModifyRecodServiceBusy = false;
        }
    }
}

