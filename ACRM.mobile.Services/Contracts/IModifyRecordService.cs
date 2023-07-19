using ACRM.mobile.Domain.Application;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IModifyRecordService: IContentServiceBase
    {
        Task ModifyRecord(UserAction userAction, CancellationToken cancellationToken);

        bool IsBusy();

        Task<UserAction> SavedAction(CancellationToken cancellationToken);
    }
}
