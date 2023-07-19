using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.Messages
{
    public class SelectedCalendarsMessage
    {
        public HashSet<string> SelectedCrmCalendarIds { get; }
        public HashSet<string> SelectedDeviceCalendarIds { get; }

        public SelectedCalendarsMessage(HashSet<string> selectedCrmCalendarIds, HashSet<string> selectedDeviceCalendarIds)
        {
            SelectedCrmCalendarIds = selectedCrmCalendarIds;
            SelectedDeviceCalendarIds = selectedDeviceCalendarIds;
        }
    }
}
