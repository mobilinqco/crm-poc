using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
    public class QuickSearchInfoAreaData
    {

        public InfoArea InfoArea { get; set; }
        public ActionTemplateBase ActionTemplate { get; set; }
        public string InfoAreaID { get; set; }
        public FieldControl SearchControl { get; set; }
        public FieldControl FieldControl { get; set; }
        public TableInfo TableInfo { get; set; }
        public List<QuickSearchEntry> Entries { get; set; } = new List<QuickSearchEntry>();

    }
}
