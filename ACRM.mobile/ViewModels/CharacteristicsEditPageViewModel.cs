using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Characteristics;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups.Characteristics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class CharacteristicsEditPageViewModel : NavigationBarBaseViewModel
    {
        public ICommand ItemTappedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>((args) => OnItemTapped(args));
        public ICommand OnCancelCommand => new Command(async () => await OnCancel());
        public ICommand OnSaveCommand => new Command(async () => await OnSave());

        private readonly ICharacteristicsContentService _contentService;

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

        public ObservableCollection<ErrorInfo> ErrorsInfo { get; set; }

        private ObservableCollection<BindableCharacteristicGroup> _bindableCharacteristicGroups = new ObservableCollection<BindableCharacteristicGroup>();
        public ObservableCollection<BindableCharacteristicGroup> BindableCharacteristicGroups
        {
            get => _bindableCharacteristicGroups;
            set
            {
                _bindableCharacteristicGroups = value;
                RaisePropertyChanged(() => BindableCharacteristicGroups);
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

        public CharacteristicsEditPageViewModel()
        {
            _contentService = AppContainer.Resolve<ICharacteristicsContentService>();
            ErrorsInfo = new ObservableCollection<ErrorInfo> { new ErrorInfo() };
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsBackButtonVisible = true;
            IsLoading = true;
            IsSaveButtonEnabled = false;
            if (navigationData is UserAction)
            {
                IsBusy = true;
                _contentService.SetSourceAction(navigationData as UserAction);
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                UpdateBindings();
                IsBusy = false;
            }
            IsLoading = false;
            IsSaveButtonEnabled = true;
            await base.InitializeAsync(navigationData);
        }

        private void UpdateBindings()
        {
            PageTitle = _contentService.PageTitle();
            InfoAreaColor = Color.FromHex(_contentService.PageAccentColor());

            List<CharacteristicGroup> characteristicGroups = _contentService.GetEditableCharacteristicGroups();
            foreach (CharacteristicGroup characteristicGroup in characteristicGroups)
            {
                _bindableCharacteristicGroups.Add(new BindableCharacteristicGroup(characteristicGroup));
            }
            BindableCharacteristicGroups = _bindableCharacteristicGroups;

            CancelButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);
            SaveButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSave);
        }

        private void OnItemTapped(Syncfusion.ListView.XForms.ItemTappedEventArgs itemTappedEventArgs)
        {
            if (itemTappedEventArgs.ItemData is BindableCharacteristicItem bindableCharacteristicItem)
            {
                bindableCharacteristicItem.IsSelected = !bindableCharacteristicItem.IsSelected;
            }
        }

        private async Task OnSave()
        {
            if(!IsLoading)
            {
                IsLoading = true;
                IsSaveButtonEnabled = false;
                foreach (BindableCharacteristicGroup bindableCharacteristicGroup in BindableCharacteristicGroups)
                {
                    foreach (BindableCharacteristicItem bindableCharacteristicItem in bindableCharacteristicGroup.BindableCharacteristicItems)
                    {
                        try
                        {
                            if (bindableCharacteristicItem.IsItemAdded())
                            {
                                await _contentService.Save(bindableCharacteristicItem.GetCurrentFieldValues(), bindableCharacteristicItem.GetOldFieldValues(), 
                                    bindableCharacteristicItem.RecordId,  _cancellationTokenSource.Token);
                            }
                            else if (bindableCharacteristicItem.IsItemRemoved())
                            {
                                await _contentService.Delete(bindableCharacteristicItem.RecordId, _cancellationTokenSource.Token);
                            }
                            else
                            {
                                foreach (BindableCharacteristicsAdditionalValue additionalValue in bindableCharacteristicItem.BindableAdditionalValues)
                                {
                                    if (additionalValue.IsContentModified())
                                    {
                                        await _contentService.Save(bindableCharacteristicItem.GetCurrentFieldValues(), bindableCharacteristicItem.GetOldFieldValues(), 
                                            bindableCharacteristicItem.RecordId, _cancellationTokenSource.Token);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (CrmException ex)
                        {
                            if (ex.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                            {
                                _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                            }

                            _logService.LogError("Error saving the record.");
                            IsErrorMessageVisible = true;
                            ErrorsInfo[0].Name = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                            ErrorsInfo[0].Description = $"{ex.Content}";
                        }
                        catch (Exception ex)
                        {
                            _logService.LogError("Error saving the record to the network.");
                            IsErrorMessageVisible = true;
                            ErrorsInfo[0].Name = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                            ErrorsInfo[0].Description = $"{ex.Message}";
                        }
                    }
                }
                IsSaveButtonEnabled = true;
                IsLoading = false;

                if(!IsErrorMessageVisible)
                {
                    await _navigationController.BackAsync();
                }
            }
        }

        private async Task OnCancel()
        {
            await _navigationController.BackAsync();
        }
    }
}
