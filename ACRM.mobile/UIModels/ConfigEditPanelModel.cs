using System;
using ACRM.mobile.Domain.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.CustomControls.SettingsEditControls.Models;
using System.Threading;

namespace ACRM.mobile.UIModels
{
    public class ConfigEditPanelModel : ConfigPanelModel
    {
        public ConfigEditPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(widgetArgs, parentCancellationTokenSource)
        {
        }

        public List<WebConfigData> GetConfigData()
        {
            List<WebConfigData> results = new List<WebConfigData>();
            if(Widgets?.Count>0)
            {
                foreach(var widget in Widgets)
                {
                    if(widget is BaseConfigControlModel ctrl)
                    {
                        results.Add(ctrl.ConfigData);
                    }
                }
            }
            return results;

        }

        public async override ValueTask<bool> InitializeControl()
        {
            var result = await base.InitializeControl();
            if (Items?.Count > 0)
            {
                Widgets = new ObservableCollection<UIWidget>();
                foreach (var item in Items)
                {
                    Widgets.Add(await GetWidget(item));
                }
             }
            return result;

        }

        private async Task<UIWidget> GetWidget(WebConfigData item)
        {
            UIWidget widget;
            if (item.ControlType.Equals("Checkbox"))
            {
                widget = new CheckboxConfigControlModel(item, _cancellationTokenSource);  
            }
            else if (item.ControlType.Equals("Combobox"))
            {
                widget = new ComboboxConfigControlModel(item, _cancellationTokenSource);

            }
            else
            {
                widget = new BaseConfigControlModel(item, _cancellationTokenSource);
            }
            widget.ParentBaseModel = this;
            await widget.InitializeControl();
            return widget;
        }
    }
}

