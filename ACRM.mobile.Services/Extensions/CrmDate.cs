using System;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services.Extensions
{
    public static class CrmDate
    {
        public static string ToCrmDateString(this DateTime value)
        {
            return value.ToString(CrmConstants.DbFieldDateFormat);
        }

        public static string ToCrmTimeString(this DateTime value)
        {
            return value.ToString(CrmConstants.DbFieldTimeFormat);
        }

        public static string ToCrmDateTimeString(this DateTime value)
        {
            return value.ToString(CrmConstants.DbFieldDateTimeFormat);
        }

        public static string ToCrmTimeSecString(this DateTime value)
        {
            return value.ToString(CrmConstants.DbFieldTimeSecFormat);
        }

        public static string ToDisplayDateString(this DateTime value)
        {
            return value.ToString(CrmConstants.DateFormat);
        }

        public static string ToDisplayTimeString(this DateTime value)
        {
            return value.ToString(CrmConstants.TimeFormat);
        }

        public static string ToDisplayDateTimeString(this DateTime value)
        {
            return value.ToString(CrmConstants.DateTimeFormat);
        }

        public static string ToDisplayTimeSecString(this DateTime value)
        {
            return value.ToString(CrmConstants.TimeSecFormat);
        }

        public static string ToControlFieldFormatString(this DateTime value)
        {
            return value.ToString(CrmConstants.DbControlFieldDateFormat);
        }

        public static string ToDbSyncTimestampFieldDateFormat(this DateTime value)
        {
            return value.ToString(CrmConstants.DbSyncTimestampFieldDateFormat);
        }
    }
}
