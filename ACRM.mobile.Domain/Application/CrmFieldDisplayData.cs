using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application
{
    public class CrmFieldDisplayData
    {
        public string StringData;
        public List<ListDisplayField> ColspanData;
        public CrmFieldLinkData LinkedFieldData;

        public bool IsNullOrEmptyColspan()
        {
            if (ColspanData != null)
            {
                foreach (ListDisplayField val in ColspanData)
                {
                    if (!string.IsNullOrWhiteSpace(val.Data.StringData))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrWhiteSpace(StringData) && IsNullOrEmptyColspan();
        }
    }
}
