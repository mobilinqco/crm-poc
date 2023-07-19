using System;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IFavoriteService: IContentServiceBase
    {
        string AddToFavoritesImageName { get; }
        string RemoveFromFavoritesImageName { get;  }

        Task<bool> HandleFavoriteStatus(CancellationToken cancellationToken);

        bool IsBusy();
        bool IsRecordFavorite();
    }
}
