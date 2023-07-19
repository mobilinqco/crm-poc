using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Formatters;
using ACRM.mobile.ViewModels.Base;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class DashboardCalenderModel : UIWidget
    {
        private DateTime _selectedDate = DateTime.MinValue;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    SetSelectedCalenderDate(_selectedDate);
                    RaisePropertyChanged(() => SelectedDate);
                }
            }
        }
        private string _month;
        public string Month
        {
            get => _month;
            set
            {
                _month = value;
                RaisePropertyChanged(() => Month);
            }
        }
        private int _year;
        public int Year
        {
            get => _year;
            set
            {
                _year = value;
                RaisePropertyChanged(() => Year);
            }
        }
        private string _dayOfTheWeek;
        public string DayOfTheWeek
        {
            get => _dayOfTheWeek;
            set
            {
                _dayOfTheWeek = value;
                RaisePropertyChanged(() => DayOfTheWeek);
            }
        }
        private int _day;
        public int Day
        {
            get => _day;
            set
            {
                _day = value;
                RaisePropertyChanged(() => Day);
            }
        }
        private string _monthYear;
        public string MonthYear
        {
            get => _monthYear;
            set
            {
                _monthYear = value;
                RaisePropertyChanged(() => MonthYear);
            }
        }
        private SeasonType _season;
        public SeasonType Season
        {
            get => _season;
            set
            {
                _season = value;
                RaisePropertyChanged(() => Season);
            }
        }

        public ICommand OnSelectionChanged => new Command<Syncfusion.SfCalendar.XForms.SelectionChangedEventArgs>(async evt => await SelectionChanged(evt));
        public ICommand OnMonthChanged => new Command<Syncfusion.SfCalendar.XForms.MonthChangedEventArgs>(async evt => await MonthChanged(evt));

        private const int StandardWidthRequest = 220;
        private int _widthRequest = StandardWidthRequest;
        public int WidthRequest
        {
            get => _widthRequest;
            set
            {
                _widthRequest = value;
                RaisePropertyChanged(() => WidthRequest);
            }
        }

        private const int StandardHeightRequest = 190;
        private int _heightRequest = StandardHeightRequest;
        public int HeightRequest
        {
            get => _heightRequest;
            set
            {
                _heightRequest = value;
                RaisePropertyChanged(() => HeightRequest);
            }
        }

        private bool _changeDayOnMonthChange = false;

        public DashboardCalenderModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs != null && widgetArgs is FormItemData formItemData && formItemData.FormItem!=null)
            {

                Dictionary<string, object> args = formItemData.FormItem.ItemAttributesDictionary();
                if(args != null && args.ContainsKey("Scale"))
                {
                    double scale = 1.0;
                    if(args["Scale"].GetType() == typeof(double))
                    {
                        scale = (double)args["Scale"];
                    }

                    HeightRequest = Convert.ToInt32(Convert.ToDouble(HeightRequest) * scale);
                    WidthRequest = Convert.ToInt32(Convert.ToDouble(WidthRequest) * scale);
                }

                args = formItemData.FormItem.OptionsDictionary();
                if (args != null && args.ContainsKey("ChangeDayOnMonthChange"))
                {
                    if (args["ChangeDayOnMonthChange"].GetType() == typeof(bool))
                    {
                        _changeDayOnMonthChange = (bool)args["ChangeDayOnMonthChange"];
                    }
                }
            }
        }

        private async Task SelectionChanged(Syncfusion.SfCalendar.XForms.SelectionChangedEventArgs args)
        {
            if (args.DateAdded.Count > 0)
            {
                await SendCalenderDateChangeMessage(SelectedDate);
            }
        }

        private async Task SendCalenderDateChangeMessage(DateTime selectedDate)
        {
            Dictionary<string, string> calanderParams = new Dictionary<string, string>();
            var currentDate = selectedDate.ToString(CrmConstants.DbFieldDateFormat);
            var sunday = selectedDate.AddDays(-(int)selectedDate.DayOfWeek).ToString(CrmConstants.DbFieldDateFormat);
            var saturday = selectedDate.AddDays(-(int)selectedDate.DayOfWeek + (int)DayOfWeek.Saturday).ToString(CrmConstants.DbFieldDateFormat);
            calanderParams.Add("Param1", currentDate);
            calanderParams.Add("Param.1.1", sunday);
            calanderParams.Add("Param.1.2", saturday);
            await ParentBaseModel?.PublishMessage(new WidgetMessage()
            {
                EventType = WidgetEventType.FormItemChanged,
                ControlKey = $"${WidgetKey}",
                Data = calanderParams
            }, MessageDirections.ToParent);
        }

        private async Task MonthChanged(Syncfusion.SfCalendar.XForms.MonthChangedEventArgs args)
        {
            if(_changeDayOnMonthChange)
            {
                if(args.CurrentValue.Month == DateTime.Now.Month)
                {
                    SelectedDate = DateTime.Now;
                }
                else
                {
                    SelectedDate = new DateTime(args.CurrentValue.Year, args.CurrentValue.Month, 1);
                }
                
            }
        }

        private void SetSelectedCalenderDate(DateTime selectedDate)
        {
            Day = selectedDate.Day;
            Year = selectedDate.Year;
            DayOfTheWeek = selectedDate.DayOfWeek.ToString();
            Month = selectedDate.ToString("MMMM");
            Season = DateTimeFormatter.CalcStyleForCurrentDate(selectedDate);
            MonthYear = $"{selectedDate.ToString("MMMM")} {selectedDate.Year}";
        }

        public override async ValueTask<bool> InitializeControl()
        {
            SelectedDate = DateTime.Now;
            WidgetKey = WidgetConfig?.FormItem?.ValueName;
            await SendCalenderDateChangeMessage(SelectedDate);
            return true;
        }
    }
}
