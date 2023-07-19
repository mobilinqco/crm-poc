using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class ChildRecordsModel : UIPanelWidget
    {
        protected int LineHeight = 100;
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

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
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

        private int _listViewHeightRequest = 100;
        public int ListViewHeightRequest
        {
            get => _listViewHeightRequest;
            set
            {
                _listViewHeightRequest = value;
                RaisePropertyChanged(() => ListViewHeightRequest);
            }
        }

        private List<ListDisplayRow> _records;
        public List<ListDisplayRow> Records
        {
            get => _records;
            set
            {
                _records = value;
                RaisePropertyChanged(() => Records);
            }
        }

        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await RecordSelected(evt));

        protected readonly ISearchContentService _contentService;

        public ChildRecordsModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<ISearchContentService>();
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }

            IsLoading = true;

        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (Data != null)
            {
                Title = Data.Label.ToUpperInvariant();
            }
            if (Data != null)
            {

                Records = await _contentService.PrepareClildRecordsAsync(Data, _cancellationTokenSource.Token);
                IsLoading = false;
                if (Records.Count == 0)
                {
                    HasData = false;
                    EnableNoResultsText = true;
                    NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                        LocalizationKeys.KeyErrorsNoResults);
                }
                else
                {
                    SetUIHeight(Records.Count);
                }

            }
            return true;
        }

        protected void SetUIHeight(int count)
        {
            if (count < 4)
            {
                ListViewHeightRequest = count * LineHeight;
            }
            else
            {
                ListViewHeightRequest = LineHeight * 3;
            }
        }

        private async Task RecordSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is ListDisplayRow selectedItem)
            {
                UserAction userAction = await _contentService.ActionForItemSelect(0, selectedItem, _cancellationTokenSource.Token);
                IUserActionSchuttle userActionSchuttle = AppContainer.Resolve<IUserActionSchuttle>();
                await userActionSchuttle.Carry(userAction, selectedItem.RecordId, _cancellationTokenSource.Token);
            }
        }

    }
}
