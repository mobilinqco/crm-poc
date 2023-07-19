using System;
using ACRM.mobile.Domain.Configuration.UserInterface;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.FormatUtils
{
	public static class TimeZoneUtilExtensions
	{
        public static DateTime InClientTimeZone(this DateTime _baseDt, string ServerTimeZone)
        {
            if(string.IsNullOrEmpty(ServerTimeZone) || _baseDt == null)
            {
                return _baseDt;
            }

            var serverTZ = TimeZoneInfo.FindSystemTimeZoneById(ServerTimeZone);

            if (serverTZ == null || serverTZ == TimeZoneInfo.Local)
            {
                return _baseDt;
            }
            return TimeZoneInfo.ConvertTime(_baseDt, serverTZ, TimeZoneInfo.Local);
        }

        public static DateTime InServerTimeZone(this DateTime _baseDt, TimeZoneInfo serverTZ)
        {
            if (serverTZ == null || _baseDt == null)
            {
                return _baseDt;
            }


            if (serverTZ == TimeZoneInfo.Local)
            {
                return _baseDt;
            }
            return TimeZoneInfo.ConvertTime(_baseDt,TimeZoneInfo.Local,serverTZ);
        }
    }
}

