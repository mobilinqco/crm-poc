using System;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.CustomControls
{
    public interface IUserActionResolver
    {
        Type Resolve(UserAction userAction);
        Type ResolveViewReference(ViewReference viewReference);
    }
}
