using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ContactTimes
{
    public class ContactTimesDayAbbreviation
    {
        public int WeekDayId { get; }
        public string WeekDayNameAbbreviation { get; }
        public bool IsSelected { get; }
        public ContactTimesDayAbbreviation(int weekDayId, string weekDayNameAbbreviation, bool isSelected)
        {
            WeekDayId = weekDayId;
            WeekDayNameAbbreviation = weekDayNameAbbreviation;
            IsSelected = isSelected;
        }

    }
}
