using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class UnsupportedTypeModel : UIPanelWidget
    {
        public UnsupportedTypeModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is UserAction)
            {
                var userAction = widgetArgs as UserAction;
                ErrorMessageText = UserActionNotSupportedErrorMessage(userAction);
            }
            else if(widgetArgs is PanelData)
            {
                PanelData _inputArgs = widgetArgs as PanelData;
                ErrorMessageText = UserActionNotSupportedErrorMessage(_inputArgs.PanelTypeKey, _inputArgs.Label);
            }

        }

        public async override ValueTask<bool> InitializeControl()
        {
            return true;
        }
    }
}
