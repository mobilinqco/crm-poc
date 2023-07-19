using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;

namespace ACRM.mobile.UIModels
{
    public class SerialEntryEditPanelControlModel : EditPanelControlModel
    {
        public SerialEntryEditPanelControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(widgetArgs, parentCancellationTokenSource)
        {
            NotifyValueChanged = true;
        }
 
        public async override ValueTask<bool> InitializeControl()
        {
            var result = await base.InitializeControl();
            return result;
        }

        public override PanelData ProcessedData
        {
            get
            {
                var data = base.ProcessedData;
                if(data== null)
                {
                    return data;
                }
                data.FourceUpdate = true;
                return data;
            }
        }
    }
}
