using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.ViewModels;

namespace ACRM.mobile.CustomControls
{
    public interface IImageSelectorInterface
    {
        void FileSelected(DocumentUploadPageViewModel documentUploadPageViewModel);
    }
}
