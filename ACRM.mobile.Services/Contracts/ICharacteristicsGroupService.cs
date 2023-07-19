using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICharacteristicsGroupService: IContentServiceBase
    {
        HashSet<string> GetVisibleCharacteristicsGroupCodes();
        Dictionary<string, bool> GetCharacteristicsGroupsSingleSelectionValues();
        Dictionary<string, bool> GetCharacteristicsGroupsExpandedValues();
    }
}
