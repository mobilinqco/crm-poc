using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Characteristics
{
    public class CharacteristicGroup
    {

        public string DisplayValue { get; }
        public bool IsExpanded { get; }
        public List<CharacteristicItem> CharacteristicItems { get; } = new List<CharacteristicItem>();

        public CharacteristicGroup(int fieldId, string displayValue, bool isExpanded)
        {
            DisplayValue = displayValue;
            IsExpanded = isExpanded;
        }
    }
}
