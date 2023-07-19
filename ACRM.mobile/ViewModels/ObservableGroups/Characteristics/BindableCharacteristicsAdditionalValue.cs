using ACRM.mobile.Domain.Application.Characteristics;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.ViewModels.ObservableGroups.Characteristics
{
    public class BindableCharacteristicsAdditionalValue : ExtendedBindableObject
    {

        public int FieldId { get; }
        public string ValueDescription { get; }
        public string InitialContentValue { get; }

        private string _currentContentValue;
        public string CurrentContentValue 
        {
            get => _currentContentValue;
            set
            {
                _currentContentValue = value;
                RaisePropertyChanged(() => CurrentContentValue);
            }
        }

        public BindableCharacteristicsAdditionalValue(CharacteristicsAdditionalValue characteristicsAdditionalValue)
        {
            FieldId = characteristicsAdditionalValue.FieldId;
            ValueDescription = characteristicsAdditionalValue.ValueDescription;
            InitialContentValue = characteristicsAdditionalValue.InitialContentValue;
            CurrentContentValue = characteristicsAdditionalValue.InitialContentValue;
        }

        public bool IsContentModified()
        {
            return InitialContentValue != CurrentContentValue;
        }
    }
}
