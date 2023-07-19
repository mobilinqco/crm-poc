using System;
using ACRM.mobile.Domain.Application.SerialEntry;

namespace ACRM.mobile.Services.Contracts
{
    public interface IListingService : IContentServiceBase
    {
        bool HasListingItems
        {
            get;
        }

        bool Match(bool listingOn, SerialEntryItem item);
    }
}
