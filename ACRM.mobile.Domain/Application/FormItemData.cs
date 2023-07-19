using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
    public class FormItemData
    {
        public UserAction Action { get; set; }
        public FormItem FormItem { get; set; }
        public Dictionary<string, Dictionary<string, string>> FormParams { get; set; }
        public FormItemData()
        {
        }
    }
}
