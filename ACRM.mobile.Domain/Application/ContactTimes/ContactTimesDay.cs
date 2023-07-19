using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.ContactTimes
{
    public class ContactTimesDay
    {

        public string RecordId { get; }
        public string TypeCode { get; }
        public int WeekDayId { get; }
        public string WeekDayName { get; }
        public DateTime InitialMorningFromDateTime { get; }
        public DateTime MorningFromDateTime { get; private set; }
        public DateTime InitialMorningToDateTime { get; }
        public DateTime MorningToDateTime { get; private set; }
        public DateTime?InitialAfternoonFromDateTime { get; }
        public DateTime AfternoonFromDateTime { get; private set; }
        public DateTime InitialAfternoonToDateTime { get; }
        public DateTime AfternoonToDateTime { get; private set; }
        private string InitialMorningIntervalString;
        private string InitialAfternoonIntervalString;
        public string MorningIntervalString { get; private set; }
        public string AfternoonIntervalString { get; private set; }

        public ContactTimesDay(string recordId, string typeCode, int weekDayId, string weekDayName, DateTime morningFromDateTime, DateTime morningToDateTime,
            DateTime afternoonFromDateTime, DateTime afternoonToDateTime)
        {
            RecordId = recordId;
            TypeCode = typeCode;
            WeekDayId = weekDayId;
            WeekDayName = weekDayName;
            InitialMorningFromDateTime = morningFromDateTime;
            MorningFromDateTime = morningFromDateTime;
            InitialMorningToDateTime = morningToDateTime;
            MorningToDateTime = morningToDateTime;
            InitialAfternoonFromDateTime = afternoonFromDateTime;
            AfternoonFromDateTime = afternoonFromDateTime;
            InitialAfternoonToDateTime = afternoonToDateTime;
            AfternoonToDateTime = afternoonToDateTime;
            InitialMorningIntervalString = BuildMorningIntervalString(morningFromDateTime, morningToDateTime);
            InitialAfternoonIntervalString = BuildAfternoonIntervalString(afternoonFromDateTime, afternoonToDateTime);
            MorningIntervalString = InitialMorningIntervalString;
            AfternoonIntervalString = InitialAfternoonIntervalString;
        }

        private string BuildMorningIntervalString(DateTime morningFromDateTime, DateTime morningToDateTime)
        {
            if(morningFromDateTime.Hour != 0 || morningFromDateTime.Minute != 0
                || morningToDateTime.Hour != 0 || morningToDateTime.Minute != 0)
            {
                return string.Format("{0}-{1}", morningFromDateTime.ToString("HH:mm"), morningToDateTime.ToString("HH:mm"));
            } 
            return "";
        }

        private string BuildAfternoonIntervalString(DateTime afternoonFromDateTime, DateTime afternoonToDateTime)
        {
            if (afternoonFromDateTime.Hour != 0 || afternoonFromDateTime.Minute != 0
                || afternoonToDateTime.Hour != 0 || afternoonToDateTime.Minute != 0)
            {
                return string.Format("{0}-{1}", afternoonFromDateTime.ToString("HH:mm"), afternoonToDateTime.ToString("HH:mm"));
            }
            return "";
        }

        public string GetTimeIntervalsString()
        {
            return string.Format("{0}\t{1}", MorningIntervalString, AfternoonIntervalString);
        }

        public void UpdateDateTimes(DateTime morningFromDateTime, DateTime morningToDateTime, DateTime afternoonFromDateTime, DateTime afternoonToDateTime)
        {
            MorningFromDateTime = morningFromDateTime;
            MorningToDateTime = morningToDateTime;
            MorningIntervalString = BuildMorningIntervalString(morningFromDateTime, morningToDateTime);

            AfternoonFromDateTime = afternoonFromDateTime;
            AfternoonToDateTime = afternoonToDateTime;
            AfternoonIntervalString = BuildAfternoonIntervalString(afternoonFromDateTime, afternoonToDateTime);
        }

        public bool IsEqual(ContactTimesDay contactTimesDay)
        {
            return MorningIntervalString.Equals(contactTimesDay.MorningIntervalString) && AfternoonIntervalString.Equals(contactTimesDay.AfternoonIntervalString);
        }

        public bool IsRecordNew()
        {
            return RecordId == null && (!string.IsNullOrEmpty(MorningIntervalString) || !string.IsNullOrEmpty(AfternoonIntervalString));
        }

        public bool IsRecordModified()
        {
            return RecordId != null && (!InitialMorningIntervalString.Equals(MorningIntervalString) || !InitialAfternoonIntervalString.Equals(AfternoonIntervalString)) &&
                (!string.IsNullOrEmpty(MorningIntervalString) || !string.IsNullOrEmpty(AfternoonIntervalString));
        }

        public bool IsRecordDeleted()
        {
            return RecordId != null && string.IsNullOrEmpty(MorningIntervalString) && string.IsNullOrEmpty(AfternoonIntervalString);
        }
    }
}
