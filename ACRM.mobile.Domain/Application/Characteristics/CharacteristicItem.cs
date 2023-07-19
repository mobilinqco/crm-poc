using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Characteristics
{
    public class CharacteristicItem
    {

        public int GroupFieldId { get; }
        public string GroupRecordId { get; }
        public int ItemFieldId { get; }
        public string ItemRecordId { get; }
        public string RecordId { get; }
        public string DisplayValue { get; }
        public bool IsSingleSelection { get; }
        public bool ShowAdditionalFields { get; }
        public List<CharacteristicsAdditionalValue> AdditionalValues { get; } = new List<CharacteristicsAdditionalValue>();

        public CharacteristicItem(int groupFieldId, string groupRecordId, int itemFieldId, string itemRecordId,
            string recordId, string displayValue, bool isSingleSelection, bool showAdditionalFields)
        {
            GroupFieldId = groupFieldId;
            GroupRecordId = groupRecordId;
            ItemFieldId = itemFieldId;
            ItemRecordId = itemRecordId;
            RecordId = recordId;
            DisplayValue = displayValue;
            IsSingleSelection = isSingleSelection;
            ShowAdditionalFields = showAdditionalFields;
        }
    }
}
