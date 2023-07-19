using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACRM.mobile.Services.Contracts;
using Xamarin.Forms;

[assembly: Dependency(typeof(CRM.Client.UWP.Services.DocHelperService))]
namespace CRM.Client.UWP.Services
{
    public class DocHelperService : IDocHelperService
    {
        public void ConvertToPDF(string htmlContent, string docfilePath, string baseUrl, Func<string, Task> callbackFun)
        {
        }

    
    }
 }
