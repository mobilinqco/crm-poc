using System;
namespace ACRM.mobile.Domain.Application
{
    public class RefreshEvent
    {
        public bool IsRefreshNeeded { get; set; }
        public string Reason { get; set; }

        public RefreshEvent()
        {
        }
    }
}
