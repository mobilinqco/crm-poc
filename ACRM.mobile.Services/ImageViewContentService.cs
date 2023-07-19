using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class ImageViewContentService : ContentServiceBase, IImageViewContentService
    {
        private ImageViewTemplate _imageView;

        public ImageViewContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
        }
        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            ViewReference vr = _action.ViewReference;
            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken);
            }

            _imageView = new ImageViewTemplate(vr);

            _logService.LogDebug("End PrepareContentAsync");
        }

        public async Task<string> GetImageViewResourcePath(CancellationToken cancellationToken)
        {
            string imageViewName = _imageView.ImageView();

            if (!string.IsNullOrWhiteSpace(imageViewName))
            {
                if (imageViewName.Contains("{language}"))
                {
                    imageViewName = imageViewName.Replace("{language}", _sessionContext.LanguageCode);
                }


                ConfigResource configResource = _configurationService.GetConfigResource(imageViewName);
                if (configResource != null)
                {
                    if (configResource != null && !string.IsNullOrWhiteSpace(configResource.FileName))
                    {
                        return _sessionContext.ResourcePath(configResource.FileName);
                    }
                }
                else
                {
                    return await _crmDataService.GetDocumentPath(imageViewName, cancellationToken);
                }
            }

            return null;
        }
    }
}
