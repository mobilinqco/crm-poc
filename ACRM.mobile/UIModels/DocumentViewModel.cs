using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;
using NLog.Filters;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class DocumentViewModel : UIWidget
    {
        private readonly ISearchContentService _contentService;
        private readonly IDocumentService _docService;
        private readonly ResetTimer timer;
        private readonly UserAction _userAction;

        public ICommand OnHeaderAction => new Command<HeaderActionButton>(async (selectedItem) => await HeaderAction(selectedItem));
        public ICommand SearchCommand => new Command(async () => PerformSearch(true));
        public ICommand SwitchRequestModeCommand => new Command(async () => await SwitchRequestMode());
        public ICommand DocumentDownloadCommand => new Command<DocumentObject>(async (selectedDoc) => await DownloadSelectedDoc(selectedDoc));

        private async Task DownloadSelectedDoc(DocumentObject selectedDoc)
        {
            await DocumentDownload.Download(selectedDoc, _docService, _sessionContext, _cancellationTokenSource.Token);
        }

        private ObservableCollection<DocumentObject> _documents;
        public ObservableCollection<DocumentObject> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                RaisePropertyChanged(() => Documents);
            }
        }

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

        private SearchAndListContentData _searchAndListContentData = new SearchAndListContentData();
        public SearchAndListContentData SearchAndListContentData
        {
            get => _searchAndListContentData;

            set
            {
                _searchAndListContentData = value;
                RaisePropertyChanged(() => SearchAndListContentData);
            }
        }

        private int SelectedTabId = 0;

        public DocumentViewModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<ISearchContentService>();
            _docService = AppContainer.Resolve<IDocumentService>();


            if (widgetArgs != null && widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
                _contentService.SetSourceAction(_userAction);
            }

            IsLoading = true;
            HeaderGroupData headerData = new HeaderGroupData();
            headerData.IsOrganizerHeaderVisible = false;
            HeaderData = headerData;
            this.timer = new ResetTimer(this.PerformAsyncSearch);
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (_contentService != null)
            {
                IsLoading = true;
                _logService.LogDebug("Start  InitializeAsync");
                if (_userAction != null)
                {
                    _userAction.ForceLinkId = 127;
                    _contentService.SetSourceAction(_userAction);
                    _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                    {
                        _logService.LogError($"Unable to prepare content {ex.Message}");
                    });
                }
                await UpdateBindingsAsync();
                _logService.LogDebug("End  InitializeAsync");
            }
            return true;
        }

        private async Task UpdateBindingsAsync()
        {
            _logService.LogDebug("Start UpdateBindingsAsync");

            // Prepare the page header data
            var headerData = HeaderData;
            headerData.InfoAreaColor = Color.FromHex(_contentService.PageAccentColor());

            List<UserAction> relatedInfoAreas = _contentService.HeaderRelatedInfoAreas();
            relatedInfoAreas[0].ActionDisplayName = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicTabAll);
            headerData.RelatedInfoAreas = relatedInfoAreas;
            if (headerData.RelatedInfoAreas.Count > 0)
            {
                headerData.SelectedRelatedInfoArea = headerData.RelatedInfoAreas[0];
            }
            headerData.AreActionsViewVisible = false;
            headerData.SetHeaderActionButtons(_contentService.HeaderButtons());
            if (headerData.HeaderActions.Count > 0)
            {
                headerData.AreActionsViewVisible = true;
            }
            HeaderData = headerData;
            await BindListData();
            _logService.LogDebug("End UpdateBindingsAsync");
        }

        private async Task BindListData()
        {
            // Prepare the search page data
            SearchAndListContentData searchData = new SearchAndListContentData();
            searchData.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
            searchData.SearchTextBoxPlaceholderText = _contentService.SearchColumns(SelectedTabId);
            searchData.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(SelectedTabId);
            SearchAndListContentData = searchData;

            // Perform the record fetch.
            PerformSearch();
        }

        private async Task HeaderAction(HeaderActionButton headerActionButton)
        {
            if (headerActionButton != null)
            {
                _logService.LogInfo($"SearchAndListPage header user action: {headerActionButton.UserAction.ActionDisplayName}");
                await _navigationController.SimpleNavigateWithAction(headerActionButton.UserAction);
            }
        }

        private void PerformSearch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    if (!SearchAndListContentData.OnlineMode
                        && _contentService.SearchAutoSwitchToOffline())
                    {
                        SearchAndListContentData.OnlineMode = false;
                    }

                    delay = _contentService.SearchDelay(!SearchAndListContentData.OnlineMode);
                }
                timer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        private async Task PerformAsyncSearch()
        {
            try
            {
                IsLoading = true;
                var salist = SearchAndListContentData;
                salist.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(SelectedTabId);
                salist.IsOnlinePossible = false;
                salist.EnableNoResultsText = false;
                Documents = null;
                SearchAndListContentData = salist;
                RequestMode requestMode = SearchAndListContentData.OnlineMode ? RequestMode.Online : RequestMode.Offline;
                ResetCancelationToken();
                if (await _contentService.PerformSearch(SelectedTabId, SearchAndListContentData.SearchText, requestMode, _cancellationTokenSource.Token))
                {
                    var salcd = SearchAndListContentData;
                    Documents = await _contentService.DocumentViewDataAsync(SelectedTabId, _cancellationTokenSource.Token);
                    salcd.EnableNoResultsText = !_contentService.HasResults(SelectedTabId);
                    if (salcd.EnableNoResultsText)
                    {
                        salcd.SearchResults.Clear();
                        salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                            LocalizationKeys.KeyErrorsNoResults);
                    }
                    salcd.OnlineMode = _contentService.AreResultsRetrievedOnline(SelectedTabId);
                    salcd.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
                    SearchAndListContentData = salcd;

                }
                else
                {
                    SetNoResultUI();
                }

                IsLoading = false;
            }
            catch (Exception e)
            {
                _logService.LogError(e?.Message);
                SetNoResultUI();
                IsLoading = false;
            }
        }

        private void SetNoResultUI()
        {
            var salcd = SearchAndListContentData;
            salcd.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(SelectedTabId);
            salcd.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
            salcd.SearchResults.Clear();
            salcd.EnableNoResultsText = true;
            salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                LocalizationKeys.KeyErrorsNoResults);
            SearchAndListContentData = salcd;
        }

        private async Task SwitchRequestMode()
        {
            var salcd = SearchAndListContentData;
            salcd.OnlineMode = !SearchAndListContentData.OnlineMode;
            SearchAndListContentData = salcd;
            PerformSearch();
        }

        public override async Task RefreshAsync(object data)
        {
            if (data is SyncResult)
            {
                PerformSearch();
            }
        }
    }
}
