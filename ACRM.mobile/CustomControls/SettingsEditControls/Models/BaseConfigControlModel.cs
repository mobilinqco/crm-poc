using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.CustomControls.SettingsEditControls.Models
{
	public class BaseConfigControlModel : ViewModels.Base.UIWidget
    {
        private string _inputLabel;
        public string InputLabel
        {
            get => _inputLabel;
            set
            {
                _inputLabel = value;
                RaisePropertyChanged(() => InputLabel);
            }
        }
        
        public bool IsMandatory { get; set; } = false;
        public bool ControlEnabled { get; set; } = true;

        private string _stringValue;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                _stringValue = value;
                RaisePropertyChanged(() => StringValue);
            }
        }

        private SelectableFieldValue _selectedValue;
        public SelectableFieldValue SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                RaisePropertyChanged(() => SelectedValue);
            }
        }

        protected WebConfigData _configData;
        public virtual WebConfigData ConfigData
        {
            get
            {
                _configData.UpdatedRawValue = StringValue;
                return _configData;
            }
            set
            {
                _configData = value;
            }
        }

        private ObservableCollection<SelectableFieldValue> _allowedValues = new ObservableCollection<SelectableFieldValue>();
        public ObservableCollection<SelectableFieldValue> AllowedValues
        {
            get => _allowedValues;
            set
            {
                _allowedValues = value;
                OnPropertyChanged();
            }
        }
        public BaseConfigControlModel(WebConfigData config, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            ConfigData = config;
        }

        public async override ValueTask<bool> InitializeControl()
        {
            StringValue = ConfigData.StringValue;
            InputLabel = ConfigData.InputLabel;
            return true;
        }
    }
}

