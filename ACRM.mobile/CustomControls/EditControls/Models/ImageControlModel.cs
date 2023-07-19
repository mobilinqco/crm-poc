using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class ImageControlModel : BaseEditControlModel, IImageSelectorInterface
    {
        private readonly IDocumentService _documentService;
        private DocumentUploadPageViewModel _documentUploadPageViewModel;
        public ImageSource _fileImageSource;
        public ImageSource FileImageSource
        {
            get
            {
                if (_fileImageSource!=null)
                {
                    return _fileImageSource;
                }
                else
                {
                    ResourceDictionary StaticResources = Application.Current.Resources;
                    return new FontImageSource
                    {
                        FontFamily = (OnPlatform<string>)StaticResources["HeaderButtonImagesFont"],
                        Glyph = "\uE060",
                        Color = Color.LightSkyBlue
                    };
                }


            }
            set
            {
                _fileImageSource = value;
                RaisePropertyChanged(() => FileImageSource);
            }

        }
        public ICommand OnImageUploadButtonTapped => new Command(async () =>
        {
            NotifyDirtyState();
            await _navigationController.NavigateToAsync<DocumentUploadPageViewModel>(parameter:this);
        });

        public ImageControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
            _documentService = AppContainer.Resolve<IDocumentService>();
           
        }

        public override async ValueTask<bool> InitializeControl()
        {
            await base.InitializeControl();
            var headerImage = Field.Data.StringData;
            if (!string.IsNullOrWhiteSpace(headerImage))
            {
                var fileImagePath = await _documentService.GetDocumentPath(headerImage, _cancellationTokenSource.Token);
                if (!string.IsNullOrEmpty(fileImagePath))
                {
                    FileImageSource = ImageSource.FromFile(fileImagePath);
                }

            }
          
            return true;
        }

        public void FileSelected(DocumentUploadPageViewModel documentUploadPageViewModel)
        {
            NotifyDirtyState();
            _documentUploadPageViewModel = documentUploadPageViewModel;
            _documentUploadPageViewModel.DocField = Field;
            Field.EditData.StringValue = "ImageSelected";
            if (_documentUploadPageViewModel.documentObject.IsImage())
            {
                var stream = _documentUploadPageViewModel.FileResult.OpenReadAsync().Result;
                FileImageSource = ImageSource.FromStream(() => stream);
            }
        }

        public override object ChangeOfflineRequest
        {
            get
            {
                if (Field.EditData.HasStringChanged)
                {
                    return new OfflineRecordField()
                    {
                        FieldId = Field.Config.FieldConfig.FieldId,
                        NewValue = Field.EditData.DefaultStringValue,
                        Offline = 0
                    };
                }

                return null;
            }
        }

        public object DocuemntModel
        {
            get
            {
                return _documentUploadPageViewModel;
            }
        }
    }
}
