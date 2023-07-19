using System;
namespace ACRM.mobile.Doman.ActionTemplates
{
    public enum SearchPageMode: int
    {
        IgnoreVirtual = 1,
        ShowColorOnDefault = 2,
        ShowColorOnVirtualInfoArea = 4,
        IgnoreColors = 8,
        ForceOptimizeForSpeed = 16
    }
}
