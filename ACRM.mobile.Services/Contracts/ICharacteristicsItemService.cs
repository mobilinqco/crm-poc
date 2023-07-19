using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICharacteristicsItemService: IContentServiceBase
    {
        HashSet<string> GetVisibleCharacteristicsItemCodes();
        Dictionary<string, bool> GetCharacteristicsItemsShowAdditionalFieldsValues();
    }
}
