using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class ConfigPanelModel : UIWidget
    {
        protected readonly IConfigurationService _configService;
        public WebConfigLayoutTab Panel { get; set; }
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private List<WebConfigData> _items;
        public List<WebConfigData> Items
        {
            get => _items;
            set
            {
                _items = value;
                RaisePropertyChanged(() => Items);
            }
        }

        public ConfigPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is WebConfigLayoutTab)
            {
                Panel = widgetArgs as WebConfigLayoutTab;
            }
            _configService = Utils.AppContainer.Resolve<IConfigurationService>();
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (Panel != null)
            {
                Title = Panel.Label;
                var listItems = new List<WebConfigData>();
                if (Panel.Fields?.Count > 0)
                {
                    foreach (var item in Panel.Fields)
                    {
                        var data = new WebConfigData()
                        {
                            InputLabel = item.Label,
                            ControlType = item.FieldType,
                            Name = item.ValueName,
                            StringValue = GetConfigValue(item),
                            Options = item.options,
                            RawValue = _configService.GetConfigValue(item.ValueName)?.Value
                        };
                        listItems.Add(data);
                    }

                }
                Items = listItems;
            }
            return true;

        }

        private string GetConfigValue(WebConfigLayoutField item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item.FieldType.Equals("Checkbox"))
            {
                return _configService.GetBoolConfigValue(item.ValueName) ? "Yes" : "No";
            }
            var config = _configService.GetConfigValue(item.ValueName);
            if (item.FieldType.Equals("Combobox") && config != null)
            {
                var option = item.options?.Find(a => a.Value.Equals(config.Value));
                if (option != null)
                {
                    return option.Label;
                }
            }
            return config?.Value;
        }
    }
}

