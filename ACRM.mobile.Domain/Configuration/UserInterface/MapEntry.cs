using System;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public class MapEntry
    {
        public string RecordId { get; set; }
        public string InfoAreaID { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string Address
        {
            get
            {
                return $"{Street}, {City}, {Country}";
            }
        }
        public string Label { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ListDisplayRow DisplayRow { get; set; }
        
    }
}
