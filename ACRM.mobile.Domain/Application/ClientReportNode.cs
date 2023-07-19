using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application
{
    public class ClientReportNode
    {
        public string RootName { get; set; }
        public string ConfigName { get; set; }
        public string ParentLink { get; set; }
        public List<ListDisplayRow> ItemRow { get; set; }
        public ClientReportNode()
        {
        }
    }
}
