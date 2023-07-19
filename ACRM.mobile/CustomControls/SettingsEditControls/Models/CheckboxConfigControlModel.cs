using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.CustomControls.SettingsEditControls.Models
{
    public class CheckboxConfigControlModel : BaseConfigControlModel
    {
        public CheckboxConfigControlModel(WebConfigData config, CancellationTokenSource parentCancellationTokenSource)
            : base(config, parentCancellationTokenSource)
        {
            var values = new List<SelectableFieldValue>();
            values.Add(new SelectableFieldValue()
            {
                DisplayValue = "No",
                RecordId = "No",
                Id = 0
            });

            values.Add(new SelectableFieldValue()
            {
                DisplayValue = "Yes",
                RecordId = "Yes",
                Id = 1
            });

            AllowedValues = new ObservableCollection<SelectableFieldValue>(values);

        }

        public override WebConfigData ConfigData
        {
            get
            {
                _configData.UpdatedRawValue = SelectedValue?.Id.ToString();
                return _configData;
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            var result = await base.InitializeControl();

            if(AllowedValues?.Count>0)
            {
                SelectedValue = AllowedValues.Where(a=> a.RecordId.Equals(StringValue)).FirstOrDefault();
                _configData.RawValue = SelectedValue?.Id.ToString();
            }

            return result;
        }
    }

}

