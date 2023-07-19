using System;
using System.Threading.Tasks;

namespace ACRM.mobile.Utils
{
    internal class WidgetEventSubscription
    {
        public WidgetEventType EventType { get; }
        public string ControlKey { get;}
        public Func<WidgetMessage, Task> MessageHandler { get;}
        public WidgetEventSubscription(WidgetEventType eventType, string controlKey, Func<WidgetMessage, Task> messageHandler)
        {
            EventType = eventType;
            ControlKey = controlKey?.ToLower();
            MessageHandler = messageHandler;
        }
    }
}
