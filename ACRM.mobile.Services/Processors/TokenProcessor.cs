using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using Jint.Parser;

namespace ACRM.mobile.Services.Processors
{
    public class TokenProcessor: ITokenProcessor
    {
        private readonly ISessionContext _sessionContext;

        private Dictionary<string, string> _sessionTokens;

        private List<string> _unreplacebleTokens = new List<string> { "$cur("};

        public TokenProcessor(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
            InitSessionTokens();
        }

        public void InitSessionTokens()
        {
            _sessionTokens = new Dictionary<string, string>
            {
                { "$curRep", CrmRep.FormatToAureaRepId(_sessionContext.User.SessionInformation.RepIdStr()) },
                { "$curTenant", _sessionContext.User.SessionInformation.Attributes.TenantNo },
                { "$curDeputy", _sessionContext.User.SessionInformation.Attributes.RepDeputyId },
                { "$curSuperior", _sessionContext.User.SessionInformation.Attributes.RepSuperiorId },
                { "$curRepId", CrmRep.FormatToAureaRepId(_sessionContext.User.SessionInformation.RepIdStr()) },
                { "$curRepName", _sessionContext.User.SessionInformation.Attributes.RepName },
                { "$curTenantNo", _sessionContext.User.SessionInformation.Attributes.TenantNo },
                { "$curLoginName", _sessionContext.User.Username },
                { "$curLanguage", _sessionContext.LanguageCode },
                { "$curServerUrl", _sessionContext.CrmInstance.Url }
                //{ "curOrgGroup", _sessionContext.User.SessionInformation.Attributes.RepGroupId },
            };

            if(_sessionContext.ExtraParams != null)
            {
                foreach(var item in _sessionContext.ExtraParams)
                {
                    _sessionTokens.Add($"$cur{item.Key}", item.Value);
                    _sessionTokens.Add($"$par{item.Key}", item.Value);
                }
            }
        }

        public bool IsOnlyFirst(string token)
        {
            if (!string.IsNullOrEmpty(token) && token.StartsWith("$onlyFirst"))
            {
                return true;
            }

            return false;
        }

        public bool ISParValueToken(string token)
        {
            if (!string.IsNullOrEmpty(token) && token.StartsWith("$parValue"))
            {
                return true;
            }

            return false;
        }

        public bool IsToken(string token)
        {
            if (!string.IsNullOrEmpty(token)
                && (token.StartsWith("$cur") || token.StartsWith("$par") || token.StartsWith("$????") || token.StartsWith("$only") || token.StartsWith("$concat")))
            {
                return true;
            }

            return false;
        }

        public DateTime TokenDateValue(string token)
        {
            throw new NotImplementedException();
        }

        public string TokenStringValue(string token, bool isForTemplateFilter = false)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            if (!IsToken(token))
            {
                return token;
            }

            if (_sessionTokens != null)
            {
                string tok = token;
                if (token.Contains(":"))
                {
                    tok = token.Substring(0, token.IndexOf(":"));
                }

                if (_sessionTokens.ContainsKey(tok))
                {
                    string val = _sessionTokens[tok];
                    if (tok.Equals(token))
                    {
                        return val;
                    }
                    else
                    {
                        var prefixLength = token.IndexOf(":") + 1;
                        var varPart = token.Substring(prefixLength);
                        DateTime date = GetDateFromParam(val);
                        if (varPart.StartsWith("Hour") || varPart.StartsWith("Time"))
                        {
                            int partLength = 4;
                            int nextPosition = token.IndexOf(":") + partLength + 1;
                            date = PostfixExtractAndAddDateComponents(token, date, ref nextPosition, false);
                            return isForTemplateFilter ? date.ToDisplayTimeString() : date.ToCrmTimeString();
                        }
                        else if (IsDateTokenPart(varPart))
                        {
                            return ReplaceDatePostfixWithDateReplacePrefixLength(date, token, prefixLength, isForTemplateFilter);
                        }
                        else
                        {
                            return val;
                        }
                    }
                }
            }

            if(IsDateTimeToken(token))
            {
                return ReplaceDateVariables(token, isForTemplateFilter);
            }

            if (IsConcatToken(token))
            {
                return ResolveConcat(token, isForTemplateFilter);
            }

            foreach (var unreplaceble in _unreplacebleTokens)
            {
                if (token.StartsWith(unreplaceble))
                {
                    return token;
                }
            }

            return string.Empty;
        }

        private string ResolveConcat(string token, bool isForTemplateFilter)
        {
            var contect = token.Substring(8, token.Length - 9);
            if (!string.IsNullOrEmpty(contect))
            {
                var tokenParts = contect.Split(',');
                StringBuilder sb = new StringBuilder();
                foreach (var part in tokenParts)
                {
                    if (part.Equals("$SPACE"))
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(TokenStringValue(part, isForTemplateFilter));
                    }
                }
                return sb.ToString();
            }

            return contect;
        }

        private bool IsConcatToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            else
            {
                return token.StartsWith("$concat(") && token.EndsWith(")");
            }

        }

        // DateTime Token replacement methods
        private bool IsDateTimeToken(string token)
        {
            if(IsWildcardDate(token))
            {
                return true;
            }

            string[] dateTokens = { "$curTime", "$curTimeMin", "$curDate", "$curHour", "$curDay", "$curfdMonth", "$curfdQuarter", "$curfdWeek", "$curfdYear", "$curYYYY" };

            foreach(string dt in dateTokens)
            {
                if(token.StartsWith(dt))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDateTokenPart(string part)
        {

            string[] dateTokens = { "Date", "Day", "$fdMonth", "fdQuarter", "fdWeek", "fdYear", "YYYY" };
            foreach (string dt in dateTokens)
            {
                if (part.StartsWith(dt, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWildcardDate(string token)
        {
            if (token.Length < 9 || !token.StartsWith("$"))
            {
                return false;
            }

            for (var i = 1; i < 9; i++)
            {
                if ("?#0123456789".IndexOf(token.Substring(i, 1)) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private string ReplaceDateVariables(string token, bool isForTemplateFilter = false)
        {
            if (token.StartsWith("$cur"))
            {
                return token.StartsWith("$curTime")
                           ? ReplaceTimeVariables(token, isForTemplateFilter)
                           : ReplaceDatePostfixWithDateReplacePrefixLength(token, 4, isForTemplateFilter);
            }

            return IsWildcardDate(token)
                       ? ReplaceDatePostfixWithDateReplacePrefixLength(token, 0, isForTemplateFilter)
                       : token;
        }

        private string ReplaceDatePostfixWithDateReplacePrefixLength(string token, int prefixLength, bool isForTemplateFilter = false)
        {
            DateTime date = date = getDateTime(); 

            return ReplaceDatePostfixWithDateReplacePrefixLength(date, token, prefixLength, isForTemplateFilter);
        }

        private string ReplaceDatePostfixWithDateReplacePrefixLength(DateTime date,  string token, int prefixLength, bool isForTemplateFilter = false) { 

            string wildcardDate = null;
            var dateComponents = date;

            var resultLength = 0;
            var nextPosition = 0;
            var varPart = token.Substring(prefixLength);
            if (!ExtractDateComponents(token, prefixLength, ref wildcardDate, ref dateComponents, ref resultLength, ref nextPosition, varPart))
            {
                return token;
            }

            date = dateComponents;
            date = PostfixExtractAndAddDateComponents(token, date, ref nextPosition, true);

            var resultDateString = date.ToCrmDateString();
            if(isForTemplateFilter)
            {
                resultDateString = date.ToDisplayDateString();
            }

            if (string.IsNullOrWhiteSpace(wildcardDate))
            {
                return resultLength > 0 ? resultDateString.Substring(0, resultLength) : resultDateString;
            }

            var resultDateBuilder = new StringBuilder();
            var wildcardCopyRangeLength = 0;
            var resultCopyRangeLength = 0;
            var resultCopyRangeLocation = 0;
            var wildcardCopyRangeLocation = 0;
            ExtractDateWildCardParameters(wildcardDate, resultDateString, resultDateBuilder, ref wildcardCopyRangeLength, ref resultCopyRangeLength, ref resultCopyRangeLocation, ref wildcardCopyRangeLocation);

            if (resultCopyRangeLength > 0)
            {
                resultDateBuilder.Append(resultDateString.Substring(resultCopyRangeLocation, resultCopyRangeLength));
            }
            else if (wildcardCopyRangeLength > 0)
            {
                resultDateBuilder.Append(wildcardDate.Substring(wildcardCopyRangeLocation, wildcardCopyRangeLength));
            }

            return resultDateBuilder.ToString();
        }

        private string ReplaceTimeVariables(string token, bool isForTemplateFilter = false)
        {
            return token.StartsWith("$cur")
                       ? ReplaceTimePostfixWithDateReplacePrefixLength(token, 4, isForTemplateFilter)
                       : token;
        }

        private string ReplaceTimePostfixWithDateReplacePrefixLength(string token, int prefixLength, bool isForTemplateFilter = false)
        {
            DateTime date = DateTime.UtcNow;
            if(isForTemplateFilter)
            {
                date = getDateTime();
            }

            var seconds = false;
            var varPart = token.Substring(prefixLength);
            int nextPosition;
            if (varPart.StartsWith("Time"))
            {
                if (varPart.StartsWith("TimeSec"))
                {
                    seconds = true;
                    nextPosition = prefixLength + 7;
                }
                else if (varPart.StartsWith("TimeMin"))
                {
                    seconds = false;
                    nextPosition = prefixLength + 7;
                }
                else
                {
                    nextPosition = prefixLength + 4;
                }
            }
            else if (varPart.StartsWith("Hour"))
            {
                nextPosition = prefixLength + 4;
                date = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            }
            else
            {
                return token;
            }

            date = PostfixExtractAndAddDateComponents(token, date, ref nextPosition, false);

            if(isForTemplateFilter)
            {
                if (seconds)
                {
                    return date.ToDisplayTimeSecString();
                }
                return date.ToDisplayTimeString();
            }

            if (seconds)
            {
                return date.ToCrmTimeSecString();
            }

            return date.ToCrmTimeString();
        }

        private DateTime getDateTime()
        {
            return _sessionContext.ServerTimeZone == null ? DateTime.Now : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _sessionContext.ServerTimeZone);
        }

        private void ExtractDateWildCardParameters(string wildcardDate, string resultDateString, StringBuilder resultDateBuilder, ref int wildcardCopyRangeLength, ref int resultCopyRangeLength, ref int resultCopyRangeLocation, ref int wildcardCopyRangeLocation)
        {
            const int WildcardLength = 8;

            for (var wildcardIndex = 0; wildcardIndex < WildcardLength; wildcardIndex++)
            {
                if (wildcardDate[wildcardIndex] == '#')
                {
                    if (resultCopyRangeLength > 0)
                    {
                        resultCopyRangeLength++;
                    }
                    else
                    {
                        if (wildcardCopyRangeLength > 0)
                        {
                            resultDateBuilder.Append(wildcardDate.Substring(wildcardCopyRangeLocation, wildcardCopyRangeLength));
                            wildcardCopyRangeLength = 0;
                        }

                        resultCopyRangeLength = 1;
                        resultCopyRangeLocation = wildcardIndex;
                    }
                }
                else
                {
                    if (wildcardCopyRangeLength > 0)
                    {
                        wildcardCopyRangeLength++;
                    }
                    else
                    {
                        if (resultCopyRangeLength > 0)
                        {
                            resultDateBuilder.Append(resultDateString.Substring(resultCopyRangeLocation, resultCopyRangeLength));
                            resultCopyRangeLength = 0;
                        }

                        wildcardCopyRangeLength = 1;
                        wildcardCopyRangeLocation = wildcardIndex;
                    }
                }
            }
        }

        private bool ExtractDateComponents(string token, int prefixLength, ref string wildcardDate, ref DateTime dateComponents, ref int resultLength, ref int nextPosition, string varPart)
        {
            if (prefixLength == 0 && IsWildcardDate(token))
            {
                wildcardDate = token.Substring(1, 8);
                nextPosition = 9;
            }
            else if (varPart.StartsWith("Day"))
            {
                nextPosition = prefixLength + 3;
            }
            else if (varPart.StartsWith("Date"))
            {
                nextPosition = prefixLength + 4;
            }
            else if (varPart.StartsWith("fdMonth"))
            {
                dateComponents = new DateTime(dateComponents.Year, dateComponents.Month, 1);
                nextPosition = prefixLength + 7;
            }
            else if (varPart.StartsWith("fdQuarter"))
            {
                dateComponents = new DateTime(dateComponents.Year, dateComponents.Month, 1);
                dateComponents = SetQuarterDate(dateComponents);

                nextPosition = prefixLength + 9;
            }
            else if (varPart.StartsWith("fdWeek"))
            {
                var dayOfWeekIndex = (int)dateComponents.DayOfWeek;
                if (dayOfWeekIndex == 1)
                {
                    dayOfWeekIndex = 8;
                }

                dateComponents = new DateTime(dateComponents.Year, dateComponents.Month, dateComponents.Day);
                nextPosition = prefixLength + 6;
            }
            else if (varPart.StartsWith("fdYear"))
            {
                dateComponents = new DateTime(dateComponents.Year, 1, 1);
                nextPosition = prefixLength + 6;
            }
            else if (varPart.StartsWith("YYYY"))
            {
                dateComponents = new DateTime(dateComponents.Year, 1, 1);
                nextPosition = prefixLength + 4;
                resultLength = 4;
            }
            else
            {
                return false;
            }

            return true;
        }

        private static DateTime SetQuarterDate(DateTime dateComponents)
        {
            switch (dateComponents.Month)
            {
                case 2:
                case 3:
                    dateComponents = new DateTime(dateComponents.Year, 1, dateComponents.Day);
                    break;
                case 5:
                case 6:
                    dateComponents = new DateTime(dateComponents.Year, 4, dateComponents.Day);
                    break;
                case 8:
                case 9:
                    dateComponents = new DateTime(dateComponents.Year, 7, dateComponents.Day);
                    break;
                case 11:
                case 12:
                    dateComponents = new DateTime(dateComponents.Year, 10, dateComponents.Day);
                    break;
                default:
                    break;
            }

            return dateComponents;
        }

        private static DateTime PostfixExtractAndAddDateComponents(string source, DateTime date, ref int nextPosition, bool isDate)
        {
            var valueLength = source.Length;
            while (nextPosition < valueLength)
            {
                var addValue = false;
                int endPosition;
                int addVal;
                switch (source[nextPosition])
                {
                    case '+':
                        addValue = true;
                        ++nextPosition;
                        break;
                    case '-':
                        addValue = false;
                        ++nextPosition;
                        break;
                    case ' ':
                        ++nextPosition;
                        continue;
                    default:
                        nextPosition = valueLength;
                        break;
                }

                while (nextPosition < valueLength && source[nextPosition] == ' ')
                {
                    ++nextPosition;
                }

                endPosition = nextPosition;
                while (endPosition < valueLength && source[endPosition] >= '0' && source[endPosition] <= '9')
                {
                    ++endPosition;
                }

                addVal = int.Parse(source.Substring(nextPosition, endPosition - nextPosition));
                nextPosition = endPosition;
                while (nextPosition < valueLength && source[nextPosition] == ' ')
                {
                    ++nextPosition;
                }

                if (!addValue)
                {
                    addVal = -addVal;
                }

                if (nextPosition >= valueLength)
                {
                    break;
                }

                if (isDate)
                {
                    date = AddDateComponentsToDatePostfix(source, date, ref nextPosition, addVal);
                }
                else
                {
                    date = AddDateComponentsToTimePostfix(source, date, ref nextPosition, addVal);
                }
            }

            return date;
        }

        private static DateTime AddDateComponentsToDatePostfix(string source, DateTime date, ref int nextPosition, int addVal)
        {
            var diffDateComponents = TimeSpan.Zero;

            switch (source[nextPosition])
            {
                case 'd':
                case 'D':
                    diffDateComponents = TimeSpan.FromDays(addVal);
                    ++nextPosition;
                    break;
                case 'w':
                case 'W':
                    diffDateComponents = TimeSpan.FromDays(addVal * 7);
                    ++nextPosition;
                    break;
                case 'm':
                case 'M':
                    diffDateComponents = TimeSpan.FromDays(addVal * 30);
                    ++nextPosition;
                    break;
                case 'q':
                case 'Q':
                    diffDateComponents = TimeSpan.FromDays(addVal * 91);
                    ++nextPosition;
                    break;
                case 'y':
                case 'Y':
                    diffDateComponents = TimeSpan.FromDays(365);
                    ++nextPosition;
                    break;
                default:
                    diffDateComponents = TimeSpan.FromDays(addVal);
                    break;
            }

            date = date.Add(diffDateComponents);

            return date;
        }

        private static DateTime AddDateComponentsToTimePostfix(string source, DateTime date, ref int nextPosition, int addVal)
        {
            var diffDateComponents = TimeSpan.Zero;
            switch (source[nextPosition])
            {
                case 'h':
                case 'H':
                    diffDateComponents = TimeSpan.FromHours(addVal);
                    ++nextPosition;
                    break;
                case 'm':
                case 'M':
                    diffDateComponents = TimeSpan.FromMinutes(addVal);
                    ++nextPosition;
                    break;
                default:
                    diffDateComponents = TimeSpan.FromMinutes(addVal);
                    break;
            }

            date = date.Add(diffDateComponents);

            return date;
        }

        private DateTime GetDateFromParam(string param)
        {
            try
            {
                string paramVal = param.Replace(":", "").Replace("-", "");

                DateTime dateTime = getDateTime();

                int year = dateTime.Year;
                int month = dateTime.Month;
                int day = dateTime.Day;
                int hour = dateTime.Hour;
                int minute = dateTime.Minute;

                if (paramVal.Length == 4)
                {
                    hour = int.Parse(paramVal.Substring(0, 2));
                    minute = int.Parse(paramVal.Substring(2, 2));
                }
                else if (paramVal.Length >= 8)
                {
                    year = int.Parse(paramVal.Substring(0, 4));
                    month = int.Parse(paramVal.Substring(4, 2));
                    day = int.Parse(paramVal.Substring(6, 2));

                    if (paramVal.Length == 12)
                    {
                        hour = int.Parse(paramVal.Substring(8, 2));
                        minute = int.Parse(paramVal.Substring(10, 2));
                    }
                }

                return new DateTime(year, month, day, hour, minute, 0);
            }
            catch
            {
                return getDateTime();
            }
        }

        public string ProcessURL(string strUrl, string curRecordId = "")
        {
            string processedURl = strUrl;
            if (!string.IsNullOrEmpty(strUrl) && strUrl.Contains("$cur"))
            {
                processedURl = processedURl.Replace("$curRepId;", TokenStringValue("$curRepId"));
                processedURl = processedURl.Replace("$curRep;", TokenStringValue("$curRepName"));
                processedURl = processedURl.Replace("$curTenantNo;", TokenStringValue("$curTenantNo"));

                processedURl = processedURl.Replace("$curLanguage;", TokenStringValue("$curLanguage"));
                processedURl = processedURl.Replace("$curLoginName;", TokenStringValue("$curLoginName"));
                processedURl = processedURl.Replace("$curServerUrl;", TokenStringValue("$curServerUrl"));


                processedURl = processedURl.Replace("$curRecordId;", curRecordId);
            }

            return processedURl;
        }

        public void AddExtraParams(Dictionary<string, string> filterAdditionalParems)
        {
            if (filterAdditionalParems != null)
            {
                foreach (var item in filterAdditionalParems)
                {
                    string key = $"$par{item.Key}";

                    if (_sessionTokens.ContainsKey(key))
                    {
                        _sessionTokens[key] = item.Value;
                    }
                    else
                    {
                        _sessionTokens.Add(key, item.Value);
                    }
                }
            }
        }
    }
}
