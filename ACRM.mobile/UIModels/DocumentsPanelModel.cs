using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class DocumentsPanelModel : UIPanelWidget
    {
        public ICommand DocumentDownloadCommand => new Command<DocumentObject>(async (selectedDoc) => await DownloadSelectedDoc(selectedDoc));

        private async Task DownloadSelectedDoc(DocumentObject selectedDoc)
        {
            await DocumentDownload.Download(selectedDoc, _contentService, _sessionContext, _cancellationTokenSource.Token);
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
        private bool _hasResults = true;
        public bool HasResults
        {
            get => _hasResults;
            set
            {
                _hasResults = value;
                RaisePropertyChanged(() => HasResults);
            }
        }

        private readonly IDocumentService _contentService;

        private ObservableCollection<DocumentObject> _documents;
        public ObservableCollection<DocumentObject> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                RaisePropertyChanged(() => Documents);
            }
        }
        public DocumentsPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<IDocumentService>();
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }

            IsLoading = true;
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (Data != null)
            {
                Title = Data.Label.ToUpperInvariant();
                Documents = await _contentService.PreparePanelDataAsync(Data, _cancellationTokenSource.Token);
                if (Documents.Count == 0)
                {
                    HasResults = false;
                }
                IsLoading = false;
            }
            return true;
        }
    }
}
