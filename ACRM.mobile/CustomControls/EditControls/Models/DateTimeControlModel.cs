using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Formatters;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class DateTimeControlModel : BaseEditControlModel
    {
        public ICommand DateSelectorEntryCommand => new Command<ListDisplayField>(async (field) => await DateSelectorEntryTapped(field));

        private bool _hasDate = false;
        public bool HasDate
        {
            get => _hasDate;
            set
            {
                _hasDate = value;
                RaisePropertyChanged(() => HasDate);
            }
        }

        private bool _hasTime = false;
        public bool HasTime
        {
            get => _hasTime;
            set
            {
                _hasTime = value;
                RaisePropertyChanged(() => HasTime);
            }
        }

        private DateTime _selectedDataTime;
        public DateTime SelectedDataTime
        {
            get => _selectedDataTime;
            set
            {
                _selectedDataTime = value;
                RaisePropertyChanged(() => SelectedDataTime);
            }
        }

        private string _dateTimeString;
        public string DateTimeString
        {
            get => _dateTimeString;
            set
            {
                if (_dateTimeString != value)
                {
                    _dateTimeString = value;
                    RaisePropertyChanged(() => DateTimeString);
                   
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
                            var dateData = value;
                            var data = dateData.Replace("-", "").Replace(":", "");
                            new Action(async () => await PublishMessage(new WidgetMessage
                            {
                                ControlKey = fieldFunction,
                                Data = data,
                                EventType = WidgetEventType.ValueChanged
                            }, MessageDirections.ToParent))();
                        }
                    }
                }
            }
        }

        public ListDisplayField DateField
        {
            get
            {
                if (Field.Data.ColspanData != null)
                {
                    foreach (var ldf in Field.Data.ColspanData)
                    {
                        if (ldf.Config.PresentationFieldAttributes.IsDate)
                        {
                            return ldf;
                        }
                    }
                }

                if (Field.Config.PresentationFieldAttributes.IsDate)
                {
                    return Field;
                }
                else
                {
                    return null;
                }
            }

        }

        public ListDisplayField TimeField
        {
            get
            {
                if (Field.Data.ColspanData != null)
                {
                    foreach (var ldf in Field.Data.ColspanData)
                    {
                        if (ldf.Config.PresentationFieldAttributes.IsTime)
                        {
                            return ldf;
                        }
                    }
                }

                if (Field.Config.PresentationFieldAttributes.IsTime)
                {
                    return Field;
                }
                else
                {
                    return null;
                }
            }
        }

        public override List<ListDisplayField> Fields
        {
            get
            {
                string tZAdjDateTimeString = getTimeZoneAdjusted(DateTimeString);
                if (HasDate && HasTime)
                {
                    var dateField = DateField;
                    var timeField = TimeField;
                    if (!string.IsNullOrWhiteSpace(tZAdjDateTimeString) && tZAdjDateTimeString.Length > 11)
                    {
                        dateField.EditData.StringValue = tZAdjDateTimeString.Substring(0, 10);
                        timeField.EditData.StringValue = tZAdjDateTimeString.Substring(11);

                        if (dateField.EditData.HasDataChanged)
                        {
                            dateField.EditData.ChangeOfflineRequest = new OfflineRecordField()
                            {
                                FieldId = dateField.Config.FieldConfig.FieldId,
                                NewValue = dateField.EditData.StringValue.Replace("-", "").Replace(":", ""),
                                OldValue = dateField.EditData.DefaultStringValue.Replace("-", "").Replace(":", ""),
                                Offline = 0
                            };
                        }

                        if (timeField.EditData.HasDataChanged)
                        {
                            timeField.EditData.ChangeOfflineRequest = new OfflineRecordField()
                            {
                                FieldId = timeField.Config.FieldConfig.FieldId,
                                NewValue = timeField.EditData.StringValue.Replace("-", "").Replace(":", ""),
                                OldValue = timeField.EditData.DefaultStringValue.Replace("-", "").Replace(":", ""),
                                Offline = 0
                            };
                        }
                    }

                    return new List<ListDisplayField> { dateField, timeField };

                }
                else
                {
                    Field.EditData.StringValue = tZAdjDateTimeString.Replace("-", "").Replace(":", "");
                    if (Field.EditData.HasDataChanged)
                    {
                        Field.EditData.ChangeOfflineRequest = ChangeOfflineRequest;
                    }
                    return new List<ListDisplayField> { Field };
                }

            }
        }

        private string getTimeZoneAdjusted(string dTString)
        {
            if(_sessionContext?.ServerTimeZone == null || string.IsNullOrWhiteSpace(dTString))
            {
                return dTString;
            }

            DateTime selectedDateTime = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(dTString))
            {
                if (DateTimeString.Length == 16)
                {
                    selectedDateTime = DateTime.ParseExact(dTString, CrmConstants.DateTimeFormat, null).InServerTimeZone(_sessionContext.ServerTimeZone);
                    return selectedDateTime.ToString(CrmConstants.DateTimeFormat);
                }
                else if (dTString.Length == 10)
                {
                    return dTString;
                }
                else if (dTString.Length == 5)
                {
                    selectedDateTime = DateTime.ParseExact(dTString, CrmConstants.TimeFormat, null).InServerTimeZone(_sessionContext.ServerTimeZone);
                    return selectedDateTime.ToString(CrmConstants.TimeFormat);
                }
                else if (dTString.Length == 4)
                {
                    selectedDateTime = DateTime.ParseExact(dTString, CrmConstants.DbFieldTimeFormat, null).InServerTimeZone(_sessionContext.ServerTimeZone);
                    return selectedDateTime.ToString(CrmConstants.DbFieldTimeFormat);
                }

            }

            return dTString;
        }

        internal override async Task SetRawValueEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string selectedValue)
            {
                if (arg.SubData != null && arg.SubData is bool skipValueChangedMessage)
                {
                    _skipValueChangedMessage = skipValueChangedMessage;
                }

                if (HasDate && selectedValue.Length == 8)
                {
                    selectedValue = $"{selectedValue.Substring(0, 4)}-{selectedValue.Substring(4, 2)}-{selectedValue.Substring(6, 2)}";
                }
                else if (HasTime && selectedValue.Length == 4)
                {
                    selectedValue = $"{selectedValue.Substring(0, 2)}:{selectedValue.Substring(2, 2)}";
                }

                DateTimeString = selectedValue;
                DateTime selectedDateTime = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(DateTimeString))
                {
                    if (DateTimeString.Length == 16)
                    {
                        selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateTimeFormat, null);
                    }
                    else if (DateTimeString.Length == 10)
                    {
                        selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateFormat, null);
                    }
                    else if (DateTimeString.Length == 5)
                    {
                        selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.TimeFormat, null);
                    }
                    else if (DateTimeString.Length == 4)
                    {
                        selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DbFieldTimeFormat, null);
                    }

                    SelectedDataTime = selectedDateTime;
                    Field.EditData.StringValue = DateTimeString;
                }
            }

        }

        public override object ChangeOfflineRequest
        {
            get
            {
                if (Field == null)
                {
                    return null;
                }
                return new OfflineRecordField()
                {
                    FieldId = Field.Config.FieldConfig.FieldId,
                    NewValue = Field.EditData.StringValue.Replace("-", "").Replace(":", ""),
                    OldValue = Field.EditData.DefaultStringValue.Replace("-", "").Replace(":", ""),
                    Offline = 0
                };

            }
        }

        private async Task DateSelectorEntryTapped(ListDisplayField field)
        {
            if (!field.Config.PresentationFieldAttributes.ReadOnly)
            {
                NotifyDirtyState();
                var message = new WidgetMessage();
                message.EventType = WidgetEventType.DateTimeEntryTapped;
                message.Data = new DateTimePopupData
                {
                    DateTimeString = DateTimeString,
                    SelectedDataTime = SelectedDataTime,
                    HasDate = HasDate,
                    HasTime = HasTime,
                    Title = FieldLabel,
                    FieldIdentification = Field.Config.FieldConfig.FieldIdentification()
                };
                message.ControlKey = "DateTimeEntryTapped";
                if (ParentBaseModel != null)
                {
                    await ParentBaseModel?.PublishMessage(message);
                }
            }
        }

        public DateTimeControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
            RegisterMessageIfNotExist(WidgetEventType.DateTimeSelected, Field.Config.FieldConfig.FieldIdentification(), SetSelectedDateTimeEventHandler);

            HasDate = DateField != null;
            HasTime = TimeField != null;

            DateTimeString = DateTimeFormatter.FormatedEditDateFromDbString(Field.EditData.RawValue, Field.Config.PresentationFieldAttributes);

            if (HasDate && HasTime)
            {
                var dateField = DateField;
                var timeField = TimeField;

                DateTimeString = $"{DateTimeFormatter.FormatedEditDateFromDbString(dateField.EditData.RawValue, dateField.Config.PresentationFieldAttributes)} {DateTimeFormatter.FormatedEditDateFromDbString(timeField.EditData.RawValue, timeField.Config.PresentationFieldAttributes)}".Trim();

            }

            if (string.IsNullOrEmpty(DateTimeString))
            {
                DateTimeString = " ";
            }

            // End Workaround

            DateTime selectedDateTime = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(DateTimeString))
            {
                if (DateTimeString.Length == 16)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateTimeFormat, null);
                }
                else if (DateTimeString.Length == 10)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateFormat, null);
                }
                else if (DateTimeString.Length == 5)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.TimeFormat, null);
                }
                else if (DateTimeString.Length == 4)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DbFieldTimeFormat, null);
                }
            }
            SelectedDataTime = selectedDateTime;
        }

        private Task SetSelectedDateTimeEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is DateTimePopupData selectedData)
            {
                SelectedDataTime = selectedData.SelectedDataTime;
                DateTimeString = selectedData.DateTimeString;
            }

            return Task.CompletedTask;
        }
    }
}
