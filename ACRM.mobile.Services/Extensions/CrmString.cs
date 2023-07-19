using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services.Extensions
{
    public static class CrmString
    {

        public static DateTime CrmDate(this string DateTimeString)
        {
            DateTime selectedDateTime = DateTime.MinValue;
            if (string.IsNullOrEmpty(DateTimeString))
            {
                return DateTime.MinValue;
            }

            if (!string.IsNullOrWhiteSpace(DateTimeString))
            {
                if (DateTimeString.Length == 13)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DbFieldDateTimeFormat, null);
                }
                else if (DateTimeString.Length == 10)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateFormat, null);
                }
                else if (DateTimeString.Length == 8)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DbFieldDateFormat, null);
                }
                else if (DateTimeString.Length == 4)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DbFieldTimeFormat, null);
                }
            }

            return selectedDateTime;
        }

        public static bool CrmBool(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (str.ToLower().Equals("true") || str.Equals("1"))
            {
                return true;
            }

            return false;
        }

        public static string CrmDbBool(this string str)
        {
            return str.CrmBool() ? "1" : "0";
        }

        public static bool Like(this string toSearch, string toFind)
        {
            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\")
                .Replace(toFind, ch => @"\" + ch)
                .Replace('_', '.')
                .Replace("%", ".*")
                + @"\z", RegexOptions.Singleline).IsMatch(toSearch);
        }

        public static string CurratedCrmCatalog(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                return str.Replace("#", "");
            }

            return str;
        }

        public static bool IsJsExpression(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str) )
            {
                if ((str.Contains("(") && str.Contains(")")))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CrmFilterCompare(this string value, string pattern, string compareOperator, bool isNumericCompare)
        {
            if(string.IsNullOrEmpty(compareOperator))
            {
                return true;
            }

            if(isNumericCompare)
            {
                if(double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double doubleVal)
                    && double.TryParse(pattern, NumberStyles.Number, CultureInfo.InvariantCulture, out double doublePattern)) {

                    switch(compareOperator)
                    {
                        case "=":
                            return doubleVal == doublePattern;
                        case "<>":
                            return doubleVal != doublePattern;
                        case ">=":
                            return doubleVal >= doublePattern;
                        case "<=":
                            return doubleVal <= doublePattern;
                        case "<":
                            return doubleVal < doublePattern;
                        case ">":
                            return doubleVal > doublePattern;
                    }
                }
            }
            else
            {
                int compareResult = string.Compare(value, pattern, true);

                switch (compareOperator)
                {
                    case "=":
                        return compareResult == 0;
                    case "<>":
                        return compareResult != 0;
                    case ">=":
                        return compareResult >= 0;
                    case "<=":
                        return compareResult <= 0;
                    case "<":
                        return compareResult < 0;
                    case ">":
                        return compareResult > 0;
                }
            }

            return true;
        }
    }
}
