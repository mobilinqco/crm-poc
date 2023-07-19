using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class PanelControlModel : UIPanelWidget
    {
        public PanelControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
                HasData = Data.HasData();
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            return true;
        }
    }
}
