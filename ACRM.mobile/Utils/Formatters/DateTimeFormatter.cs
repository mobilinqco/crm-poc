using System;
using System.Globalization;
using System.Threading;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Utils.Formatters
{
    public class DateTimeFormatter
    {
        public static DateTime DateTimeFromString(string value, string serverTimezone)
        {
            DateTime parsedDateTime;
            DateTime.TryParse(value, out parsedDateTime);
            return parsedDateTime.InClientTimeZone(serverTimezone);
        }

        private static DateTime? DateFromDbString(string value, PresentationFieldAttributes pa)
        {
            int year, month, day, hour, minute;

            if (value.Length == 8 || value.Length == 12)
            {
                try
                {
                    year = int.Parse(value.Substring(0, 4));
                    month = int.Parse(value.Substring(4, 2));
                    day = int.Parse(value.Substring(6, 2));

                    if (value.Length == 12)
                    {
                        hour = int.Parse(value.Substring(8, 2));
                        minute = int.Parse(value.Substring(10, 2));
                        return new DateTime(year, month, day, hour, minute, 0).InClientTimeZone(pa.ServerTimezone);
                    }
                    else
                    {
                        if(!string.IsNullOrWhiteSpace(pa.RelatedTimeValue) && pa.RelatedTimeValue.Length == 4)
                        {
                            hour = int.Parse(pa.RelatedTimeValue.Substring(0, 2));
                            minute = int.Parse(pa.RelatedTimeValue.Substring(2, 2));
                            DateTime convertedDate = new DateTime(year, month, day, hour, minute, 0).InClientTimeZone(pa.ServerTimezone);
                            year = convertedDate.Year;
                            month = convertedDate.Month;
                            day = convertedDate.Day;
                        }

                        return new DateTime(year, month, day);
                    }
                }
                catch
                {

                }
            }
            return null;
        }

        public static SeasonType CalcStyleForCurrentDate(DateTime selectedDate)
        {
            int month = selectedDate.Month;
            int day = selectedDate.Day;
            //int month = 12;
            //int day = 23;

            if (month == 1 || month == 2)
            {
                return SeasonType.UPDayPickerSeasonStyleWinter;
            }
            else if (month == 3)
            {
                if (day < 20)
                {
                    return SeasonType.UPDayPickerSeasonStyleWinter;
                }
                else
                {
                    return SeasonType.UPDayPickerSeasonStyleSpring;
                }
            }
            else if (month == 4 || month == 5)
            {
                return SeasonType.UPDayPickerSeasonStyleSpring;
            }
            else if (month == 6)
            {
                if (day < 21)
                {
                    return SeasonType.UPDayPickerSeasonStyleSpring;
                }
                else
                {
                    return SeasonType.UPDayPickerSeasonStyleSummer;
                }
            }
            else if (month == 7 || month == 8)
            {
                return SeasonType.UPDayPickerSeasonStyleSummer;
            }
            else if (month == 9)
            {
                if (day < 22)
                {
                    return SeasonType.UPDayPickerSeasonStyleSummer;
                }
                else
                {
                    return SeasonType.UPDayPickerSeasonStyleAutumn;
                }
            }
            else if (month == 10 || month == 11)
            {
                return SeasonType.UPDayPickerSeasonStyleAutumn;
            }
            else if (month == 12)
            {
                if (day < 22)
                {
                    return SeasonType.UPDayPickerSeasonStyleAutumn;
                }
                else
                {
                    return SeasonType.UPDayPickerSeasonStyleWinter;
                }
            }
            else
            {
                return SeasonType.UPDayPickerSeasonStyleAutumn;
            }
        }

        public static string FormatedEditDateFromDbString(string value, PresentationFieldAttributes pa)
        {
            if (!string.IsNullOrWhiteSpace(value) && pa != null)
            {
                switch (value.Length)
                {
                    case 4:
                        try
                        {
                            int hour = int.Parse(value.Substring(0, 2));
                            int minutes = int.Parse(value.Substring(2, 2));
                            var now = DateTime.Now;
                            DateTime time = new DateTime(now.Year, now.Month, now.Day, hour, minutes, 0).InClientTimeZone(pa.ServerTimezone);
                            return $"{time.Hour.ToString("D2")}:{time.Minute.ToString("D2")}";
                        }
                        catch { }
                        break;
                    case 8:
                        DateTime? dt = DateFromDbString(value, pa);
                        if (dt is DateTime dtValue)
                        {
                            return dtValue.ToString(CrmConstants.DateFormat);
                        }

                        break;

                    case 12:
                        DateTime? ldt = DateFromDbString(value, pa);
                        if (ldt is DateTime ldtValue)
                        {
                            return ldtValue.ToString(CrmConstants.DateTimeFormat);
                        }

                        break;
                    default:
                        return DateTimeFromString(value, pa.ServerTimezone).ToString("f", CultureInfo.CurrentUICulture);
                }
            }

            return string.Empty;
        }

        public static string FormatedDateFromDbString(string value, PresentationFieldAttributes pa)
        {
            if (!string.IsNullOrWhiteSpace(value) && pa != null)
            {
                switch (value.Length)
                {
                    case 4:
                        try
                        {
                            int hour = int.Parse(value.Substring(0, 2));
                            int minutes = int.Parse(value.Substring(2, 2));
                            var now = DateTime.Now;
                            DateTime time = new DateTime(now.Year, now.Month, now.Day, hour, minutes, 0).InClientTimeZone(pa.ServerTimezone);
                            return $"{time.Hour.ToString("D2")}:{time.Minute.ToString("D2")}";
                        }
                        catch { }
                        break;
                    case 8:
                        DateTime? dt = DateFromDbString(value, pa);
                        if (dt is DateTime dtValue)
                        {
                            return dtValue.ToString("d MMM yyyy");
                        }

                        break;

                    case 12:
                        DateTime? ldt = DateFromDbString(value, pa);
                        if (ldt is DateTime ldtValue)
                        {
                            return ldtValue.ToString("d MMM yyyy HH:mm");
                        }

                        break;
                    default:
                        return DateTimeFromString(value, pa.ServerTimezone).ToString("f", CultureInfo.CurrentUICulture);
                }
            }

            return string.Empty;
        }
    }
}
