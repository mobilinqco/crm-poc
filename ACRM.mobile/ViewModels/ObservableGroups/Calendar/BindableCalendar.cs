using System;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.Utils.Calendar
{
    public class BindableCalendar: ExtendedBindableObject
    {

        public string Identifier { get; }
        public string Name { get; }
        public bool IsCRMCalendar { get;  }

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

        public BindableCalendar(string identifier, string name, bool isCRMCalendar, bool isSelected)
        {
            Identifier = identifier;
            Name = name;
            IsCRMCalendar = isCRMCalendar;
            IsSelected = isSelected;
        }
    }
}
