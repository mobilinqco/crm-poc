using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
	public class WebConfigData : INotifyPropertyChanged
    {
		public WebConfigData()
		{
		}

        private string _stringValue = string.Empty;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                if (_stringValue != value)
                {
                    _stringValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _controlType = string.Empty;
        public string ControlType
        {
            get => _controlType;
            set
            {
                if (_controlType != value)
                {
                    _controlType = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _label = string.Empty;
        public string InputLabel
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<WebConfigOption> Options { get; set; }
        public string RawValue { get; set; }
        public string UpdatedRawValue { get; set; }
        public string Name { get; set; }
        public bool IsChanged
        {
            get
            {
                return RawValue != UpdatedRawValue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void SetStringValue()
        {
            if(ControlType == "Checkbox")
            {
                StringValue = RawValue == "1" ? "Yes" : "No";
            }
        }
    }
}

