using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.ActionTemplates
{
    public class CalendarViewTemplate: ActionTemplateBase
    {
        public CalendarViewTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public List<string> AdditionalCalendarConfigs()
        {
            List<string> additionalCalendarConfigs = new List<string>();

            string additionalConfigsString = GetValue("AdditionalCalendarConfigs");

            if(!string.IsNullOrEmpty(additionalConfigsString))
            {
                additionalCalendarConfigs.AddRange(additionalConfigsString.Split(','));
                return additionalCalendarConfigs;
            }

            return additionalCalendarConfigs;
        }

        public string NewAppointmentAction()
        {
            return GetValue("NewAppointmentAction");
        }

        public string CalendarPopOverConfig()
        {
            return GetValue("CalendarPopOverConfig");
        }

        public string DefaultViewType()
        {
            return GetValue("DefaultViewType");
        }

        public bool IncludeSystemCalendar()
        {
            bool.TryParse(GetValue("IncludeSystemCalendar"), out bool includeSystemCalendar);
            return includeSystemCalendar;
        }

        public bool ShowSystemCalendarFilter()
        {
            bool.TryParse(GetValue("ShowSystemCalendarFilter"), out bool showSystemCalendarFilter);
            return showSystemCalendarFilter;
        }

        public bool RepFilterCurrentRepActive()
        {
            bool.TryParse(GetValue("RepFilterCurrentRepActive"), out bool repFilterCurrentRepActive);
            return repFilterCurrentRepActive;
        }

        public string RepFilter()
        {
            return GetValue("RepFilter");
        }
    }
}
