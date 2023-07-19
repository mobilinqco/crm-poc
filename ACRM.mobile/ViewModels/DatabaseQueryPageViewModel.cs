using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
using ACRM.mobile.CustomControls;
using Syncfusion.SfDataGrid.XForms.Exporting;
using System.IO;
using Syncfusion.Pdf;

namespace ACRM.mobile.ViewModels
{
    public class DatabaseQueryPageViewModel: BaseViewModel
    {
        private readonly ICrmDataUnitOfWork _crmDataUnitOfWork;
        private readonly IConfigurationService _configurationService;
        private readonly IOfflineRequestsUnitOfWork _offlineRequestsUnitOfWork;

        public ICommand CloseCommand => new Command(async () => await Close());
        public ICommand ExecuteCommand => new Command(async () => await ExecuteRawQueryString());
        public ICommand LoadMoreCommand => new Command(() => OnLoadMore());
        public ICommand CopyCommand => new Command(async () => await Copy());
        public ICommand ExportCommand => new Command<ExportDataGridEventArgs>(async (args) => await Export(args));

        private WidgetEventType _widgetEventType = WidgetEventType.ShowCrmDBRequested;

        private List<DynamicStringModel> _queryResultModels = new List<DynamicStringModel>();

        private static int pageNumber = 100;

        private string _closeText;
        public string CloseText
        {
            get => _closeText;
            set
            {
                _closeText = value;
                RaisePropertyChanged(() => CloseText);
            }
        }

        private string _rawSQLText;
        public string RawSQLText
        {
            get => _rawSQLText;
            set
            {
                _rawSQLText = value;
                RaisePropertyChanged(() => RawSQLText);
            }
        }

        private bool _isDataGridBusy;
        public bool IsDataGridBusy
        {
            get => _isDataGridBusy;
            set
            {
                _isDataGridBusy = value;
                RaisePropertyChanged(() => IsDataGridBusy);
            }
        }

        private ObservableCollection<DynamicStringModel> _loadedQueryResultModels = new ObservableCollection<DynamicStringModel>();
        public ObservableCollection<DynamicStringModel> LoadedQueryResultModels
        {
            get => _loadedQueryResultModels;
            set
            {
                _loadedQueryResultModels = value;
                RaisePropertyChanged(() => LoadedQueryResultModels);
            }
        }

        public DatabaseQueryPageViewModel(ICrmDataUnitOfWork crmDataUnitOfWork,
            IConfigurationService configurationService,
            IOfflineRequestsUnitOfWork offlineRequestsUnitOfWork)
        {
            _crmDataUnitOfWork = crmDataUnitOfWork;
            _configurationService = configurationService;
            _offlineRequestsUnitOfWork = offlineRequestsUnitOfWork;
            InitProperties();
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if(navigationData is WidgetEventType type)
            {
                _widgetEventType = type;
            }

            await base.InitializeAsync(navigationData);
        }

        private async Task ExecuteRawQueryString()
        {
            if (IsRawQueryStringValid())
            {
                try
                {
                    ResetQueryResultModels();
                    switch(_widgetEventType)
                    {
                        case WidgetEventType.ShowCrmDBRequested:
                            await GetCrmRawQueryStringResults();
                            break;
                        case WidgetEventType.ShowConfigDBRequested:
                            await GetConfigurationRawQueryStringResults();
                            break;
                        case WidgetEventType.ShowOfflineDBRequested:
                            await GetOfflineRawQueryStringResults();
                            break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        // TODO Only "select" queries should be valid?
        private bool IsRawQueryStringValid()
        {
            return true;
        }

        private void ResetQueryResultModels()
        {
            _loadedQueryResultModels = new ObservableCollection<DynamicStringModel>();
            LoadedQueryResultModels = _loadedQueryResultModels;
        }

        private async Task GetCrmRawQueryStringResults()
        {
            _queryResultModels.Clear();
            _queryResultModels.AddRange(await _crmDataUnitOfWork.ExecuteRawQueryString(_rawSQLText, _cancellationTokenSource.Token));
            OnLoadMore(true);
        }

        private async Task GetConfigurationRawQueryStringResults()
        {
            _queryResultModels.Clear();
            _queryResultModels.AddRange(await _configurationService.ExecuteRawQueryString(_rawSQLText, _cancellationTokenSource.Token));
            OnLoadMore(true);
        }

        private async Task GetOfflineRawQueryStringResults()
        {
            _queryResultModels.Clear();
            _queryResultModels.AddRange(await _offlineRequestsUnitOfWork.ExecuteRawQueryString(_rawSQLText, _cancellationTokenSource.Token));
            OnLoadMore(true);
        }

        private void OnLoadMore(bool isDataNew = false)
        {
            int tempPageNumber = (_queryResultModels.Count > LoadedQueryResultModels.Count + pageNumber) ?
                (LoadedQueryResultModels.Count + pageNumber) : _queryResultModels.Count;

            IsDataGridBusy = true;
            if (isDataNew)
            {
                _loadedQueryResultModels = new ObservableCollection<DynamicStringModel>();
                for (int i = LoadedQueryResultModels.Count; i < tempPageNumber; i++)
                {
                    _loadedQueryResultModels.Add(_queryResultModels[i]);
                }
                LoadedQueryResultModels = _loadedQueryResultModels;
            }
            else
            {
                for (int i = LoadedQueryResultModels.Count; i < tempPageNumber; i++)
                {
                    LoadedQueryResultModels.Add(_queryResultModels[i]);
                }
            }
            IsDataGridBusy = false;
        }

        private string GetQueryResultsAsString()
        {
            string queryResultString = $"SQL: {_rawSQLText}\n\n";
            foreach (DynamicStringModel resultModel in LoadedQueryResultModels)
            {
                queryResultString += resultModel.ToString() + "\n";
            }
            return queryResultString;
        }

        private async Task Copy()
        {
            await Clipboard.SetTextAsync(GetQueryResultsAsString());
        }

        private async Task Export(ExportDataGridEventArgs exportDataGridEventArgs)
        {
            try
            {
                DataGridPdfExportingController pdfExport = new DataGridPdfExportingController();
                MemoryStream stream = new MemoryStream();

                PdfDocument exportToPdf = pdfExport.ExportToPdf(exportDataGridEventArgs.DataGrid, new DataGridPdfExportOption());
                exportToPdf.Save(stream);
                exportToPdf.Close(true);

                string filename = "CRM.ClientQueryResults.pdf";
                string pdfDocumentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);

                using (FileStream file = new FileStream(pdfDocumentPath, FileMode.Create, FileAccess.Write))
                {
                    stream.WriteTo(file);
                    stream.Close();
                }

                await Send(pdfDocumentPath);
            }
            catch(Exception ex)
            {

            }
        }

        private async Task Send(string pdfDocumentPath)
        {
            var message = new EmailMessage
            {
                Subject = $"CRM.Client Query Result for {_sessionContext.CrmInstance.Name}",
                Body = $"SQL: {_rawSQLText}\n\n"
            };
            message.Attachments.Add(new EmailAttachment(pdfDocumentPath));
            await Email.ComposeAsync(message);
        }

        private async Task Close()
        {
            _cancellationTokenSource.Cancel();
            await _navigationController.PopPopupAsync();
        }
    }
}
