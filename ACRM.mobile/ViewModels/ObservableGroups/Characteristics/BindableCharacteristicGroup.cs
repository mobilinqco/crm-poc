using ACRM.mobile.Domain.Application.Characteristics;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.ViewModels.ObservableGroups.Characteristics
{
    public class BindableCharacteristicGroup : ExtendedBindableObject
    {

        public string DisplayValue { get; }
        public bool IsExpanded { get; }
        public List<BindableCharacteristicItem> BindableCharacteristicItems { get; } = new List<BindableCharacteristicItem>();

        public BindableCharacteristicGroup(CharacteristicGroup characteristicGroup)
        {
            DisplayValue = characteristicGroup.DisplayValue;
            IsExpanded = characteristicGroup.IsExpanded;
            foreach (CharacteristicItem characteristicItem in characteristicGroup.CharacteristicItems)
            {
                BindableCharacteristicItems.Add(new BindableCharacteristicItem(this, characteristicItem));
            }
        }

        public void SetSingleSelectedItem(string selectedItemRecordId)
        {
            foreach(BindableCharacteristicItem bindableCharacteristicItem in BindableCharacteristicItems)
            {
                if(bindableCharacteristicItem.ItemRecordId != selectedItemRecordId && bindableCharacteristicItem.IsSelected)
                {
                    bindableCharacteristicItem.IsSelected = false;
                }
            }
        }
    }
}
