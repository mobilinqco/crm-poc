using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SerialEntryPageViewModel : NavigationBarBaseViewModel
    {
        private readonly ISerialEntryService _serialentryService;
        private readonly IRightsProcessor _rightsProcessor;
        private UserAction _selectedUserAction;
        private Dictionary<int, UIWidget> tabWidgets;

        public ICommand OnRelatedInfoAreaSelected => new Command<UserAction>(async (selectedItem) => await TabSelected(selectedItem));
        public ICommand OnCancleButtonTappedCommand => new Command(async () => await OnCancleButtonTapped());

        public SerialEntryPageViewModel(ISerialEntryService serialentryService, IRightsProcessor rightsProcessor)
        {
            _serialentryService = serialentryService;
            _rightsProcessor = rightsProcessor;
            tabWidgets = new Dictionary<int, UIWidget>();
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

        private ObservableCollection<UserAction> _tabItems;
        public ObservableCollection<UserAction> TabItems
        {
            get => _tabItems;
            set
            {
                _tabItems = value;
                RaisePropertyChanged(() => TabItems);
            }
        }

        private async Task TabSelected(UserAction selectedUserAction)
        {
            if (selectedUserAction != null && selectedUserAction != _selectedUserAction)
            {
                _logService.LogInfo($"TabSelected user action: {selectedUserAction.ActionDisplayName}");
                _selectedUserAction = selectedUserAction;
                ResetCancelationToken();
                foreach (UserAction ua in TabItems)
                {
                    ua.IsSelected = false;
                    if (ua == selectedUserAction)
                    {
                        ua.IsSelected = true;
                    }
                }
                
                await PrepareContent(_selectedUserAction);

            }
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            _logService.LogDebug("Start  InitializeAsync");
            if (navigationData is UserAction)
            {
                _serialentryService.SetSourceAction(navigationData as UserAction);
     
            }
            await UpdateBindingsAsync();
            await base.InitializeAsync(navigationData);
            _logService.LogDebug("End  InitializeAsync");
        }

        private async Task UpdateBindingsAsync()
        {
            TabItems = await _serialentryService.GetTabItems(_cancellationTokenSource.Token);
            PageTitle = _serialentryService.PageTitle();
            await TabSelected(TabItems[0]);
        }

        private async Task PrepareContent(UserAction selectedUserAction)
        {
            if (tabWidgets.ContainsKey(selectedUserAction.Id))
            {
                Content = null;
                IsLoading = true;
                Content = tabWidgets[selectedUserAction.Id];
                IsLoading = false;
            }
            else
            {
                HasEditRights = true;
                var (status, result, message) = await _rightsProcessor.EvaluateRightsFilter(selectedUserAction, _cancellationTokenSource.Token);
                if (status)
                {
                    if (!result)
                    {
                        HasEditRights = false;
                        RightsMessage = message;
                        IsLoading = false;
                        return;

                    }
                }

                Content = null;
                IsLoading = true;
                Content = await BuildTabContent(selectedUserAction);
                IsLoading = false;
            }
        }

        private async Task<UIWidget> BuildTabContent(UserAction selectedUserAction)
        {
            var widgetKey = selectedUserAction.ActionType.ToString();
            if (selectedUserAction.ViewReference.Name.StartsWith("ClientReport"))
            {
                widgetKey = selectedUserAction.ViewReference.Name;
            }

            var widget = await FormBuilderExtensions.BuildWidget(widgetKey, selectedUserAction, this, _cancellationTokenSource);
            tabWidgets.Add(selectedUserAction.Id, widget);
            return widget;

        }

        private async Task OnCancleButtonTapped()
        {
            await _navigationController.BackAsync();
        }
    }
}
