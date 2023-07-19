using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ACRM.mobile.Domain.Application.ContactTimes
{
    public class ContactTimesDataGridEntry
    {

        public string WeekDayName { get; }
        public int WeekDayId { get; }
        public List<string> OrderedContactTimesTypeNames { get; } // Used to keep type names in sorted order
        public Dictionary<string, string> TypeNameTimeIntervalsStringDict { get; }

        public ContactTimesDataGridEntry(string weekDayName, int weekDayId, List<string> orderedContactTimesTypeNames, Dictionary<string, string> typeNameTimeIntervalsStringDict)
        {
            WeekDayName = weekDayName;
            WeekDayId = weekDayId;
            OrderedContactTimesTypeNames = orderedContactTimesTypeNames;
            TypeNameTimeIntervalsStringDict = typeNameTimeIntervalsStringDict;
        }

        public bool IsToday()
        {
            return (WeekDayId + 1) % 7 == ((int)DateTime.Now.DayOfWeek);
        }
    }
}
