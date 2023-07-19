using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.CustomControls.SettingsEditControls.Models
{
    public class ComboboxConfigControlModel : BaseConfigControlModel
    {
        public ComboboxConfigControlModel(WebConfigData config, CancellationTokenSource parentCancellationTokenSource)
            : base(config, parentCancellationTokenSource)
        {
            var values = new List<SelectableFieldValue>();
            if (config?.Options != null)
            {
                foreach (var option in config.Options)
                {

                    values.Add(new SelectableFieldValue()
                    {
                        DisplayValue = option.Label,
                        RecordId = option.Value,
                        Id = option.Id,
                    });
                }
            }

            AllowedValues = new ObservableCollection<SelectableFieldValue>(values);
        }
        public override WebConfigData ConfigData
        {
            get
            {
                _configData.UpdatedRawValue = SelectedValue?.RecordId;
                return _configData;
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            var result = await base.InitializeControl();

            if (AllowedValues?.Count > 0 && ConfigData?.RawValue != null)
            {
                SelectedValue = AllowedValues.Where(a => a.RecordId.Equals(ConfigData?.RawValue)).FirstOrDefault();
            }

            return result;
        }
    }
}

