using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class ImageViewTemplate : ActionTemplateBase
    {
        public ImageViewTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string ImageView()
        {
            return GetValue("ImageName");
        }
    }
}
