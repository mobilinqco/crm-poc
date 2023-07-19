using ACRM.mobile.Domain.Application.Characteristics;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.ViewModels.ObservableGroups.Characteristics
{
    public class BindableCharacteristicItem : ExtendedBindableObject
    {

        public int GroupFieldId { get; }
        public string GroupRecordId { get; }
        public int ItemFieldId { get; }
        public string ItemRecordId { get; }
        public string RecordId { get; }
        public string DisplayValue { get; }
        public bool IsSingleSelection { get; }
        public bool ShowAdditionalFields { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if(IsSingleSelection && value)
                {
                    _bindableCharacteristicGroup.SetSingleSelectedItem(ItemRecordId);
                }
                _isSelected = value;
                AdditionalValuesVisible = _isSelected && ShowAdditionalFields;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        private bool _additionalValuesVisible;
        public bool AdditionalValuesVisible
        {
            get => _additionalValuesVisible;
            set
            {
                _additionalValuesVisible = value;
                RaisePropertyChanged(() => AdditionalValuesVisible);
            }
        }


        public List<BindableCharacteristicsAdditionalValue> BindableAdditionalValues { get; } = new List<BindableCharacteristicsAdditionalValue>();

        private BindableCharacteristicGroup _bindableCharacteristicGroup;

        public BindableCharacteristicItem(BindableCharacteristicGroup bindableCharacteristicGroup, CharacteristicItem characteristicItem)
        {
            _bindableCharacteristicGroup = bindableCharacteristicGroup;
            GroupFieldId = characteristicItem.GroupFieldId;
            GroupRecordId = characteristicItem.GroupRecordId;
            ItemFieldId = characteristicItem.ItemFieldId;
            ItemRecordId = characteristicItem.ItemRecordId;
            RecordId = characteristicItem.RecordId;
            DisplayValue = characteristicItem.DisplayValue;
            IsSingleSelection = characteristicItem.IsSingleSelection;
            ShowAdditionalFields = characteristicItem.ShowAdditionalFields;
            IsSelected = characteristicItem.RecordId != null;
            foreach (CharacteristicsAdditionalValue characteristicsAdditionalValue in characteristicItem.AdditionalValues)
            {
                BindableAdditionalValues.Add(new BindableCharacteristicsAdditionalValue(characteristicsAdditionalValue));
            }
        }

        public bool IsItemAdded()
        {
            return IsSelected == true && RecordId == null;
        }

        public bool IsItemRemoved()
        {
            return IsSelected == false && RecordId != null;
        }

        public Dictionary<int, string> GetCurrentFieldValues()
        {
            Dictionary<int, string> currentFieldValues = new Dictionary<int, string>
            {
                [GroupFieldId] = GroupRecordId,
                [ItemFieldId] = ItemRecordId
            };

            foreach (BindableCharacteristicsAdditionalValue bindableAdditionalValue in BindableAdditionalValues)
            {
                currentFieldValues[bindableAdditionalValue.FieldId] = bindableAdditionalValue.CurrentContentValue;
            }

            return currentFieldValues;
        }

        public Dictionary<int, string> GetOldFieldValues()
        {
            Dictionary<int, string> oldFieldValues = new Dictionary<int, string>
            {
                [GroupFieldId] = GroupRecordId,
                [ItemFieldId] = ItemRecordId
            };

            foreach (BindableCharacteristicsAdditionalValue bindableAdditionalValue in BindableAdditionalValues)
            {
                oldFieldValues[bindableAdditionalValue.FieldId] = bindableAdditionalValue.InitialContentValue;
            }

            return oldFieldValues;
        }
    }
}
