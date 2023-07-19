using System;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class DateTimeSelectorPageViewModel : BaseViewModel
    {
        private int _pageWidthRequest = 350;
        public int PageWidthRequest
        {
            get => _pageWidthRequest;
            set
            {
                _pageWidthRequest = value;
                RaisePropertyChanged(() => PageWidthRequest);
            }
        }

        private int _pageHeightRequest = 350;
        public int PageHeightRequest
        {
            get => _pageHeightRequest;
            set
            {
                _pageHeightRequest = value;
                RaisePropertyChanged(() => PageHeightRequest);
            }
        }

        private bool _hasDate = true;
        public bool HasDate
        {
            get => _hasDate;
            set
            {
                _hasDate = value;
                RaisePropertyChanged(() => HasDate);
            }
        }

        private bool _hasTime = false;
        public bool HasTime
        {
            get => _hasTime;
            set
            {
                _hasTime = value;
                RaisePropertyChanged(() => HasTime);
            }
        }

        private DateTime _selectedDataTime = DateTime.Now;
        public DateTime SelectedDataTime
        {
            get => _selectedDataTime;
            set
            {
                _selectedDataTime = value;
                RaisePropertyChanged(() => SelectedDataTime);
            }
        }

        private string _dateTimeString;
        public string DateTimeString
        {
            get => _dateTimeString;
            set
            {
                if (_dateTimeString != value)
                {
                    _dateTimeString = value;
                    RaisePropertyChanged(() => DateTimeString);
                }
            }
        }

        private PopupStatusAction _popupAction = PopupStatusAction.Open;
        public PopupStatusAction PopupAction
        {
            get => _popupAction;
            set
            {
                _popupAction = value;
                if (_popupAction == PopupStatusAction.Close || _popupAction == PopupStatusAction.UpdateAndClose)
                {
                    if(_popupAction == PopupStatusAction.UpdateAndClose)
                    {
                        DateTimePopupData message = new DateTimePopupData
                        {
                            DateTimeString = DateTimeString,
                            SelectedDataTime = SelectedDataTime,
                            HasDate = HasDate,
                            HasTime = HasTime,
                            Title = Title,
                            FieldIdentification = _fieldIdentification
                        };

                        MessagingCenter.Send<BaseViewModel, DateTimePopupData>(this, InAppMessages.DateTimeSelected, message);
                    }

                    _navigationController.PopPopupAsync();
                }
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private string _fieldIdentification = string.Empty;

        public DateTimeSelectorPageViewModel()
        {
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            _logService.LogDebug("Start DateTimeSelectorPageViewModel InitializeAsync");
            if (navigationData is DateTimePopupData fieldData)
            {
                HasTime = fieldData.HasTime;
                HasDate = fieldData.HasDate;
                DateTimeString = fieldData.DateTimeString;
                SelectedDataTime = fieldData.SelectedDataTime;
                Title = fieldData.Title;
                _fieldIdentification = fieldData.FieldIdentification;

                if (HasTime && HasDate)
                {
                    PageWidthRequest = 450;
                }
                else if(!HasDate)
                {
                    PageWidthRequest = 250;
                }
            }
            await base.InitializeAsync(navigationData);
            _logService.LogDebug("End DateTimeSelectorPageViewModel InitializeAsync");
        }
    }
}

