using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IImageViewContentService: IContentServiceBase
    {
        Task<string> GetImageViewResourcePath(CancellationToken cancellationToken);
    }
}
