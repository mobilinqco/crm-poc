using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public class EditTriggerItem
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public List<EditTriggerUnit> TriggerUnits { get; set; }
        public EditTriggerItem()
        {
            TriggerUnits = new List<EditTriggerUnit>();
        }
    }
}
