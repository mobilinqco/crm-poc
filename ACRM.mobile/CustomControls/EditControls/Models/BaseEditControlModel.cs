using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Org.BouncyCastle.Asn1.Pkcs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class BaseEditControlModel : UIWidget
    {
        public bool Hidden { get => Field.Config.PresentationFieldAttributes.Hide; }
        public string InputLabel { get; set; }     

        private string _fieldLabel;
        public string FieldLabel
        {
            get => _fieldLabel;
            set
            {
                _fieldLabel = value;
                RaisePropertyChanged(() => FieldLabel);
            }
        }

        private bool _ctrlEnabled = true;
        public bool ControlEnabled
        {
            get => _ctrlEnabled;
            set
            {
                _ctrlEnabled = value;
                RaisePropertyChanged(() => ControlEnabled);
            }
        }

        internal bool _skipValueChangedMessage = false;

        public virtual string StringValue
        {
            get => Field?.EditData?.StringValue;
            set
            {
                if (Field?.EditData?.StringValue != value)
                {                    
                    Field.EditData.StringValue = value;
                    RaisePropertyChanged(() => StringValue);

                    if (_skipValueChangedMessage)
                    {
                        _skipValueChangedMessage = false;
                        return;
                    }

                    if (NotifyValueChanged)
                    {
                        string fieldFunction = Field?.Config?.FieldConfig?.Function;
                        if (!string.IsNullOrEmpty(fieldFunction))
                        {
                            new Action(async () => await PublishMessage(new WidgetMessage
                            {
                                ControlKey = fieldFunction,
                                Data = value,
                                EventType = WidgetEventType.ValueChanged
                            }, MessageDirections.ToParent))();
                          }
                    }
                }
            }
        }

        public virtual SelectableFieldValue SelectedValue
        {
            get => Field?.EditData?.SelectedValue;
            set
            {
                if (Field?.EditData?.SelectedValue != value)
                {
                    Field.EditData.SelectedValue = value;
                    OnPropertyChanged();

                    if (_skipValueChangedMessage)
                    {
                        _skipValueChangedMessage = false;
                        return;
                    }

                    if (NotifyValueChanged)
                    {
                        string fieldFunction = Field?.Config?.FieldConfig?.Function;
                        if (!string.IsNullOrEmpty(fieldFunction))
                        {
                            new Action(async () => await PublishMessage(new WidgetMessage
                            {
                                ControlKey = fieldFunction,
                                Data = value,
                                EventType = WidgetEventType.ValueChanged
                            }, MessageDirections.ToParent))();
                        }
                    }
                }
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

        private ListDisplayField _field;
        public ListDisplayField Field
        {
            get => _field;
            set
            {
                _field = value;
                RaisePropertyChanged(() => Field);
            }
        }

        public virtual List<ListDisplayField> Fields
        {
            get
            {
                if (_field == null)
                {
                    return null;
                }
                else
                {
                    if (_field.EditData.HasDataChanged)
                    {
                        _field.EditData.ChangeOfflineRequest = ChangeOfflineRequest;
                    }
                    return new List<ListDisplayField> { _field };
                }

            }
        }

        public virtual object ChangeOfflineRequest
        {
            get
            {
                if (_field.EditData.HasStringChanged)
                {
                    string oldValue = Field.EditData.DefaultStringValue;
                    if(string.IsNullOrWhiteSpace(oldValue))
                    {
                        oldValue = string.Empty;
                    }

                    string newValue = Field.EditData.StringValue.Trim();

                    if(!string.IsNullOrWhiteSpace(newValue)
                        && Field.Config.PresentationFieldAttributes.FieldInfo.IsPercent
                        && Field.Config.PresentationFieldAttributes.FieldInfo.IsReal)
                    {
                        if(double.TryParse(newValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double result))
                        {
                            result = result / 100;
                            newValue = $"{result}";
                        }
                    }

                    return new OfflineRecordField()
                    {
                        FieldId = Field.Config.FieldConfig.FieldId,
                        NewValue = newValue,
                        OldValue = oldValue,
                        Offline = 0
                    };
                }
                else if (_field.EditData.HasValueChanged)
                {
                    var newValue = Field.EditData.SelectedValue != null ? Field.EditData.SelectedValue.RecordId : string.Empty;
                    return new OfflineRecordField()
                    {
                        FieldId = Field.Config.FieldConfig.FieldId,
                        NewValue = newValue,
                        Offline = 0
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        private bool _isMandatory;
        public bool IsMandatory
        {
            get => _isMandatory;
            set
            {
                _isMandatory = value;
                RaisePropertyChanged(() => IsMandatory);
            }
        }

        private FontAttributes _labelFontAttributes = FontAttributes.None;
        public FontAttributes LabelFontAttributes
        {
            get => _labelFontAttributes;
            set
            {
                _labelFontAttributes = value;
                RaisePropertyChanged(() => LabelFontAttributes);
            }
        }


        private Color _labelColor = Color.Gray;
        public Color LabelColor
        {
            get => _labelColor;
            set
            {
                _labelColor = value;
                RaisePropertyChanged(() => LabelColor);
            }
        }

        public BaseEditControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            InitaizeEdit(field);
        }

        protected virtual void InitaizeEdit(ListDisplayField field)
        {
            Field = field;
            Field.EditData.InitaizeEdit();

            if (field != null)
            {

                InitaizeDefaultValue();
               
                FieldLabel = field.Config.PresentationFieldAttributes.Label();

                SetInputLabel();

                string fieldFunction = field?.Config?.FieldConfig?.Function;
                if (!string.IsNullOrEmpty(fieldFunction))
                {
                    if (fieldFunction.StartsWith("FixedValue:"))
                    {
                        var parts = fieldFunction.Split(':');
                        if (parts.Length > 1)
                        {
                            StringValue = parts[1];
                        }
                    }
                    else
                    {
                        RegisterMessageIfNotExist(WidgetEventType.ClearFieldValues, fieldFunction, ClearFieldValuesEventHandler);
                        RegisterMessageIfNotExist(WidgetEventType.SetSelectedResult, fieldFunction, SetSelectedResultEventHandler);
                        RegisterMessageIfNotExist(WidgetEventType.SetValue, fieldFunction, SetStringValueEventHandler);
                    }
                }

                RegisterMessageIfNotExist(WidgetEventType.SetValueByFieldId, $"{field.Config.FieldConfig?.InfoAreaId}_{field.Config.FieldConfig?.FieldId}", SetRawValueEventHandler);

                RegisterMessageIfNotExist(WidgetEventType.FieldValidationError, $"{field.Config.FieldConfig?.InfoAreaId}_{field.Config.FieldConfig?.FieldId}", FieldValidationErrorEventHandler);
                RegisterMessageIfNotExist(WidgetEventType.ClearFieldValidationError, "*", ClearFieldValidationErrorEventHandler);

                if (field.Config.PresentationFieldAttributes.ReadOnly)
                {
                    ControlEnabled = false;
                }
            }
        }

        private void InitaizeDefaultValue()
        {
            var defaultValue = Field.Config.PresentationFieldAttributes.ExtendedOptionForKey("DefaultValue");
            if (!string.IsNullOrEmpty(defaultValue))
            {
                if (AllowedValues?.Count > 0 && AllowedValues.Any(a => a.RecordId.Equals(defaultValue)))
                {
                    Field.EditData.SelectedValue = AllowedValues.Where(a => a.RecordId.Equals(defaultValue)).FirstOrDefault();
                }
                else
                {
                    Field.EditData.StringValue = defaultValue;
                }
            }
        }

        private void SetInputLabel()
        {
            if (!Field.Config.PresentationFieldAttributes.NoLabel)
            {
                InputLabel = FieldLabel;
            }

            if (Field.Config.PresentationFieldAttributes.Must && !Field.Config.PresentationFieldAttributes.ReadOnly)
            {
                IsMandatory = true;
                LabelFontAttributes = FontAttributes.Bold;
            }

            if(Field.Config.PresentationFieldAttributes.ReadOnly)
            {
                LabelFontAttributes = FontAttributes.Italic;
            }

            LabelColor = Color.Gray;
        }

        internal virtual async Task SetSelectedResultEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is SelectedRecordFieldData selectedData)
            {
                if (Field.Config.PresentationFieldAttributes.IsSelectableInput() && Field.Config.RecordSelectorAction == null)
                {
                    if (Field.Config.AllowedValues != null && Field.Config.AllowedValues.Count > 0)
                    {
                        var item = Field.Config.AllowedValues.Find(a => a.DisplayValue.Equals(selectedData.StringValue, StringComparison.CurrentCultureIgnoreCase));
                        if(selectedData.ParentCode > 0)
                        {
                            item = Field.Config.AllowedValues.Find(a => a.DisplayValue.Equals(selectedData.StringValue, StringComparison.CurrentCultureIgnoreCase) && a.ParentCode == selectedData.ParentCode);
                        }

                        if (item != null)
                        {
                            SelectedValue = item;
                        }
                    }
                }
                else
                {
                    StringValue = selectedData.StringValue;
                    
                    if (Field.Config.FieldConfig.InfoAreaId.Equals(selectedData.SelectedField.RowDecorators.Expand?.InfoAreaId))
                    {
                        SelectedValue = selectedData.SelectedValue;
                        _logService.LogDebug($"Setting the value {selectedData.StringValue} for {Field.Config.FieldConfig.Function} ");
                    }

                }
                
            }

        }

        internal async Task SetStringValueEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string selectedValue)
            {
                if (arg.SubData != null && arg.SubData is bool skipValueChangedMessage)
                {
                    _skipValueChangedMessage = skipValueChangedMessage;
                }
                StringValue = selectedValue;
            }
        }

        internal virtual async Task SetRawValueEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string selectedValue)
            {
                if (arg.SubData != null && arg.SubData is bool skipValueChangedMessage)
                {
                    _skipValueChangedMessage = skipValueChangedMessage;
                }
                StringValue = selectedValue;


                if (Field.Config.PresentationFieldAttributes.IsSelectableInput() && Field.Config.RecordSelectorAction == null)
                {
                    if (arg.SubData != null && arg.SubData is bool skipValueChangedMeg)
                    {
                        _skipValueChangedMessage = skipValueChangedMeg;
                    }
                    if (Field.Config.AllowedValues != null && Field.Config.AllowedValues.Count > 0)
                    {
                        var item = Field.Config.AllowedValues.Find(a => a.RecordId.Equals(selectedValue, StringComparison.CurrentCultureIgnoreCase));

                        if (item != null)
                        {
                            SelectedValue = item;
                        }
                    }

                }
            }

        }

        private async Task ClearFieldValuesEventHandler(WidgetMessage arg)
        {
            _logService.LogDebug($"Clearing the value for {Field.Config.FieldConfig.Function} ");
            SelectedValue = null;
            StringValue = string.Empty;
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }

        internal async Task FieldValidationErrorEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string fieldKey)
            {
                LabelColor = Color.Red;
            }
        }

        internal async Task ClearFieldValidationErrorEventHandler(WidgetMessage arg)
        {
            LabelColor = Color.Gray;
        }
    }
}
