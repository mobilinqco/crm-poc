using System;
using System.Collections.Generic;

namespace ACRM.mobile.Services.Contracts
{
    public interface ITokenProcessor
    {
        bool IsToken(string token);
        bool IsOnlyFirst(string token);
        bool ISParValueToken(string token);
        string TokenStringValue(string token, bool isForTemplateFilter = false);
        DateTime TokenDateValue(string token);
        string ProcessURL(string url, string curRecordId = "");
        void AddExtraParams(Dictionary<string, string> filterAdditionalParems);
    }
}
