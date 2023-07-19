using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Characteristics
{
    public class CharacteristicsAdditionalValue
    {

        public int FieldId { get; }
        public string ValueDescription { get; }
        public string InitialContentValue { get; }

        public CharacteristicsAdditionalValue(int fieldId, string valueDescription, string initialContentValue)
        {
            FieldId = fieldId;
            ValueDescription = valueDescription;
            InitialContentValue = initialContentValue;
        }

    }
}
