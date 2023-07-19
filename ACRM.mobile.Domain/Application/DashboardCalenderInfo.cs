using System;
namespace ACRM.mobile.Domain.Application
{
    public class DashboardCalenderInfo
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public string DayOfTheWeek { get; set; }
        public int Day { get; set; }
        public string MonthYear { get; set; }
        public SeasonType Season { get; set; }
    
    }
}
