using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services
{
    public class TabDataWithConfig
    {
        public InfoArea InfoArea;
        public ActionTemplateBase ActionTemplate;
        public SearchAndList SearchAndList;
        public FieldControl SearchControl;
        public FieldControl FieldControl;
        public TableInfo TableInfo;
        public List<Filter> EnabledDataFilters;
        public List<Filter> UserFilters;
        public List<Filter> EnabledUserFilters;
        public DataResponse RawData;
        public List<FieldControlField> ExternalFields;

        public List<Filter> GetAllEnabledFilters()
        {
            var filters = new List<Filter>();
            if (EnabledDataFilters?.Count > 0)
            {
                filters.AddRange(EnabledDataFilters);
            }

            if (EnabledUserFilters?.Count > 0)
            {
                filters.AddRange(EnabledUserFilters);
            }

            return filters;
        }
    }
}
