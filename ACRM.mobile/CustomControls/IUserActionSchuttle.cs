using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.CustomControls
{
    public interface IUserActionSchuttle
    {
        Task Carry(UserAction action, string recordId, CancellationToken cancellationToken);
    }
}
