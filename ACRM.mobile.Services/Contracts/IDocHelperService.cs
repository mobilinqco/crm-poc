using System;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IDocHelperService
    {
        void ConvertToPDF(string htmlContent, string docfilePath, string baseUrl, Func<string, Task> callbackFun);
    }
}
