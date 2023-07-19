using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls.SettingsEditControls.Models;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.UIModels
{
    public class UserConfigEditPanelModel : UserConfigPanelModel
    {
        public UserConfigEditPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(widgetArgs, parentCancellationTokenSource)
        {
        }

        public List<WebConfigData> GetConfigData()
        {
            List<WebConfigData> results = new List<WebConfigData>();
            if (Widgets?.Count > 0)
            {
                foreach (var widget in Widgets)
                {
                    if (widget is BaseConfigControlModel ctrl)
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
                Widgets = new ObservableCollection<ViewModels.Base.UIWidget>();
                foreach (var item in Items)
                {
                    Widgets.Add(await GetWidget(item));
                }
            }
            return result;

        }

        private async Task<ViewModels.Base.UIWidget> GetWidget(WebConfigData item)
        {
            ViewModels.Base.UIWidget widget;
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

