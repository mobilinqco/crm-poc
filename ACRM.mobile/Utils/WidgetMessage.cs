using System;
namespace ACRM.mobile.Utils
{
    internal class WidgetMessage
    {
        public WidgetEventType EventType { get; set; }
        public string ControlKey { get; set; }
        public object Data { get; set; }
        public object SubData { get; set; }
        public WidgetMessage()
        {
        }
    }
}
