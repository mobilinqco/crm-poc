using System;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services.SubComponents
{
    public class ImageResolverComponent
    {
        private readonly ISessionContext _sessionContext;
        private readonly ILogService _logService;

        public ImageResolverComponent(ISessionContext sessionContext, ILogService logService)
        {
            _sessionContext = sessionContext;
            _logService = logService;
        }

        public (string imageName, string glyphText) ExtractImage(IConfigurationService configurationService, string resourceName)
        {
            string imageName = null;
            string glyphText = null;
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                resourceName = "Icon:Record";
            }

            ConfigResource imageResource = configurationService.GetConfigResource(resourceName);

            if (imageResource != null)
            {
                if (imageResource.FileName.StartsWith("\\"))
                {
                    glyphText = imageResource.FileName;
                    if (glyphText.StartsWith("\\e"))
                    {
                        // XAML use: "&#xE198;"
                        glyphText = GlyphIconCode.ConvertCode(glyphText);
                    }
                    else
                    {
                        try
                        {
                            int hexResult = int.Parse(glyphText.Remove(0, 1), System.Globalization.NumberStyles.HexNumber, null);
                            glyphText = char.ConvertFromUtf32(hexResult);

                        }
                        catch (Exception ex)
                        {
                            _logService.LogError($"{"Unable to parse glyph text: " + imageResource.FileName + ". Error: " + ex.GetType().Name + " : " + ex.Message}");

                            glyphText = imageResource.FileName;
                        }
                    }
                }
                else
                {
                    imageName = _sessionContext.ResourcePath(imageResource.FileName);
                }
            }
            else
            {
                glyphText = GlyphIconCode.ResolveCode(resourceName);
            }

            return (imageName: imageName, glyphText: glyphText);
        }

        public (string image, string glyph) InoAreaImageTuple(InfoArea infoArea, IConfigurationService configurationService)
        {
            if (infoArea != null && !string.IsNullOrWhiteSpace(infoArea.ImageName))
            {
                return ImageTuple(infoArea.ImageName, configurationService);
            }

            return ("", "");
        }

        public (string image, string glyph) ImageTuple(string resourceName, IConfigurationService configurationService)
        {
            if (!string.IsNullOrWhiteSpace(resourceName))
            {
                return ExtractImage(configurationService, resourceName);
            }

            return ("", "");
        }
    }
}
