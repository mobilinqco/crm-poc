using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class DetailsPageViewModel: NavigationBarBaseViewModel
    {
        private readonly IDetailsContentService _contentService;
        private readonly IDocumentService _documentService;
        private readonly IRightsProcessor _rightsProcessor;
        private readonly IUserActionBuilder _userActionBuilder;
        private readonly ModifyRecordActivity _modifyRecordActivity;

        public ICommand OnRelatedInfoAreaSelected => new Command<UserAction>(async (selectedItem) => await RelatedInfoAreaSelected(selectedItem));
        public ICommand OnHeaderAction => new Command<HeaderActionButton>(async (selectedItem) => await HeaderAction(selectedItem));

        private HeaderGroupData _headerData;
        public HeaderGroupData HeaderData
        {
            get => _headerData;
            set
            {
                _headerData = value;
                RaisePropertyChanged(() => HeaderData);
            }
        }

        private UIWidget _content;
        public UIWidget Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }

        private CancellationTokenSource _tabCancellationTokenSource;
        private UserAction _currentUserAction;

        public DetailsPageViewModel(IDetailsContentService contentService,
            IDocumentService docuemntService,
            IRightsProcessor rightsProcessor,
            IUserActionBuilder userActionBuilder,
            ModifyRecordActivity modifyRecordActivity)
        {
            _contentService = contentService;
            _documentService = docuemntService;
            _rightsProcessor = rightsProcessor;
            _userActionBuilder = userActionBuilder;
            _modifyRecordActivity = modifyRecordActivity;
            this.IsLoading = true;
            _contentService.DataReady += OnDataReady;
            RegisterMessageIfNotExist(WidgetEventType.InsightItemActionForParent, string.Empty, OnInsightItemActionForParent);
        }

        private void OnDataReady(object sender, EventArgs e)
        {
            UpdateBindingsAsync();
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsBackButtonVisible = true;
            IsLoading = true;
            if (navigationData is UserAction)
            {
                _contentService.SetSourceAction(navigationData as UserAction);
                _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to prepare content {ex.Message}");
                });
            }
            await base.InitializeAsync(navigationData);
        }

        private async void UpdateBindingsAsync()
        {
            PageTitle = _contentService.PageTitle();
            foreach(UserAction userAction in _contentService.HeaderButtons())
            {
                if(userAction.ActionUnitName.Equals("ToggleFavorite"))
                {
                    if(_contentService.IsRecordFavorite())
                    {
                        userAction.ActionDisplayName = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesDeleteFromFavorites);
                    }
                    else
                    {
                        userAction.ActionDisplayName = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesAddToFavorites);
                    }
                }
            }
            await UpdateHeaderBindings();
            await PrepareContent(HeaderData.RelatedInfoAreas[0]);
        }

        private async Task UpdateHeaderBindings()
        {
            HeaderGroupData hgd = new HeaderGroupData();
            hgd.InfoAreaColor = Color.FromHex(_contentService.PageAccentColor());
            var relatedInfoAreas = _contentService.HeaderRelatedInfoAreas();
            if (relatedInfoAreas[0].ActionDisplayName.Equals("Overview"))
            {
                relatedInfoAreas[0].ActionDisplayName = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                    LocalizationKeys.KeyBasicTabOverview);
            }
            hgd.RelatedInfoAreas = relatedInfoAreas;
            hgd.IsHeaderVisible = false;
            hgd.IsInfoAreaHeaderImageVisible = false;
            hgd.IsHeaderTableCaptionVisible = false;
            hgd.IsOrganizerHeaderVisible = false;

            string headerImage = _contentService.HeaderImageName();
            if (headerImage != null)
            {
                hgd.IsHeaderVisible = true;
                hgd.IsInfoAreaHeaderImageVisible = true;

                if (!string.IsNullOrEmpty(headerImage))
                {
                    // TODO: ACRM-480 This may need more extracting to get the image from the D1 and D3 info areas.

                    string imagePath = await _documentService.GetDocumentPath(headerImage, _cancellationTokenSource.Token);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        hgd.InfoAreaHeaderImageSource = ImageSource.FromFile(imagePath);
                    }
                    else
                    {
                        ResourceDictionary StaticResources = Application.Current.Resources;
                        hgd.InfoAreaHeaderImageSource = new FontImageSource
                        {
                            FontFamily = (OnPlatform<string>)StaticResources["HeaderButtonImagesFont"],
                            Glyph = "\uE060",
                            Color = Color.LightSkyBlue
                        };
                    }
                }
                else
                {
                    ResourceDictionary StaticResources = Application.Current.Resources;

                    hgd.InfoAreaHeaderImageSource = new FontImageSource
                    {
                        FontFamily = (OnPlatform<string>)StaticResources["HeaderButtonImagesFont"],
                        Glyph = "\uE060",
                        Color = Color.LightSkyBlue
                    };
                }
            }

            ListDisplayRow headerText = _contentService.OrganizerHeaderSubText();
            if (headerText != null && headerText.Fields != null && headerText.Fields.Count > 0)
            {
                hgd.IsHeaderVisible = true;
                hgd.IsOrganizerHeaderVisible = true;
                hgd.OrganizerHeaderSubText = _localizationController.GetLocalizedValue(headerText.Fields[0]);
            }

            ListDisplayRow tableCaption = _contentService.HeaderTableCaptionText();
            if (tableCaption != null)
            {
                string headerTableCaptionText = _localizationController.GetLocalizedValue(tableCaption.Fields[0]);
                if (headerTableCaptionText != null)
                {
                    hgd.IsHeaderVisible = true;
                    hgd.IsHeaderTableCaptionVisible = true;
                    hgd.HeaderTableCaptionText = headerTableCaptionText;
                }
            }

            hgd.AreActionsViewVisible = false;
            hgd.SetHeaderActionButtons(_contentService.HeaderButtons());
            if (hgd.HeaderActions.Count > 0)
            {
                hgd.AreActionsViewVisible = true;
            }

            hgd.IsOnlineRecord = _contentService.IsOnlineRecord();
            HeaderData = hgd;
    
        }

        private async Task RelatedInfoAreaSelected(UserAction selectedUserAction)
        {
            if (selectedUserAction != null && selectedUserAction != HeaderData.SelectedRelatedInfoArea)
            {
                _logService.LogInfo($"SearchAndListPage user action: {selectedUserAction.ActionDisplayName}");
                List<UserAction> userActions = new List<UserAction>();
                HeaderGroupData hgd = HeaderData;

                foreach (UserAction ua in hgd.RelatedInfoAreas)
                {
                    ua.IsSelected = false;
                    if (ua == selectedUserAction)
                    {
                        ua.IsSelected = true;
                        hgd.SelectedRelatedInfoArea = ua;
                    }

                    userActions.Add(ua);
                }
                hgd.RelatedInfoAreas = userActions;
                HeaderData = hgd;
                await PrepareContent(selectedUserAction);
            }
        }

        private async Task PrepareContent(UserAction selectedUserAction)
        {
            _currentUserAction = selectedUserAction;
            CancelWidgetsLoading();
            Content = null;
            IsLoading = true;
            var cnt = await BuildTabContent(selectedUserAction);
            if(_currentUserAction == selectedUserAction)
            {
                Content = cnt;
            }
            IsLoading = false;
        }

        private async Task<UIWidget> BuildTabContent(UserAction selectedUserAction)
        {
            // EventSubscriptions.Clear();
            _tabCancellationTokenSource = new CancellationTokenSource();
            return await FormBuilderExtensions.BuildWidget(selectedUserAction.ActionType.ToString(), selectedUserAction, this, _tabCancellationTokenSource);
        }

        private async Task HeaderAction(HeaderActionButton headerActionButton)
        {
            if (headerActionButton != null)
            {
                _logService.LogInfo($"DetailsPage header user action: {headerActionButton.UserAction.ActionDisplayName}");
                if (headerActionButton.UserAction.IsToggleFavorite())
                {
                    if (!_contentService.IsFavoriteServiceBusy())
                    {
                        await HandleFavoriteStatus(headerActionButton);
                    }
                }
                else
                {
                    await PerformAction(headerActionButton.UserAction);
                }
            }
        }

        private async Task PerformAction(UserAction userAction)
        {
            if (userAction.IsToggleFavorite())
            {
                if (!_contentService.IsFavoriteServiceBusy())
                {
                    await HandleFavoriteStatus(userAction);
                }
            }
            else if (userAction.ViewReference != null && userAction.ViewReference.IsModifyRecordAction())
            {
                if (!_modifyRecordActivity.IsModifyRecodServiceBusy)
                {
                    await _modifyRecordActivity.PrepareModifyRecordServiceAsync(_cancellationTokenSource.Token);
                    await _modifyRecordActivity.HandleModifyRecord(userAction, RefreshAsync, false, _cancellationTokenSource.Token);
                }
            }
            else if (userAction.ViewReference != null && userAction.ViewReference.IsSyncRecordAction())
            {
                if (!_contentService.IsSyncRecordServiceBusy())
                {
                    await HandleSyncRecord(userAction);

                }
                else
                {
                    await _dialogContorller.ShowAlertAsync(_localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesSyncing)
                        , "",
                        _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                }
            }
            else if (userAction.IsDeleteAction())
            {
                IsBusy = true;
                IsLoading = true;
                await HandleDeleteRecord(userAction);
            }
            else
            {
                IUserActionSchuttle userActionSchuttle = AppContainer.Resolve<IUserActionSchuttle>();
                await userActionSchuttle.Carry(userAction, userAction.RecordId, _cancellationTokenSource.Token);
            }
        }

        private async Task HandleSyncRecord(UserAction userAction)
        {
            try
            {
                _dialogContorller.ShowProgress(_localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesSyncing));
                await _contentService.SyncRecord(userAction, _cancellationTokenSource.Token);
                await RefreshAsync(new SyncResult());
                _dialogContorller.HideProgress();
                await _dialogContorller.ShowAlertAsync(_localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesRecordDataWasSync)
                    , "",
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose)); 
            }
            catch (CrmException e)
            {
                _dialogContorller.HideProgress();
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
                _dialogContorller.HideProgress();
                _logService.LogError("An error has occurred while modifying record.");
                await _dialogContorller.ShowAlertAsync(e.Message,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
            
        }

        private async Task HandleDeleteRecord(UserAction userAction)
        {
            try
            {
                bool shouldDelete = true;
                var rightsFilter = userAction?.GetRightsFilter();
                ParentLink rootRecord = _userActionBuilder.GetRootRecord(userAction);
                if (!string.IsNullOrEmpty(rightsFilter))
                {
                    var (result, message) = await _rightsProcessor.EvaluateRightsFilter(rightsFilter, rootRecord?.RecordId, _cancellationTokenSource.Token, true);
                    if (!result)
                    {
                        shouldDelete = false;
                        await _dialogContorller.ShowAlertAsync(message,
                            _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                            _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));

                    }
                }

                if (shouldDelete)
                {
                    shouldDelete = await _dialogContorller.ShowConfirm(
                                _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDeleteRecordMessage),
                                _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDeleteRecordTitle),
                                _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDelete),
                                _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel));
                    if (shouldDelete)
                    {

                        var deleteWarningFilterName = userAction?.GetDeleteWarningFilter();
                        if (!string.IsNullOrEmpty(deleteWarningFilterName))
                        {
                            var (result, message) = await _rightsProcessor.EvaluateRightsFilter(deleteWarningFilterName, rootRecord?.RecordId, _cancellationTokenSource.Token, false);

                            if (result)
                            {
                                shouldDelete = await _dialogContorller.ShowConfirm(
                                    message,
                                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDeleteRecordTitle),
                                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDelete),
                                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel));

                            }
                        }

                        if (shouldDelete)
                        {
                            await _contentService.DeleteRecord(userAction, _cancellationTokenSource.Token);
                            _cancellationTokenSource.Cancel();
                            await _navigationController.BackAsync(true);
                        }
                    }
                }
                IsLoading = false;
            }
            catch (CrmException e)
            {
                if (e.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                {
                    _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                }

                _logService.LogError("An error has occurred while deleting record.");
                await _dialogContorller.ShowAlertAsync(e.Content,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                IsLoading = false;
            }
            catch (Exception e)
            {
                _logService.LogError("An error has occurred while deleting record.");
                await _dialogContorller.ShowAlertAsync(e.Message,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                IsLoading = false;
            }
        }

        private async Task<bool> HandleFavoriteStatus(UserAction userAction)
        {
            bool isFavourite = _contentService.IsRecordFavorite();
            try
            {
                isFavourite = await _contentService.HandleFavoriteStatus(_cancellationTokenSource.Token);
            }
            catch (CrmException e)
            {
                if (e.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                {
                    _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                }

                _logService.LogError("An error has occurred while handling favorite status.");
                await _dialogContorller.ShowAlertAsync(e.Content,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
            catch (Exception e)
            {
                _logService.LogError("An error has occurred while handling favorite status.");
                await _dialogContorller.ShowAlertAsync(e.Message,
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitle),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }

            return isFavourite;
        }


        private async Task HandleFavoriteStatus(HeaderActionButton headerActionButton)
        {
            if (await HandleFavoriteStatus(headerActionButton.UserAction))
            {
                (string imageName, string glyphText) = _contentService.GetAddToFavoritesImageName();
                headerActionButton.UpdateUserActionImage(imageName, glyphText);
                headerActionButton.UpdateUserActionDisplayName(
                    _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesDeleteFromFavorites));
            }
            else
            {
                (string imageName, string glyphText) = _contentService.GetRemoveFromFavoritesImageName();
                headerActionButton.UpdateUserActionImage(imageName, glyphText);
                headerActionButton.UpdateUserActionDisplayName(
                    _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesAddToFavorites));
            }
        }

        public override async Task RefreshAsync(object data)
        {
            if (data is SyncResult)
            {
                IsLoading = true;
                _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to prepare content {ex.Message}");
                });
            }
        }

        private async Task OnInsightItemActionForParent(WidgetMessage arg)
        {
            if (arg != null)
            {
                if (arg.Data is UserAction userAction)
                {
                    if (userAction != null)
                    {
                       await PerformAction(userAction);
                    }
                }
            }
        }

        protected override void CancelWidgetsLoading()
        {
            if(_tabCancellationTokenSource != null && !_tabCancellationTokenSource.IsCancellationRequested)
            {
                _tabCancellationTokenSource.Cancel();
            }

            if (Widgets != null)
            {
                foreach (var widget in Widgets)
                {
                    widget.Cancel();
                }
            }

            if(Content != null)
            {
                Content.CancelChilds();
                Content.Cancel();
            }
        }
    }
}
