using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Utils
{
    public class RecordSelectorInput
    {
        public Dictionary<string, string> UIParams { get; set; }
        public UserAction UserAction { get; set; }
        public string OwnerKey { get; set; }

    }
}
