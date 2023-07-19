using System;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public enum AnalysisFieldFlag
    {
        Category = 1,
        ResultColumn = 2,
        DefaultCategory = 4,
        DateFilter = 16,
        Currency = 32,
        DependentOnCurrency = 64,
        Filter = 128,
        DependentOnWeight = 256,
        Weight = 512,
        DoNotSort = 1024,
        ShowAll = 2048,
        MustSelect = 4096,
        ReadAll = 8192,
        AlternateCurrency = 16384,
        NoOther = 32768,
        CatNotMandatory = 65536,
        XCategory = 131072
    }
}
