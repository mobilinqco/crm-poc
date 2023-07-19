using System;
using System.Data;

namespace ACRM.mobile.Domain.Application.Messages
{
    public class RecordSelectedMessage
    {
        public UserAction RecordSelectorAction { get; set; }
        public ListDisplayRow SelectedRow { get; set; }
        public ParentLink LinkDetails { get; set; }
        public string OwnerKey { get; set; }
    }
}
