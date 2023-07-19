using System;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.Utils.Calendar
{
    public class BindableCrmRep: ExtendedBindableObject
    {
        public string Id { get; }
        public string Name { get; }

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

        public BindableCrmRep(string id, string name, bool isSelected)
        {
            Id = id;
            Name = name;
            IsSelected = isSelected;
        }
    }
}
