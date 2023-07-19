using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class LogModel : UIWidget
    {
        public ICommand FullScreenCommand => new Command(() => OnFullScreenRequested());
        public ICommand ScrolledCommand => new Command<ItemsViewScrolledEventArgs>((args) => Scrolled(args));
        public ICommand SendLogFileCommand => new Command(async () => await SendLogFile());
        public ICommand ResetLogFileCommand => new Command(() => ResetLogFile());

        private static string _appLogsPath = "";

        private static int pageNumber = 100;

        private int totalNumberOfLinesRead = 0;

        private string _logText;
        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                RaisePropertyChanged(() => LogText);
            }
        }

        private bool _isFullScreenButtonVisible;
        public bool IsFullScreenButtonVisible
        {
            get => _isFullScreenButtonVisible;
            set
            {
                _isFullScreenButtonVisible = value;
                RaisePropertyChanged(() => IsFullScreenButtonVisible);
            }
        }

        private string _fullScreenIconText;
        public string FullScreenIconText
        {
            get => _fullScreenIconText;
            set
            {
                _fullScreenIconText = value;
                RaisePropertyChanged(() => FullScreenIconText);
            }
        }

        private ObservableCollection<string> _appLogsStrings = new ObservableCollection<string>();
        public ObservableCollection<string> AppLogsStrings
        {
            get => _appLogsStrings;
            set
            {
                _appLogsStrings = value;
                RaisePropertyChanged(() => AppLogsStrings);
            }
        }

        private string _sendLogFileText;
        public string SendLogFileText
        {
            get => _sendLogFileText;
            set
            {
                _sendLogFileText = value;
                RaisePropertyChanged(() => SendLogFileText);
            }
        }

        private string _resetLogFileText;
        public string ResetLogFileText
        {
            get => _resetLogFileText;
            set
            {
                _resetLogFileText = value;
                RaisePropertyChanged(() => ResetLogFileText);
            }
        }

        public LogModel(bool isFullScreenButtonVisible, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            InitAppLogsFilePath();
            InitProperties(isFullScreenButtonVisible);
            ReadAppLogsStrings();
        }

        private void InitAppLogsFilePath()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Path suffix corresponds to Android NLog File target configuration  
                _appLogsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/logs/nlog.log";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Path suffix corresponds to iOS NLog File target configuration  
                _appLogsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/../Library/logs/nlog.log";
            }
            else if (DeviceInfo.Platform == DevicePlatform.UWP)
            {
                // Path suffix corresponds to UWP NLog File target configuration  
                _appLogsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/logs/nlog.log";
            }
        }

        private void InitProperties(bool isFullScreenButtonVisible)
        {
            IsFullScreenButtonVisible = isFullScreenButtonVisible;
            LogText = "Log"; // TODO Using localization
            FullScreenIconText = MaterialDesignIcons.ArrowExpandAll;
            SendLogFileText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSendLogPerEmail);
            ResetLogFileText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicResetLogfile);
        }

        private void Scrolled(ItemsViewScrolledEventArgs itemsViewScrolledEventArgs)
        {
            if (itemsViewScrolledEventArgs.LastVisibleItemIndex == _appLogsStrings.Count - 1)
            {
                ReadAppLogsStrings();
            }
        }

        private void ReadAppLogsStrings()
        {
            try
            {
                using (var logFile = new FileStream(_appLogsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(logFile))
                    {
                        int numberOfLinesRead = 0;

                        while (sr.Peek() >= 0 && numberOfLinesRead < totalNumberOfLinesRead + pageNumber)
                        {
                            if (totalNumberOfLinesRead != 0 && numberOfLinesRead < totalNumberOfLinesRead)
                            {
                                numberOfLinesRead++;
                                sr.ReadLine();
                            }
                            else
                            {
                                numberOfLinesRead++;
                                _appLogsStrings.Add(sr.ReadLine());
                            }
                        }

                        totalNumberOfLinesRead = numberOfLinesRead;

                        AppLogsStrings = _appLogsStrings;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void OnFullScreenRequested()
        {
            ParentBaseModel?.PublishMessage(new WidgetMessage
            {
                EventType = WidgetEventType.ShowAppLogsRequested,
                ControlKey = "LogModel"
            });
        }

        private async Task SendLogFile()
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = $"CRM.Client Log File for {_sessionContext.CrmInstance.Name}"
                };
                message.Attachments.Add(new EmailAttachment(_appLogsPath));
                await Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {

            }
        }

        private void ResetLogFile()
        {
            try
            {
                File.WriteAllText(_appLogsPath, string.Empty);
                _appLogsStrings.Clear();
                totalNumberOfLinesRead = 0;
                ReadAppLogsStrings();
            }
            catch (Exception ex)
            {

            }
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }
    }
}