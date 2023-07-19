using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using Xamarin.Essentials;

namespace ACRM.mobile.ViewModels.Base
{
    public class BaseViewModel: ExtendedBindableObject
    {
        protected readonly IDialogContorller _dialogContorller;
        protected readonly INavigationController _navigationController;
        protected readonly ILocalizationController _localizationController;
        protected readonly ILogService _logService;
        protected readonly ISessionContext _sessionContext;
        protected CancellationTokenSource _cancellationTokenSource;
        public BaseViewModel ParentBaseModel { get; set; }
        public bool NeedRefreshOnBack { get; set; } = false;

        private ObservableCollection<UIWidget> _widgets;
        public ObservableCollection<UIWidget> Widgets
        {
            get => _widgets;
            set
            {
                _widgets = value;
                RaisePropertyChanged(() => Widgets);
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }

        private string _errorMessageText;
        public string ErrorMessageText
        {
            get => _errorMessageText;
            set
            {
                _errorMessageText = value;
                RaisePropertyChanged(() => ErrorMessageText);
            }
        }

        private List<WidgetEventSubscription> _eventSubscriptions = new List<WidgetEventSubscription>();
        internal List<WidgetEventSubscription> EventSubscriptions
        {
            get => _eventSubscriptions;
            set
            {
                _eventSubscriptions = value;
                RaisePropertyChanged(() => EventSubscriptions);
            }
        }

        public BaseViewModel()
        {
            _dialogContorller = AppContainer.Resolve<IDialogContorller>();
            _navigationController = AppContainer.Resolve<INavigationController>();
            _localizationController = AppContainer.Resolve<ILocalizationController>();
            _logService = AppContainer.Resolve<ILogService>();
            _sessionContext = AppContainer.Resolve<ISessionContext>();
            Connectivity.ConnectivityChanged += NetworkConnectivityChanged;
            ValidateNetworkConnectivity(Connectivity.NetworkAccess);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public string UserActionNotSupportedErrorMessage(UserAction userAction)
        {
            string type = userAction.ActionType.ToString();
            if (userAction.ViewReference != null && userAction.ViewReference.Name != null)
            {
                type = userAction.ViewReference.Name;
            }

            return _localizationController.GetFormatedString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicUserActionNotImplementedError,
                userAction.ActionDisplayName,
                type);
        }

        public string UserActionNotSupportedErrorMessage(string key, string type)
        {
            return _localizationController.GetFormatedString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicUserActionNotImplementedError,
                key,
                type);
        }

        protected void ResetCancelationToken()
        {
            if (Widgets != null)
            {
                foreach (var widget in Widgets)
                {
                    widget.ResetCancelationToken();
                }
            }

            if (_cancellationTokenSource != null &&
                !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public virtual Task InitializeAsync(object data)
        {
            return Task.FromResult(false);
        }

        public virtual Task RefreshAsync(object data)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> CanClose()
        {
            return Task.FromResult(true);
        }

        public void Cancel()
        {
            if (Widgets != null)
            {
                foreach (var widget in Widgets)
                {
                    widget.Cancel();
                }
            }

            _cancellationTokenSource.Cancel();
        }

        private void NetworkConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            ValidateNetworkConnectivity(e.NetworkAccess);
        }

        private void ValidateNetworkConnectivity(NetworkAccess access)
        {
            if (access == NetworkAccess.Internet || access == NetworkAccess.Local)
            {
                _sessionContext.HasNetworkConnectivity = true;
            }
        }

        internal void RegisterMessage(WidgetEventType eventType, string controlKey, Func<WidgetMessage, Task> messageHandler)
        {
            EventSubscriptions.Add(new WidgetEventSubscription(eventType, controlKey, messageHandler));
        }

        internal void RegisterMessageIfNotExist(WidgetEventType eventType, string controlKey, Func<WidgetMessage, Task> messageHandler)
        {
            var lowerKey = controlKey?.ToLower();
            var hasEventItem = EventSubscriptions.Any(a => a.EventType.Equals(eventType) && a.ControlKey.Equals(lowerKey) && a.MessageHandler.Equals(messageHandler));
            if (!hasEventItem)
            {
                EventSubscriptions.Add(new WidgetEventSubscription(eventType, lowerKey, messageHandler));
            }
        }

        internal async Task PublishMessage(WidgetMessage message, MessageDirections direction = MessageDirections.ToParent)
        {
            var lowerKey = message.ControlKey?.ToLower();
            if (message != null)
            {
                switch (direction)
                {
                    case MessageDirections.ToChildren:
                        {
                            var Subscriptions = EventSubscriptions.Where(a => (a.ControlKey == lowerKey || a.ControlKey == "*")
                            && a.EventType == message.EventType).ToList();
                            foreach (var Subscription in Subscriptions)
                            {
                                if (Subscription != null)
                                {
                                    await Subscription?.MessageHandler(message);
                                }

                            }
                            if (Widgets != null)
                            {
                                foreach (var widget in Widgets)
                                {
                                    await widget.PublishMessage(message, MessageDirections.ToChildren);
                                }
                            }
                            break;
                        }
                    case MessageDirections.ToParent:
                        {
                            var Subscriptions = EventSubscriptions.Where(a => (a.ControlKey == lowerKey || a.ControlKey == "*")
                            && a.EventType == message.EventType).ToList();
                            foreach (var Subscription in Subscriptions)
                            {
                                if (Subscription != null)
                                {
                                    await Subscription?.MessageHandler(message);
                                }
                            }
                            if (ParentBaseModel != null)
                            {
                                await ParentBaseModel.PublishMessage(message, MessageDirections.ToParent);
                            }
                            break;
                        }
                    case MessageDirections.Both:
                        {
                            if (ParentBaseModel != null)
                            {
                                await ParentBaseModel.PublishMessage(message, MessageDirections.Both);
                            }
                            else
                            {
                                await PublishMessage(message, MessageDirections.ToChildren);
                            }
                            break;
                        }
                }

            }
        }        
    }
}

