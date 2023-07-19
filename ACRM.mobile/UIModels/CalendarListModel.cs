using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class CalendarListModel : UIWidget
    {
        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async (evt) => await RecordSelected(evt));

        private SearchAndListContentData _searchAndListContentData = new SearchAndListContentData();
        public SearchAndListContentData SearchAndListContentData
        {
            get => _searchAndListContentData;

            set
            {
                _searchAndListContentData = value;
                RaisePropertyChanged(() => SearchAndListContentData);
            }
        }
        private DateTime _startDate = new DateTime();
        public DateTime StartDate
        {
            get => _startDate;

            set
            {
                _startDate = value;
                RaisePropertyChanged(() => DateRange);
            }
        }
        private DateTime _endDate = new DateTime();
        public DateTime EndDate
        {
            get => _endDate;

            set
            {
                _endDate = value;
                RaisePropertyChanged(() => DateRange);
            }
        }
        public string DateRange
        {
            get
            {
                return $"{_startDate:dd MMMM yyyy} - {_endDate:dd MMMM yyyy}";
            }
            set
            {
                RaisePropertyChanged(() => DateRange);
            }
        }

        public CalendarListModel(CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            SearchAndListContentData.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                LocalizationKeys.KeyErrorsNoResults);
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }

        public void UpdateData(List<ListDisplayRow> searchResults, DateTime startDate, DateTime endDate)
        {
            // interact with UI elements
            StartDate = startDate;
            EndDate = endDate;
            if (searchResults != null && searchResults.Count > 0)
            {
                _searchAndListContentData.EnableNoResultsText = false;
                _searchAndListContentData.SearchResults = new ObservableRangeCollection<ListDisplayRow>(searchResults);
                SearchAndListContentData = _searchAndListContentData;
            }
            else
            {
                SetNoResultUI();
            }
            IsLoading = false;
        }

        private void SetNoResultUI()
        {
            _searchAndListContentData.EnableNoResultsText = true;
            _searchAndListContentData.SearchResults.Clear();
            SearchAndListContentData = _searchAndListContentData;
        }

        public void DataIsLoading()
        {
            IsLoading = true;
            _searchAndListContentData.EnableNoResultsText = false;
            _searchAndListContentData.SearchResults.Clear();
            SearchAndListContentData = _searchAndListContentData;
        }

        private async Task RecordSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is ListDisplayRow selectedItem)
            {
                await ParentBaseModel?.PublishMessage(new WidgetMessage
                {
                    EventType = WidgetEventType.RecordSelected,
                    ControlKey = "CalendarList",
                    Data = selectedItem
                });
            }
        }
    }
}
