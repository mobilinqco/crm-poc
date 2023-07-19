using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class LanguageSelectionPageViewModel: BaseViewModel
    {
        public readonly IAuthenticationService _authenticationService;
        public readonly ICacheService _cacheService;
        public readonly ISyncStatusService _syncStatusService;

        public ICommand OnCloseButtonTapped => new Command(() =>
        {
            _navigationController.PopPopupAsync();
        });

        public ICommand LanguageSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(evt => LanguageSelected(evt));

        public ICommand OnApply => new Command<ServerLanguage>(async selectedLanguage => await OnApplySelectedLanguage());

        private ObservableCollection<ServerLanguage> _availableLanguages;
        public ObservableCollection<ServerLanguage> AvailableLanguages
        {
            get => _availableLanguages;
            set
            {
                _availableLanguages = value;
                RaisePropertyChanged(() => AvailableLanguages);
            }
        }

        private string _cancelButtonText;
        public string CancelButtonText
        {
            get => _cancelButtonText;
            set
            {
                _cancelButtonText = value;
                RaisePropertyChanged(() => CancelButtonText);
            }
        }

        private string _applyButtonText;
        public string ApplyButtonText
        {
            get => _applyButtonText;
            set
            {
                _applyButtonText = value;
                RaisePropertyChanged(() => ApplyButtonText);
            }
        }

        private bool _isApplyEnabled;
        public bool IsApplyEnabled
        {
            get => _isApplyEnabled;
            set
            {
                _isApplyEnabled = value;
                RaisePropertyChanged(() => IsApplyEnabled);
            }
        }


        public LanguageSelectionPageViewModel(IAuthenticationService authenticationService,
            ISyncStatusService syncStatusService,
            ICacheService cacheService) 
        {
            _authenticationService = authenticationService;
            _cacheService = cacheService;
            _syncStatusService = syncStatusService;

            ApplyButtonText = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginApplyLanguage);
            CancelButtonText = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginExit);
            IsApplyEnabled = false;
        }

        public override Task InitializeAsync(object data)
        {
            IsApplyEnabled = false;
            List<ServerLanguage> serverLanguages = (List<ServerLanguage>)data;

            serverLanguages.Sort((sl1, sl2) => sl1.OrderLangId.CompareTo(sl2.OrderLangId));
            if(_sessionContext.User?.SessionInformation?.ServerUserDefaultLanguage is string defaultServerLanguage)
            {
                foreach (ServerLanguage sl in serverLanguages)
                {
                    sl.IsSelected = false;
                    if (sl.Code == defaultServerLanguage)
                    {
                        sl.IsSelected = true;
                        IsApplyEnabled = true;
                    }
                }
            }

            AvailableLanguages = new ObservableCollection<ServerLanguage>(serverLanguages);

            return RefreshAsync(true);
        }

        private void LanguageSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            IsApplyEnabled = false;
            if (evt.ItemData is ServerLanguage language)
            {
                List<ServerLanguage> languages = new List<ServerLanguage>();
                foreach (ServerLanguage sl in AvailableLanguages)
                {
                    sl.IsSelected = false;
                    if (sl.Code == language.Code)
                    {
                        sl.IsSelected = true;
                        sl.Name = "Bula";
                        IsApplyEnabled = true;
                    }
                    languages.Add(sl);
                }

                AvailableLanguages = new ObservableCollection<ServerLanguage>(languages);
            }
        }

        private ServerLanguage SelectedServerLanguage()
        {
            foreach (ServerLanguage sl in _availableLanguages)
            {
                if (sl.IsSelected)
                {
                    return sl;
                }
            }

            return null;
        }

        private async Task OnApplySelectedLanguage()
        {
            ServerLanguage language = SelectedServerLanguage();

            if(language == null)
            {
                return;
            }
            IsApplyEnabled = false;

            await _syncStatusService.SetLanguageAsync(language);

            _cacheService.ResetCache();
            _localizationController.ResetConfiguration();

            var authenticationResponse = await _authenticationService.Authenticate(_sessionContext.CrmInstance, _sessionContext.User.Username, _sessionContext.User.Password, language.Code);

            _sessionContext.SessionCookies = authenticationResponse.Cookies;
            _sessionContext.LanguageCode = language.Code;
            _sessionContext.User = new User(_sessionContext.User.Username, _sessionContext.User.Password, authenticationResponse.SessionInformation);
            _sessionContext.CrmInstance.RevolutionRuntimeUrl = authenticationResponse.RedirectionUrl;

            _dialogContorller.HideProgress();

            if (_sessionContext.IsAuthenticated())
            {
                await _navigationController.PopPopupAsync();
                await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncFull, UseForce = true });
            }
            else
            {
                IsApplyEnabled = true;
                await ShowErrorMessageAsync(LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                        LocalizationKeys.KeyLoginErrorMessageCheckLoginData);
            }
        }

        private async Task ShowErrorMessageAsync(int titleKey, int messageKey)
        {
            await _dialogContorller.ShowAlertAsync(
                       _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           messageKey),
                       _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           titleKey),
                       _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicClose));
        }
    }
}
