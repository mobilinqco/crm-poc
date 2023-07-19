using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class ConflictListPageViewModel : NavigationBarBaseViewModel
    {
        private readonly IOfflineRequestsService _offlineRequestsService;
        private readonly IConfigurationService _configurationService;

        public ICommand SendConflictsFileCommand => new Command(async () => await SendConflictsFile());
        public ICommand ItemTappedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await OnItemTapped(evt));
        public ICommand DeleteItemCommand => new Command(async (args) => await OnDeleteItem(args));

        private string _emailAddress;

        private ObservableCollection<OfflineRequest> _offlineRequestsWithConflicts = new ObservableCollection<OfflineRequest>();
        public ObservableCollection<OfflineRequest> OfflineRequestsWithConflicts
        {
            get => _offlineRequestsWithConflicts;
            set
            {
                _offlineRequestsWithConflicts = value;
                RaisePropertyChanged(() => OfflineRequestsWithConflicts);
            }
        }

        private string _sendConflictsFileText;
        public string SendConflictsFileText
        {
            get => _sendConflictsFileText;
            set
            {
                _sendConflictsFileText = value;
                RaisePropertyChanged(() => SendConflictsFileText);
            }
        }

        public ConflictListPageViewModel()
        {

            _offlineRequestsService = AppContainer.Resolve<IOfflineRequestsService>();
            _configurationService = AppContainer.Resolve<IConfigurationService>();

            InitialiseConfigurationParameters();

            InitialiseProperties();
        }

        private void InitialiseConfigurationParameters()
        {
            var configValue = _configurationService.GetConfigValue("Sync.ConflictEmailAddress");
            if(configValue != null && configValue.Value is string emailAddress && !string.IsNullOrEmpty(emailAddress))
            {
                _emailAddress = emailAddress;
            }
        }

        private void InitialiseProperties()
        {
            IsLoading = true;
            IsConflictsButtonEnabled = false;
            IsBackButtonVisible = true;
            PageTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSyncConflictsTitle);
            SendConflictsFileText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSendMail);
        }

        public override async Task InitializeAsync(object data)
        {
            IsLoading = true;

            var offlineRequests = await _offlineRequestsService.GetAllRequests(_cancellationTokenSource.Token);

            foreach(OfflineRequest offlineRequest in offlineRequests)
            {
                if(!string.IsNullOrWhiteSpace(offlineRequest.ErrorMessage))
                {
                    _offlineRequestsWithConflicts.Add(offlineRequest);
                }
                else
                {
                    offlineRequest.ErrorMessage = "Unknown Error";
                    _offlineRequestsWithConflicts.Add(offlineRequest);
                }
            }

            OfflineRequestsWithConflicts = _offlineRequestsWithConflicts;

            IsLoading = false;
        }

        private async Task SendConflictsFile()
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = $"CRM.Client Conflicts File for {_sessionContext.CrmInstance.Name}"
                };

                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Conflicts.txt");

                using (var streamWriter = new StreamWriter(fileName, true))
                {
                    foreach (OfflineRequest offlineRequest in OfflineRequestsWithConflicts)
                    {
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(offlineRequest);
                        streamWriter.WriteLine(jsonString);
                    }
                }

                message.Attachments.Add(new EmailAttachment(fileName));

                if (!string.IsNullOrEmpty(_emailAddress) && _emailAddress != "#")
                {
                    message.To.Add(_emailAddress);
                }

                await Email.ComposeAsync(message);

                File.Delete(fileName);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task OnItemTapped(Syncfusion.ListView.XForms.ItemTappedEventArgs itemTappedEventArgs)
        {
            // TODO
        }

        private async Task OnDeleteItem(object args)
        {
            if(args is OfflineRequest offlineRequest)
            {
                IsLoading = true;

                await _offlineRequestsService.Delete(offlineRequest, _cancellationTokenSource.Token);
                _offlineRequestsWithConflicts.Remove(offlineRequest);

                _logService.LogDebug("Offline record has been deleted.");

                if (_offlineRequestsWithConflicts.Count == 0)
                {
                    _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(false);
                }

                IsLoading = false;
            }
        }
    }
}
