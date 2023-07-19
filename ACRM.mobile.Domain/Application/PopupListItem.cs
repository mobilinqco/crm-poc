using System;
namespace ACRM.mobile.Domain.Application
{
    public class PopupListItem
    {
        public (string image, string glyph) ImageSource
        {
            get; set;
        }
        public string RecordId { get; set; }
        public string DisplayText { get; set; }
        public object OrginalObject { get; set; }
        public bool Selected { get; set; }
        public PopupListItem()
        {
        }
    }
}
