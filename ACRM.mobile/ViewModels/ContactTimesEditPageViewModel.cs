using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ContactTimes;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Syncfusion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class ContactTimesEditPageViewModel : NavigationBarBaseViewModel
    {
        public ICommand ItemTappedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>((args) => OnItemTapped(args));
        public ICommand SwipeStartedCommand => new Command<Syncfusion.ListView.XForms.SwipeStartedEventArgs>((args) => OnSwipeStarted(args));
        public ICommand SwipeEndedCommand => new Command<Syncfusion.ListView.XForms.SwipeEndedEventArgs>((args) => OnSwipeEnded(args));
        public ICommand OnCancelCommand => new Command(async () => await OnCancel());
        public ICommand OnSaveCommand => new Command(async () => await OnSave());

        private readonly IContactTimesContentService _contentService;

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

        private ObservableCollection<ContactTimesType> _contactTimesTypes = new ObservableCollection<ContactTimesType>();
        public ObservableCollection<ContactTimesType> ContactTimesTypes
        {
            get => _contactTimesTypes;
            set
            {
                _contactTimesTypes = value;
                RaisePropertyChanged(() => ContactTimesTypes);
            }
        }

        private string _deleteText;
        public string DeleteText
        {
            get
            {
                return _deleteText;
            }

            set
            {
                _deleteText = value;
                RaisePropertyChanged(() => DeleteText);
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

        public ContactTimesEditPageViewModel()
        {
            _contentService = AppContainer.Resolve<IContactTimesContentService>();
            ErrorsInfo = new ObservableCollection<ErrorInfo> { new ErrorInfo() };

            MessagingCenter.Subscribe<BaseViewModel, ContactTimesIntervalSelectionData>(this, InAppMessages.ContactTimesDayAbbreviations, OnContactTimesIntervalSelectionDataReceived);
        }

        private void OnContactTimesIntervalSelectionDataReceived(BaseViewModel caller, ContactTimesIntervalSelectionData contactTimesIntervalSelectionData)
        {
            _contentService.UpdateContactTimesTypeData(contactTimesIntervalSelectionData);
            UpdateContactTimesTypes(); // Get updated ContactTimesTypes
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
                _contentService.SetWeekDayNames(GetWeekDayNames());
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                UpdateBindings();
                IsBusy = false;
            }
            IsLoading = false;
            IsSaveButtonEnabled = true;
            await base.InitializeAsync(navigationData);
        }

        private List<string> GetWeekDayNames()
        {
            return new List<string>()
            {
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysMonday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysTuesday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysWednesday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysThursday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysFriday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysSaturday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysSunday)
            };
        }

        private void UpdateBindings()
        {
            PageTitle = _contentService.PageTitle();
            InfoAreaColor = Color.FromHex(_contentService.PageAccentColor());

            UpdateContactTimesTypes();

            DeleteText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDelete);
            CancelButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);
            SaveButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSave);
        }

        private void UpdateContactTimesTypes()
        {
            _contactTimesTypes.Clear();
            foreach (ContactTimesType contactTimesType in _contentService.GetContactTimesTypes())
            {
                _contactTimesTypes.Add(contactTimesType);
            }
            ContactTimesTypes = _contactTimesTypes;
        }

        private void OnItemTapped(Syncfusion.ListView.XForms.ItemTappedEventArgs itemTappedEventArgs)
        {
            if (itemTappedEventArgs.ItemData is ContactTimesDay contactTimesDay && !IsBusy)
            {
                IsBusy = true; // Prevent multiple Popups from opening
                ContactTimesIntervalSelectionData contactTimesIntervalSelectionData = _contentService.GetContactTimesIntervalSelectionData(contactTimesDay);
                if (contactTimesIntervalSelectionData != null)
                {
                    _navigationController.DisplayPopupAsync<ContactTimesIntervalSelectionPageViewModel>(contactTimesIntervalSelectionData);
                }
                IsBusy = false;
            }
        }

        private void OnSwipeStarted(Syncfusion.ListView.XForms.SwipeStartedEventArgs e)
        {
            if (e.SwipeDirection == Syncfusion.ListView.XForms.SwipeDirection.Left && e.ItemData is ContactTimesDay contactTimesDay && 
                string.IsNullOrEmpty(contactTimesDay.MorningIntervalString) && string.IsNullOrEmpty(contactTimesDay.AfternoonIntervalString))
            {
                e.Cancel = true;
            }
        }

        private void OnSwipeEnded(Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if(e.SwipeDirection == Syncfusion.ListView.XForms.SwipeDirection.Left && e.ItemData is ContactTimesDay contactTimesDay)
            {
                DateTime defaultDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                contactTimesDay.UpdateDateTimes(defaultDateTime, defaultDateTime, defaultDateTime, defaultDateTime);
                _contentService.UpdateContactTimesDayData(contactTimesDay);
                UpdateContactTimesTypes(); // Get updated ContactTimesTypes
            }
        }

        private async Task OnSave()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                IsSaveButtonEnabled = false;

                foreach (ContactTimesType contactTimesType in ContactTimesTypes)
                {
                    foreach (ContactTimesDay contactTimesDay in contactTimesType.ContactTimesDays)
                    {
                        try
                        {
                            if (contactTimesDay.IsRecordNew())
                            {
                                await _contentService.Save(contactTimesDay, _cancellationTokenSource.Token);
                            }
                            else if (contactTimesDay.IsRecordDeleted())
                            {
                                await _contentService.Delete(contactTimesDay.RecordId, _cancellationTokenSource.Token);
                            }
                            else if (contactTimesDay.IsRecordModified())
                            {
                                await _contentService.Save(contactTimesDay, _cancellationTokenSource.Token);
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

                if (!IsErrorMessageVisible)
                {
                    await _navigationController.BackAsync(true);
                }
            }
        }

        private async Task OnCancel()
        {
            await _navigationController.BackAsync();
        }
    }
}
