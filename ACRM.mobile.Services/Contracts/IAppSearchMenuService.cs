using System.Collections.Generic;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface IAppSearchMenuService: IContentServiceBase
    {
        List<UserAction> InfoAreaRelatedActions();
        UserAction GlobalSearch();
        UserAction HistorySearch();
        UserAction FavoriteSearch();
    }
}
