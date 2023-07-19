using System;
namespace ACRM.mobile.Domain.Application.Calendar
{
    public class DeviceCalendar
    {
        public string Identifier { get; }
        public string Name { get; }
        public string Color { get; }
        public bool IsCRMCalendar { get; }

        public DeviceCalendar(string name, string identifier, string color, bool isCRMCalendar)
        {
            Name = name;
            Identifier = identifier;
            Color = color;
            IsCRMCalendar = isCRMCalendar;
        }
    }
}
