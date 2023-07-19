using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.UIModels;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class CalendarPageViewModel : NavigationBarBaseViewModel
    {
        readonly private ICalendarContentService _contentService;
        public ICommand OnHeaderAction => new Command<HeaderActionButton>((selectedItem) => HeaderAction(selectedItem));
        private HeaderGroupData _headerData = new HeaderGroupData();
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

        public CalendarPageViewModel(ICalendarContentService contentService)
        {
            _contentService = contentService;
            IsBackButtonVisible = true;
            _headerData.IsOrganizerHeaderVisible = false;
            HeaderData = _headerData;
            IsLoading = true;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is UserAction)
            {
                _contentService.SetSourceAction(navigationData as UserAction);
                _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to prepare content {ex.Message}");
                });
            }
            SetHeaderData();
            
            if (navigationData is UserAction ua)
            {
                PageTitle = ua.ActionDisplayName;
            }
            Content = await Utils.FormBuilderExtensions.BuildWidget("Calendar", navigationData, this, _cancellationTokenSource);
            IsLoading = false;
            await base.InitializeAsync(navigationData);
        }

        public override async Task RefreshAsync(object data)
        {
            if (data is SyncResult && Content is CalendarControlModel model)
            {
                model.PerformSearch();
            }
        }

        private void HeaderAction(HeaderActionButton headerActionButton)
        {
            if (headerActionButton != null)
            {
                _logService.LogInfo($"DetailsPage header user action: {headerActionButton.UserAction.ActionDisplayName}");
                _navigationController.NavigateAsyncForAction(headerActionButton.UserAction, _cancellationTokenSource.Token);
            }
        }

        private void SetHeaderData()
        {
            var headerGroupData = new HeaderGroupData();
            headerGroupData.InfoAreaColor = Color.FromHex(_contentService.PageAccentColor());

            headerGroupData.AreActionsViewVisible = false;
            headerGroupData.SetHeaderActionButtons(_contentService.HeaderButtons());
            if (headerGroupData.HeaderActions.Count > 0)
            {
                headerGroupData.AreActionsViewVisible = true;
            }
            HeaderData = headerGroupData;
        }
    }
}
