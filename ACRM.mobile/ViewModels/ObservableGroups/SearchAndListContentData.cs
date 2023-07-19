using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using Syncfusion.DataSource;
using Xamarin.CommunityToolkit.ObjectModel;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class SearchAndListContentData: Base.ExtendedBindableObject
    {
        public bool EnableNoResultsText { get; set; } = false;
        public bool IsSearchBarVisible { get; set; } = true;
        public bool HasActiveFilters { get; set; } = false;
        public bool IsFilteringEnabled { get; set; } = false;
        public bool IsFilterVisible { get; set; } = true;
        public bool IsOnlinePossible { get; set; } = false;
        public bool IsOnlineOfflineVisible { get; set; } = true;
        public long ResultsCount { get; set; } = 0;
        public bool IsCounterVisible { get; set; } = true;
        public bool IsUserFilterEnabled { get; set; } = false;
        public int UserFilterCount { get; set; } = 0;
        public bool AreSectionsEnabled { get; set; } = false;

        public int InitialPageSize { get; set; } = 20;
        public int PageSize { get; set; } = 20;

        private bool _onlineMode = false;
        
        public bool OnlineMode {
            get => _onlineMode;
            set
            {
                _onlineMode = value;
                if (value)
                {
                    OnlineButtonIconText = MaterialDesignIcons.Cloud;
                }
                else
                {
                    OnlineButtonIconText = MaterialDesignIcons.CloudOutline;
                }
                
            }
        }

        public string OnlineButtonIconText { get; set; } = MaterialDesignIcons.CloudOutline;

        public string SearchText { get; set; }
        public string SearchTextBoxPlaceholderText { get; set; }

        private ObservableRangeCollection<ListDisplayRow> _searchResults = new ObservableRangeCollection<ListDisplayRow>();
        public ObservableRangeCollection<ListDisplayRow> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                _searchResults = value;
            }
        }

        public string NoResultsText { get; set; }

        private DataSource _groupingDataSource = new DataSource();
        public DataSource GroupingDataSource
        {
            get => _groupingDataSource;
            set
            {
                _groupingDataSource = value;
                RaisePropertyChanged(() => GroupingDataSource);
            }
        }

        public SearchAndListContentData()
        {
            GroupingDataSource.Source = SearchResults;
            GroupingDataSource.GroupDescriptors.Add(
                new GroupDescriptor()
                {
                    PropertyName = "SectionGroupingValue",
                    KeySelector = (object obj1) =>
                    {
                        var item = (obj1 as ListDisplayRow);
                        ListDisplayField ldf = item.SectionGroupingValue;

                        if (ldf != null
                            && !string.IsNullOrWhiteSpace(ldf.Data.StringData))
                        {
                            if(ldf.Config.PresentationFieldAttributes.IsDate || ldf.Config.PresentationFieldAttributes.IsTime)
                            {
                                return Utils.Formatters.DateTimeFormatter.FormatedDateFromDbString(ldf.Data.StringData, ldf.Config.PresentationFieldAttributes); 
                            }
                            return item.SectionGroupingValue.Data.StringData[0].ToString().ToUpper();
                        }

                        return null;
                    }
                });
        }
    }
}
