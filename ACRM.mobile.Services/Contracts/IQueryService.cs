using System;
using System.Data;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQueryService : IContentServiceBase
    {
        DataTable GetData();
    }
}

