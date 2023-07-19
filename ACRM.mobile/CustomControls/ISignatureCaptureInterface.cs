using System;
using System.Threading.Tasks;

namespace ACRM.mobile.CustomControls
{
    public interface ISignatureCaptureInterface
    {
        Task GenerateReport(string signaturePath);
    }
}
