using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using AsyncAwaitBestPractices;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class ImageViewPageViewModel: NavigationBarBaseViewModel
    {
        private readonly IImageViewContentService _contentService;
        
        private ImageSource _imageSourcePath = ImageSource.FromFile("aurea.png");
        public ImageSource ImageSourcePath
        {
            get => _imageSourcePath;
            set
            {
                _imageSourcePath = value;
                RaisePropertyChanged(() => ImageSourcePath);
            }
        }

        public ImageViewPageViewModel(IImageViewContentService contentService)
        {
            IsLoading = true;
            _contentService = contentService;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            _logService.LogDebug("Start  InitializeAsync");

            IsBackButtonVisible = true;
            IsLoading = true;

            if (navigationData is UserAction userAction)
            {
                PageTitle = userAction.ActionDisplayName;

                _contentService.SetSourceAction(userAction);
                _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to prepare content {ex.Message}");
                });

                var resourcePath = await _contentService.GetImageViewResourcePath(_cancellationTokenSource.Token);

                if(!string.IsNullOrEmpty(resourcePath) && File.Exists(resourcePath))
                {
                    ImageSourcePath = ImageSource.FromFile(resourcePath);
                }
                else
                {
                    ResourceDictionary StaticResources = Application.Current.Resources;
                    ImageSourcePath = new FontImageSource
                    {
                        FontFamily = (OnPlatform<string>)StaticResources["HeaderButtonImagesFont"],
                        Glyph = "\uE060",
                        Color = Color.LightSkyBlue
                    };
                }
            }

            await base.InitializeAsync(navigationData);

            IsLoading = false;

            _logService.LogDebug("End  InitializeAsync");
        }

    }
}
