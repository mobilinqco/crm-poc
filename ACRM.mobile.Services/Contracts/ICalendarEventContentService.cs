using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICalendarEventDetailsContentService: IContentServiceBase
    {
        void SetCalendarViewTemplate(CalendarViewTemplate calendarViewTemplate);
        List<PanelData> Panels();
    }
}
