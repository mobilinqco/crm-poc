using ACRM.mobile.Domain.Configuration;
using Newtonsoft.Json;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACRM.mobile.Utils
{
    public class SyncConfigParser
    {
        public List<DateTime> FullSyncDateTimes(string configSyncJson, TimeZoneInfo serverTimezone)
        {
            List<DateTime> configSyncDateTimes = new List<DateTime>();
            ConfigSyncData configSyncDataRoot = JsonConvert.DeserializeObject<ConfigSyncData>(configSyncJson);
            configSyncDateTimes.AddRange(BuildDateTime(configSyncDataRoot, serverTimezone));
            foreach(ConfigSyncData configSyncData in configSyncDataRoot.Alternates)
            {
                configSyncDateTimes.AddRange(BuildDateTime(configSyncData, serverTimezone));
            }
            configSyncDateTimes.Sort((first, second) => DateTime.Compare(first, second));
            return configSyncDateTimes;
        }

        private List<DateTime> BuildDateTime(ConfigSyncData configSyncData, TimeZoneInfo serverTimezone)
        {
            List<DateTime> configSyncDateTimes = new List<DateTime>();

            List<float> hours = new List<float>();
            List<int> weekdays = new List<int>();

            if (configSyncData.Hours.Count > 0)
            {
                hours.AddRange(configSyncData.Hours);
            }
            else
            {
                hours.Add(configSyncData.Hour);
            }

            if(configSyncData.Weekdays.Count > 0)
            {
                weekdays.AddRange(configSyncData.Weekdays);
            }
            else
            {
                weekdays.Add(configSyncData.Weekday);
            }

            TimeZoneInfo timeZoneInfo;
            if (!string.IsNullOrEmpty(configSyncData.Timezone))
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(configSyncData.Timezone);
            }
            else
            {
                timeZoneInfo = serverTimezone;
            }

            foreach (int weekday in weekdays)
            {
                int convertedWeekDay = WeekdayConversion(weekday);
                foreach (float hour in hours)
                {
                    (int convertedHour, int convertedMinutes) = HourFloatConversion(hour);
                    DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, convertedWeekDay, convertedHour, convertedMinutes, 0);
                    if(dateTime >= DateTime.Now)
                    {
                        configSyncDateTimes.Add(TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo, TimeZoneInfo.Local));
                    }         
                }
            }

            return configSyncDateTimes;
        }

        private int WeekdayConversion(int weekday)
        {
            DateTime startOfWeek = DateTime.Today.AddDays(DayOfWeek.Sunday - DateTime.Today.DayOfWeek);
            return startOfWeek.Day + (weekday - 1);
        }

        private (int, int) HourFloatConversion(float decimalHour)
        {
            int hour = (int)Math.Truncate(decimalHour);
            int minutes = (int)Math.Floor((decimalHour - hour) / 100.0f * 60);
            return (hour, minutes);
        }
    }
}
