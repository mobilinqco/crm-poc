using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using ACRM.mobile.ViewModels.Base;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class ClientReportViewModel : UIWidget, ISignatureCaptureInterface, IPopupItemSelectionHandler
    {
        public ICommand ReportActionCommand => new Command(async () => await ReportActionCommandHandler());
        public ICommand EmailButtonCommand => new Command(async () => await EmailButtonCommandHandler());
        
        private readonly ResetTimer timer;
        private readonly IReportingService _contentService;
        private readonly IDocHelperService _docHelperService;
        private readonly IRightsProcessor _rightsProcessor;
        private readonly IDocumentService _docServic;

        private readonly UserAction _userAction;
        public string DocFilePath { get; set; }
        public string HTMLContent { get; set; }
        public string PrintIconText { get; set; } = MaterialDesignIcons.Printer;
        public string EmailIconText { get; set; } = MaterialDesignIcons.Email;
        public string TickIconText { get; set; } = MaterialDesignIcons.Check;

        private string _actionText;
        public string ActionText
        {
            get => _actionText;
            set
            {
                _actionText = value;
                RaisePropertyChanged(() => ActionText);
            }
        }

        private bool _actionButtonVisablity = false;
        public bool ActionButtonVisablity
        {
            get => _actionButtonVisablity;
            set
            {
                _actionButtonVisablity = value;
                RaisePropertyChanged(() => ActionButtonVisablity);
                RaisePropertyChanged(() => ReportHeaderVisablity);
            }
        }

        public bool ReportHeaderVisablity
        {
            get => _actionButtonVisablity || _sendByEmail;
        }

        private bool _sendByEmail = false;
        public bool SendByEmail
        {
            get => _sendByEmail;
            set
            {
                _sendByEmail = value;
                RaisePropertyChanged(() => SendByEmail);
                RaisePropertyChanged(() => ReportHeaderVisablity);
            }
        }

        private UIWidget _content;
        public UIWidget Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }

        public ClientReportViewModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<IReportingService>();
            _docHelperService = AppContainer.Resolve<IDocHelperService>();
            _rightsProcessor = AppContainer.Resolve<IRightsProcessor>();
            _docServic = AppContainer.Resolve<IDocumentService>();
            ActionButtonVisablity = false;
            if (widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
                _contentService.SetSourceAction(_userAction);
                _docServic.SetSourceAction(_userAction);
            }
            this.timer = new ResetTimer(this.LoadWebControl);

        }

        private async Task ReportActionCommandHandler()
        {
            if (_contentService.GetReportConfig("sign").Equals("true",StringComparison.InvariantCultureIgnoreCase))
            {
                await _navigationController.NavigateToAsync<SignaturePageViewModel>(parameter: this);
            }
            else
            {
                await GenerateReport(string.Empty);
            }

        }

        private async Task EmailButtonCommandHandler()
        {

            var emailButtons = _contentService.SendByEmailActionButtons;
            if (emailButtons.Count > 1)
            {
                await _navigationController.NavigateToAsync<PopupListPageViewModel>(parameter: this);
            }
            else if (emailButtons.Count == 1)
            {
                await SendMail(emailButtons[0]);

            }

        }

        private async Task SendMail(Domain.Configuration.UserInterface.Menu menu)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(DocFilePath) || !string.IsNullOrWhiteSpace(HTMLContent))
                {
                    var email = await _contentService.GetEmailContentAsync(menu, _cancellationTokenSource.Token);
                    if (email != null)
                    {

                        var message = new EmailMessage
                        {
                            Subject = email.Subject,
                            Body = email.Body
                        };

                        if(!string.IsNullOrWhiteSpace(email.To))
                        {
                            message.To.Add(email.To);
                        }

                        if (!string.IsNullOrWhiteSpace(email.Cc))
                        {
                            message.Cc.Add(email.Cc);
                        }

                        if (!string.IsNullOrWhiteSpace(email.Bcc))
                        {
                            message.Bcc.Add(email.Bcc);
                        }

                        if (string.IsNullOrWhiteSpace(email.Body) && !string.IsNullOrWhiteSpace(HTMLContent))
                        {
                            message.Body = HTMLContent;
                            message.BodyFormat = EmailBodyFormat.Html;
                        }
                        if (!string.IsNullOrWhiteSpace(DocFilePath) && _contentService.SendByEmailAttachReport)
                        {
                            message.Attachments.Add(new EmailAttachment(DocFilePath));
                        }
                        await Email.ComposeAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task setLoadingHTML()
        {
            var loadingText = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicMessageLoadingReport);
            var h1Style = "\"text-align:center;\"";
            var htmlContent =
                   @$"<!DOCTYPE html>
   <html>
  <head>
  </head>
  <body>
  <div>
    <h1 style={h1Style} >{loadingText}</h1>
    </div>
  </body >
  </html>";
            var WebContent = new WebContentParams
            {
                BaseUrl = _sessionContext.ResourcesFolder(),
                HTMLContent = htmlContent
            };
            Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
        }

        public async override ValueTask<bool> InitializeControl()
        {
            var (status, result, message) = await _rightsProcessor.EvaluateRightsFilter(_userAction, _cancellationTokenSource.Token);
            if (status)
            {
                if (!result)
                {
                    var title = $"RightsFilter access denied, message : {message}";
                    _logService.LogInfo(title);
                    var htmlContent =
                        @$"<html>
  <head>
    <title>{title}</title>
  </head>
  <body>
    <h1>{message}</h1>
  </body >
  </html>";
                    var WebContent = new WebContentParams
                    {
                        BaseUrl = _sessionContext.ResourcesFolder(),
                        HTMLContent = htmlContent
                    };
                    Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
                    return false;

                }
            }

            await setLoadingHTML();
            await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
            ActionText = _contentService.ActionButton?.Label;
            SendByEmail = _contentService.SendByEmail;
            //ActionButtonVisablity = string.IsNullOrWhiteSpace(ActionText) ? false : true;
            await PerformWebLoad();
            return true;
        }

        private async Task PerformWebLoad(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    delay = 5;
                }
                timer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        private async Task LoadWebControl()
        {
            try
            {
                DocFilePath = await _contentService.GetReportDoc(_cancellationTokenSource.Token);
                if (string.IsNullOrWhiteSpace(DocFilePath))
                {
                    HTMLContent = await _contentService.GetHTMLReport(_cancellationTokenSource.Token);
                    var WebContent = new WebContentParams
                    {
                        BaseUrl = _sessionContext.ResourcesFolder(),
                        HTMLContent = HTMLContent
                    };
                    Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
                }
                else
                {

                    var WebContent = new WebContentParams
                    {
                        BaseUrl = DocFilePath,
                        IsURLSource = true
                    };
                    Content = await FormBuilderExtensions.BuildWidget("PDFViewer", WebContent, this, _cancellationTokenSource);

                }
                await SetActionButton();

            }
            catch (Exception ex)
            {
                _logService.LogError($"Unable to Load WebControl content {ex.Message}");
                var htmlContent =
                    @$"<html>
  <head>
    <title>{ex.Message}</title>
  </head>
  <body>
    <h1>{ex.Message}</h1>
  </body >
  </html>";
                var WebContent = new WebContentParams
                {
                    BaseUrl = _sessionContext.ResourcesFolder(),
                    HTMLContent = htmlContent
                };
                Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
            }
        }

        private async Task SetActionButton()
        {

            if (_contentService?.ActionButton !=null)
            {

                var (status, result, message) = await _rightsProcessor.EvaluateRightsFilter(_userAction, _cancellationTokenSource.Token, false, "ButtonShowFilter");
                if (status)
                {
                    if (result)
                    {
                        ActionButtonVisablity = false;
                        var title = $"Button Action will not be displayed, message : {message}";
                        _logService.LogInfo(title);

                    }
                    else
                    {
                        ActionButtonVisablity = true;
                    }
                }
            }
        }

        public async Task GenerateReport(string signaturePath)
        {
            if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android)
            {
                ActionButtonVisablity = false;
                await setLoadingHTML();
                if (!string.IsNullOrWhiteSpace(signaturePath))
                {
                    _contentService.SignatureFile = _sessionContext.ResourcePath(signaturePath);
                }
                HTMLContent = await _contentService.GetHTMLReport(_cancellationTokenSource.Token);
                DocFilePath = _sessionContext.ReportPath($"temp_report.pdf");
                var baseUrl = _sessionContext.ResourcesFolder();
                _docHelperService.ConvertToPDF(HTMLContent, DocFilePath, baseUrl, SavePDFdoc);
                var WebContent = new WebContentParams
                {
                    BaseUrl = _sessionContext.ResourcesFolder(),
                    HTMLContent = HTMLContent
                };

                Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);

            }

        }

        public async Task SavePDFdoc(string docfilePath)
        {
            var WebContent = new WebContentParams
            {
                BaseUrl = docfilePath,
                IsURLSource = true
            };
            Content = await FormBuilderExtensions.BuildWidget("PDFViewer", WebContent, this, _cancellationTokenSource);
            var doUpload = _contentService.GetReportConfig("upload");
            if (doUpload.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                var filName = _contentService.GetReportConfig("signedReportFileName");
                filName = _contentService.ProcessToken(filName, _contentService.SourceFieldGroupData);
                var documentObject = new DocumentObject
                {
                    LocalPath = docfilePath,
                    MimeType = "application/pdf",
                    FileName = filName
                };
                var docFieldId = _contentService.DocFieldId;
                bool result = false;
                if (docFieldId > 0)
                {
                    documentObject.FieldId = docFieldId;
                }

                using (documentObject.FileStream = new FileStream(docfilePath, FileMode.Open, FileAccess.Read))
                {
                    result = await _docServic.UploadDocument(documentObject, null, _cancellationTokenSource.Token);
                }
            }
            await _contentService.ExecuteReportAction(_cancellationTokenSource.Token);

        }

        async Task<List<PopupListItem>> IPopupItemSelectionHandler.GetPoupList()
        {
            var emailButtons = _contentService.SendByEmailActionButtons;
            if (emailButtons != null && emailButtons.Count > 0)
            {
                var items = new List<PopupListItem>();
                foreach (var menu in emailButtons)
                {
                    items.Add(new PopupListItem
                    {
                        RecordId = menu.DisplayName,
                        DisplayText = menu.DisplayName,
                        OrginalObject = menu
                    }); ;
                }
                return items;
            }
           
            return null;
        }

        async Task IPopupItemSelectionHandler.PopupItemSelected(PopupListItem item)
        {
            if (item != null && item.OrginalObject is Domain.Configuration.UserInterface.Menu menu)
            {
                await SendMail(menu);
            }
        }
    }
}
