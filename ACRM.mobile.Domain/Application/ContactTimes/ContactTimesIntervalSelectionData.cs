using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.ContactTimes
{
    public class ContactTimesIntervalSelectionData
    {
        public string TypeCode { get; }
        public string TypeName { get; }
        public List<ContactTimesDayAbbreviation> ContactTimesDayAbbreviations { get; }
        public DateTime MorningFromDateTime { get; set; }
        public DateTime MorningToDateTime { get; set; }
        public DateTime AfternoonFromDateTime { get; set; }
        public DateTime AfternoonToDateTime { get; set; }

        public ContactTimesIntervalSelectionData(string typeCode, string typeName, List<ContactTimesDayAbbreviation> contactTimesDayAbbreviations,
            DateTime morningFromDateTime, DateTime morningToDateTime, DateTime afternoonFromDateTime, DateTime afternoonToDateTime)
        {
            TypeCode = typeCode;
            TypeName = typeName;
            ContactTimesDayAbbreviations = contactTimesDayAbbreviations;
            MorningFromDateTime = morningFromDateTime;
            MorningToDateTime = morningToDateTime;
            AfternoonFromDateTime = afternoonFromDateTime;
            AfternoonToDateTime = afternoonToDateTime;
        }

    }
}
