using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ACRM.mobile.Domain.Application
{
    public class CrmFieldEditData: INotifyPropertyChanged
    {
        public object ChangeOfflineRequest = null;
        public string RawValue = string.Empty;
        public bool wasSetByFilter = false;

        public string DefaultStringValue = string.Empty;
        public SelectableFieldValue DefaultSelectedValue;

        private string _stringValue = string.Empty;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                if(_stringValue != value)
                {
                    _stringValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private SelectableFieldValue _selectedValue;
        public SelectableFieldValue SelectedValue
        {
            get => _selectedValue;
            set
            {
                if(_selectedValue != value)
                {
                    _selectedValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasDataChanged
        {
            get
            {
                return HasStringChanged || HasValueChanged;
            }
        }

        public bool HasStringChanged
        {
            get
            {
                // test if both are empty. some fields require a space to be properly displayed on the UI.
                if(string.IsNullOrWhiteSpace(DefaultStringValue) && string.IsNullOrWhiteSpace(_stringValue))
                {
                    return false;
                }

                // test for date and time (they are having a defaultstringvalue with "-" or ":"
                return !DefaultStringValue.Equals(_stringValue)
                    && !DefaultStringValue.Replace("-", "").Equals(_stringValue)
                    && !DefaultStringValue.Replace(":", "").Equals(_stringValue);
            }
        }

        public bool HasValueChanged
        {
            get
            {
                var selectedRecordId = _selectedValue?.RecordId ?? "-1";
                var defaultRecordId = DefaultSelectedValue?.RecordId ?? "-1";
                if (string.IsNullOrWhiteSpace(selectedRecordId))
                {
                    selectedRecordId = "-1";
                }

                if (string.IsNullOrWhiteSpace(defaultRecordId))
                {
                    defaultRecordId = "-1";
                }

                return !defaultRecordId.Equals(selectedRecordId);
            }
        }

        public bool HasData
        {
            get
            {
                return HasStringData || HasValueData;
            }
        }

        public bool HasStringData
        {
            get
            {
                return !string.IsNullOrWhiteSpace(StringValue);
            }
        }

        public bool HasValueData
        {
            get
            {
                var selectedRecordId = SelectedValue?.RecordId ?? "-1";
                return !selectedRecordId.Equals("-1");
            }
        }

        public string SelectedRawValue()
        {
            if(string.IsNullOrWhiteSpace(RawValue))
            {
                if(!string.IsNullOrWhiteSpace(SelectedValue?.RecordId) && !string.Equals(SelectedValue?.RecordId, "-1"))
                {
                    return SelectedValue.RecordId;
                }
                else if (!string.IsNullOrWhiteSpace(StringValue))
                {
                    return StringValue;
                }
            }

            return RawValue;
        }

        public string ValueForFilter()
        {
            return _selectedValue?.RecordId ?? _stringValue;
        }

        public CrmFieldEditData()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void InitaizeEdit()
        {
            _selectedValue = DefaultSelectedValue;
            _stringValue = DefaultStringValue;
        }
    }
}
