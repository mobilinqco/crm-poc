using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;

namespace ACRM.mobile.UIModels
{
    public class QueryViewModel : UIWidget
    {
        private readonly IQueryService _queryService;

        private readonly UserAction _userAction;

        private string _title = "Query Title";
        public string Title
        {
            get => _title;

            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private DataTable _queryData;
        public DataTable QueryData
        {
            get => _queryData;

            set
            {
                _queryData = value;
                RaisePropertyChanged(() => QueryData);
            }
        }

        private bool _enableNoResultsText;
        public bool EnableNoResultsText
        {
            get => _enableNoResultsText;
            set
            {
                _enableNoResultsText = value;
                RaisePropertyChanged(() => EnableNoResultsText);
            }
        }

        private string _noResultsText;
        public string NoResultsText
        {
            get => _noResultsText;
            set
            {
                _noResultsText = value;
                RaisePropertyChanged(() => NoResultsText);
            }
        }

        private bool _hasData = false;
        public bool HasData
        {
            get => _hasData;
            set
            {
                _hasData = value;
                RaisePropertyChanged(() => HasData);
            }
        }

        public QueryViewModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _queryService = AppContainer.Resolve<IQueryService>();

            if (widgetArgs != null)
            {
                if (widgetArgs is UserAction)
                {
                    _userAction = widgetArgs as UserAction;
                    _queryService.SetSourceAction(_userAction);
                }
                else if (widgetArgs.GetType() == typeof(FormItem))
                {
                    FormItem formItem = (FormItem)widgetArgs;

                    Dictionary<string, object> args = formItem.ItemAttributesDictionary();
                    UserAction ua = new UserAction { ActionType = UserActionType.Query,
                        ActionDisplayName = formItem.Label,
                        ActionUnitName = formItem.Func };

                    //if (args != null && args.ContainsKey("Scale"))
                    //{
                    //    double scale = 1.0;
                    //    if (args["Scale"].GetType() == typeof(double))
                    //    {
                    //        scale = (double)args["Scale"];
                    //    }

                    //    HeightRequest = Convert.ToInt32(Convert.ToDouble(HeightRequest) * scale);
                    //    WidthRequest = Convert.ToInt32(Convert.ToDouble(WidthRequest) * scale);
                    //}

                    args = formItem.OptionsDictionary();
                    //if (args != null && args.ContainsKey("ChangeDayOnMonthChange"))
                    //{
                    //    if (args["ChangeDayOnMonthChange"].GetType() == typeof(bool))
                    //    {
                    //        _changeDayOnMonthChange = (bool)args["ChangeDayOnMonthChange"];
                    //    }
                    //}
                    _userAction = ua;
                    _queryService.SetSourceAction(ua);
                }
            }
            IsLoading = true;
            EnableNoResultsText = false;
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (_queryService != null)
            {
                IsLoading = true;
                _logService.LogDebug("QueryView Start InitializeAsync");
                if (_userAction != null)
                {
                    _queryService.SetSourceAction(_userAction);
                    _queryService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                    {
                        _logService.LogError($"Unable to prepare content {ex.Message}");
                    });
                }
                await UpdateBindingsAsync();
                _logService.LogDebug("End  InitializeAsync");
            }
            return true;
        }

        private async Task UpdateBindingsAsync()
        {
            _logService.LogDebug("Start UpdateBindingsAsync");
            IsLoading = false;
            // Prepare the page data
            Title = _queryService.PageTitle();
            DataTable dataTable = _queryService.GetData();
            if(dataTable != null)
            {
                HasData = true;
                _queryData = dataTable;
            }
            else
            {
                EnableNoResultsText = true;
                NoResultsText = "Query Returned No Results";
            }
            
            
            _logService.LogDebug("End UpdateBindingsAsync");
        }
    }
}

