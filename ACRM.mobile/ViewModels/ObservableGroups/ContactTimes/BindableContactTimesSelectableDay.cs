using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.ViewModels.ObservableGroups.ContactTimes
{
    public class BindableContactTimesSelectableDay : ExtendedBindableObject
    {
        public int WeekDayId { get; }

        private string _weekDayAbbreviation;
        public string WeekDayAbbreviation
        {
            get => _weekDayAbbreviation;
            set
            {
                _weekDayAbbreviation = value;
                RaisePropertyChanged(() => WeekDayAbbreviation);
            }
        }
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        public BindableContactTimesSelectableDay(int weekDayId, string weekDayAbbreviation, bool isSelected)
        {
            WeekDayId = weekDayId;
            WeekDayAbbreviation = weekDayAbbreviation;
            IsSelected = isSelected;
        }
    }
}
