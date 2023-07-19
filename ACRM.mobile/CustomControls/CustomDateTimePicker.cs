using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using Syncfusion.SfPicker.XForms;
using Xamarin.Forms;
using SelectionChangedEventArgs = Syncfusion.SfPicker.XForms.SelectionChangedEventArgs;

namespace ACRM.mobile.CustomControls
{
    public class CustomDateTimePicker: SfPicker
    {
        public static readonly BindableProperty SelectedDataTimeProperty = BindableProperty.Create(nameof(SelectedDataTime), typeof(DateTime), typeof(CustomDateTimePicker), DateTime.Now, propertyChanged: OnSelectedDatePropertyChanged);
        public static readonly BindableProperty SelectedDataTimeStringProperty = BindableProperty.Create(nameof(SelectedDataTimeString), typeof(string), typeof(CustomDateTimePicker), DateTime.Now.ToString(CrmConstants.DateTimeFormat, DateTimeFormatInfo.InvariantInfo));
        public static readonly BindableProperty HasDateProperty = BindableProperty.Create(nameof(HasDate), typeof(bool), typeof(CustomDateTimePicker), false, propertyChanged: OnCustomPropertyChanged);
        public static readonly BindableProperty HasTimeProperty = BindableProperty.Create(nameof(HasTime), typeof(bool), typeof(CustomDateTimePicker), false, propertyChanged: OnCustomPropertyChanged);
        public static readonly BindableProperty PopupActionProperty = BindableProperty.Create(nameof(PopupAction), typeof(PopupStatusAction), typeof(CustomDateTimePicker), PopupStatusAction.Open);

        public ICommand OnOKButtonClickedCommand => new Command(() => OnOKButtonClicked());
        public ICommand OnCancelButtonClickedCommand => new Command(() => OnCancelButtonClicked());

        #region Public Properties

        /// <summary>
        /// Date is the acutal DataSource for SfPicker control which will hold the collection of Day, Month, Year, Hour and Minute
        /// </summary>
        /// <value>The date.</value>
        public ObservableCollection<object> Date { get; set; }

        //Day is the collection of day numbers
        internal ObservableCollection<object> Day { get; set; }

        //Month is the collection of Month Names
        internal ObservableCollection<object> Month { get; set; }

        //Year is the collection of Years from 1990 to 2042
        internal ObservableCollection<object> Year { get; set; }

        //Hour is the collection of Hours in Railway time format
        internal ObservableCollection<object> Hour { get; set; }

        //Minute is the collection of Minutes from 00 to 59
        internal ObservableCollection<object> Minute { get; set; }


        /// <summary>
        /// Headers api is holds the column name for every column in date picker
        /// </summary>
        /// <value>The Headers.</value>
        public ObservableCollection<string> Headers { get; set; }

        private static void OnCustomPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null)
            {
                ((CustomDateTimePicker)bindable).InitalizeControl();
            }
        }

        private static void OnSelectedDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null && newValue is DateTime dt)
            {
                ((CustomDateTimePicker)bindable).SetSelectedDate(dt);
            }
        }

        public DateTime SelectedDataTime
        {
            get => (DateTime)GetValue(SelectedDataTimeProperty);
            set => SetValue(SelectedDataTimeProperty, value);
        }

        public string SelectedDataTimeString
        {
            get => (string)GetValue(SelectedDataTimeStringProperty);
            set => SetValue(SelectedDataTimeStringProperty, value);
        }

        public bool HasDate
        {
            get => (bool)GetValue(HasDateProperty);
            set => SetValue(HasDateProperty, value);
        }

        public bool HasTime
        {
            get => (bool)GetValue(HasTimeProperty);
            set => SetValue(HasTimeProperty, value);
        }

        public PopupStatusAction PopupAction
        {
            get => (PopupStatusAction)GetValue(PopupActionProperty);
            set => SetValue(PopupActionProperty, value);
        }

        public string OKButtonText { get; private set; }
        public string CancelButtonText { get; private set; }

        #endregion

        private int _startDate = 1910;
        private int _endDate = 2100;

        private SelectionChangedEventArgs _latestSelectionChangedEventArgs = null;

        private string _stringDateTimeFormat = CrmConstants.DateTimeFormat;
        private string _stringDateFormat = CrmConstants.DateFormat;
        protected readonly ILocalizationController _localizationController;

        private int MonthIndex
        {
            get
            {
                return DatePartIndex("m");
            }
        }

        private int DayIndex
        {
            get
            {
                return DatePartIndex("d");
            }
        }

        private int YearIndex
        {
            get
            {
                return DatePartIndex("y");
            }
        }

        public CustomDateTimePicker()
        {
            _localizationController = AppContainer.Resolve<ILocalizationController>();
            SelectionChanged += CustomDatePicker_SelectionChanged;
            InitalizeControl();
        }

        internal void InitalizeControl()
        {
            Date = new ObservableCollection<object>();
            Day = new ObservableCollection<object>();
            Month = new ObservableCollection<object>();
            Year = new ObservableCollection<object>();
            Hour = new ObservableCollection<object>();
            Minute = new ObservableCollection<object>();
            Headers = new ObservableCollection<string>();

            if (HasDate)
            {
                HeaderAddAtIndex(0);
                HeaderAddAtIndex(1);
                HeaderAddAtIndex(2);
            }

            if (HasTime)
            {
                Headers.Add(_localizationController.GetString(LocalizationKeys.TextGroupDate, LocalizationKeys.KeyDateHour));
                Headers.Add(_localizationController.GetString(LocalizationKeys.TextGroupDate, LocalizationKeys.KeyDateMinute));
            }

            PopulateDateCollection();

            ColumnHeaderText = Headers;

            ItemsSource = Date;

            OKButtonText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicOK);
            CancelButtonText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);

            BuildFooterView();

            ShowFooter = true;
            ShowHeader = true;
            ShowColumnHeader = true;

            SetSelectedDate(SelectedDataTime);
        }

        private void BuildFooterView()
        {
            BackgroundColor = Color.White;
            Grid footerGrid = new Grid();
            footerGrid.BackgroundColor = Color.White;
            footerGrid.HorizontalOptions = LayoutOptions.FillAndExpand;

            ColumnDefinitionCollection columnDefinitions = new ColumnDefinitionCollection();
            columnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            columnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            RowDefinitionCollection rowDefinitions = new RowDefinitionCollection();
            rowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            footerGrid.RowDefinitions = rowDefinitions;
            footerGrid.ColumnDefinitions = columnDefinitions;

            Button okButton = new Button();
            okButton.Text = OKButtonText;
            okButton.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Button));
            okButton.FontAttributes = FontAttributes.Bold;
            okButton.VerticalOptions = LayoutOptions.Center;
            okButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
            okButton.BackgroundColor = Color.White;
            okButton.TextColor = Color.FromHex("4384b8");
            okButton.Command = OnOKButtonClickedCommand;

            footerGrid.Children.Add(okButton, 1, 0);

            Button cancelButton = new Button();
            cancelButton.Text = CancelButtonText;
            cancelButton.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Button));
            cancelButton.FontAttributes = FontAttributes.Bold;
            cancelButton.VerticalOptions = LayoutOptions.Center;
            cancelButton.HorizontalOptions = LayoutOptions.CenterAndExpand;
            cancelButton.Command = OnCancelButtonClickedCommand;
            cancelButton.BackgroundColor = Color.White;
            cancelButton.TextColor = Color.FromHex("4384b8");

            footerGrid.Children.Add(cancelButton, 0, 0);

            FooterView = footerGrid;
        }

        internal int DatePartIndex(string partFormatString)
        {
            string shortDateTime = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            if (shortDateTime.ToLower().StartsWith(partFormatString))
            {
                return 0;
            }
            else if (shortDateTime.ToLower().EndsWith(partFormatString))
            {
                return 2;
            }

            return 1;
        }

        private void CustomDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDays(Date, e);
        }

        private void OnOKButtonClicked()
        {
            if(_latestSelectionChangedEventArgs != null)
            {
                UpdateOtherObservable(_latestSelectionChangedEventArgs);
            }
            IsOpen = false;
            PopupAction = PopupStatusAction.UpdateAndClose;
        }

        private void OnCancelButtonClicked()
        {
            IsOpen = false;
            PopupAction = PopupStatusAction.Close;
        }

        //Updatedays method is used to alter the Date collection as per selection change in Month column(if feb is Selected day collection has value from 1 to 28)
        public void UpdateDays(ObservableCollection<object> Date, SelectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _latestSelectionChangedEventArgs = e;
                if (Date.Count >= 3)
                {
                    bool isupdate = false;

                    if (e.OldValue != null && e.NewValue != null && (e.OldValue is ObservableCollection<object>) && (e.OldValue as ObservableCollection<object>).Count > 0)
                    {
                        if (MonthIndex >= (e.NewValue as IList).Count
                        || YearIndex >= (e.NewValue as IList).Count
                        || MonthIndex >= (e.OldValue as IList).Count
                        || YearIndex >= (e.OldValue as IList).Count)
                        {
                            return;
                        }

                        if (!object.Equals((e.OldValue as IList)[MonthIndex], (e.NewValue as IList)[MonthIndex]))
                        {
                            isupdate = true;
                        }

                        if (!object.Equals((e.OldValue as IList)[YearIndex], (e.NewValue as IList)[YearIndex]))
                        {
                            isupdate = true;
                        }
                    }

                    if (isupdate)
                    {
                        ObservableCollection<object> days = new ObservableCollection<object>();
                        int month = GetMonth((e.NewValue as IList)[MonthIndex].ToString());
                        int year = int.Parse((e.NewValue as IList)[YearIndex].ToString());
                        for (int j = 1; j <= DateTime.DaysInMonth(year, month); j++)
                        {
                            if (j < 10)
                            {
                                days.Add("0" + j);
                            }
                            else
                            {
                                days.Add(j.ToString());
                            }
                        }

                        ObservableCollection<object> oldvalue = new ObservableCollection<object>();

                        foreach (var item in e.NewValue as IList)
                        {
                            oldvalue.Add(item);
                        }

                        if (days.Count > 0)
                        {
                            
                            Date.RemoveAt(DayIndex);
                            Date.Insert(DayIndex, days);
                        }

                        if ((Date[DayIndex] as IList).Contains(oldvalue[DayIndex]))
                        {
                            SelectedItem = oldvalue;
                        }
                        else
                        {
                            oldvalue[DayIndex] = (Date[DayIndex] as IList)[(Date[DayIndex] as IList).Count - 1];
                            SelectedItem = oldvalue;
                        }
                    }
                }
            });
        }


        public void SetSelectedDate(DateTime selectedDate)
        {
            ObservableCollection<object> selectedDateCollection = new ObservableCollection<object>();

            if (HasDate)
            {
                SelectedDateAddAtIndex(selectedDate, selectedDateCollection, 0);
                SelectedDateAddAtIndex(selectedDate, selectedDateCollection, 1);
                SelectedDateAddAtIndex(selectedDate, selectedDateCollection, 2);
            }

            if (HasTime)
            {
                selectedDateCollection.Add(ZeroPadding(selectedDate.Hour));
                selectedDateCollection.Add(ZeroPadding(selectedDate.Minute));
            }

            SelectedItem = selectedDateCollection;
        }

        private void SelectedDateAddAtIndex(DateTime selectedDate, ObservableCollection<object> selectedDateCollection, int index)
        {
            if (DayIndex == index)
            {
                selectedDateCollection.Insert(DayIndex, ZeroPadding(selectedDate.Date.Day));
            }
            else if (YearIndex == index)
            {
                selectedDateCollection.Insert(YearIndex, selectedDate.Date.Year.ToString());
            }
            else
            {
                selectedDateCollection.Insert(MonthIndex, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(selectedDate.Date.Month));
            }
        }

        private string ZeroPadding(int value)
        {
            if(value < 10)
            {
                return "0" + value.ToString();
            }
            return value.ToString();
        }

        private void UpdateOtherObservable(SelectionChangedEventArgs e)
        {
            if (Date.Count == 2)
            {
                string hour = (e.NewValue as IList)[0].ToString();
                string minute = (e.NewValue as IList)[1].ToString();

                SelectedDataTimeString = hour + ":" + minute;
            }
            else if (Date.Count >= 3)
            {
                int year = int.Parse((e.NewValue as IList)[YearIndex].ToString());
                int month = GetMonth((e.NewValue as IList)[MonthIndex].ToString());
                int day = int.Parse((e.NewValue as IList)[DayIndex].ToString());
                int hour = 0;
                int minute = 0;

                if (Date.Count == 5)
                {
                    hour = int.Parse((e.NewValue as IList)[3].ToString());
                    minute = int.Parse((e.NewValue as IList)[4].ToString());
                }

                DateTime dateTime = new DateTime(year, month, day, hour, minute, 0);
                SelectedDataTime = dateTime;
                if (Date.Count > 3)
                {
                    SelectedDataTimeString = dateTime.ToString(_stringDateTimeFormat, DateTimeFormatInfo.InvariantInfo);
                }
                else
                {
                    SelectedDataTimeString = dateTime.ToString(_stringDateFormat, DateTimeFormatInfo.InvariantInfo);
                }
            }
        }

        private void PopulateDateCollection()
        {
            if (HasDate)
            {
                //populate Days
                for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                {
                    if (i < 10)
                    {
                        Day.Add("0" + i);
                    }
                    else
                    {
                        Day.Add(i.ToString());
                    }
                }

                //populate year
                for (int i = _startDate; i < _endDate; i++)
                {
                    Year.Add(i.ToString());
                }

                //populate months
                for (int i = 1; i < 13; i++)
                {
                    Month.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
                }
                
                DateAddAtIndex(0);
                DateAddAtIndex(1);
                DateAddAtIndex(2);
            }

            if (HasTime)
            {
                for (int i = 0; i <= 23; i++)
                {
                    if (i < 10)
                    {
                        Hour.Add("0" + i.ToString());
                    }
                    else
                    {
                        Hour.Add(i.ToString());
                    }
                }
                for (int j = 0; j < 60; j++)
                {
                    if (j < 10)
                    {
                        Minute.Add("0" + j);
                    }
                    else
                    {
                        Minute.Add(j.ToString());
                    }
                }

                Date.Add(Hour);
                Date.Add(Minute);
            }
        }

        private void DateAddAtIndex(int index)
        {
            if (DayIndex == index)
            {
                Date.Insert(DayIndex, Day);
            }
            else if (YearIndex == index)
            {
                Date.Insert(YearIndex, Year);
            }
            else
            {
                Date.Insert(MonthIndex, Month);
            }
        }

        private void HeaderAddAtIndex(int index)
        {
            if (DayIndex == index)
            {
                Headers.Insert(DayIndex, _localizationController.GetString(LocalizationKeys.TextGroupDate, LocalizationKeys.KeyDateDay));
            }
            else if (YearIndex == index)
            {
                Headers.Insert(YearIndex, _localizationController.GetString(LocalizationKeys.TextGroupDate, LocalizationKeys.KeyDateYear));
            }
            else
            {
                Headers.Insert(MonthIndex, _localizationController.GetString(LocalizationKeys.TextGroupDate, LocalizationKeys.KeyDateMonth));
            }
        }

        private int GetMonth(string monthString)
        {
            for (int i = 0; i < Month.Count; i++)
            {
                if (monthString.Equals(Month[i]))
                {
                    return i + 1;
                }
            }

            return 1;
        }
    }
}
