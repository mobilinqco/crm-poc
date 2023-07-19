using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SignaturePageViewModel : BaseViewModel
    {
        private ISignatureCaptureInterface _signatureInterface;
        private string _title = "Signature";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private string _signatureConfirmText = "Confirm";
        public string SignatureConfirmText
        {
            get => _signatureConfirmText;
            set
            {
                _signatureConfirmText = value;
                RaisePropertyChanged(() => SignatureConfirmText);
            }
        }
        private string _closeText = "Close";
        public string CloseText
        {
            get => _closeText;
            set
            {
                _closeText = value;
                RaisePropertyChanged(() => CloseText);
            }
        }
        
        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged(() => Message);
            }
        }
        public Func<Task<byte[]>> SignatureFromStream { get; set; }
        public byte[] Signature { get; set; }
        protected IConfigurationService _configurationService;
        public ICommand OnCloseButtonTapped => new Command(async () =>
        {
            await _navigationController.PopAllPopupAsync(null);
        });

        public ICommand SaveSignature => new Command(async () =>
        {
            Signature = await SignatureFromStream();
            if (!Directory.Exists(_sessionContext.ReportFolder()))
            {
                Directory.CreateDirectory(_sessionContext.ReportFolder());
            }
            var filePath = _sessionContext.ResourcePath("signature01.png");
            using (FileStream file = new FileStream(filePath, FileMode.Create))
            {
                file.Write(Signature, 0, Signature.Length);
            }
            await _navigationController.PopAllPopupAsync(null);

            await _signatureInterface?.GenerateReport("signature01.png");
            // Signature should be != null
        });

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is ISignatureCaptureInterface signatureInterface)
            {
                _signatureInterface = signatureInterface;
            }
            await base.InitializeAsync(navigationData);
        }

        public SignaturePageViewModel()
        {
            Title = _localizationController.GetString(LocalizationKeys.TextGroupProcesses,
                           LocalizationKeys.KeyProcessesSignatureTitle);
            SignatureConfirmText = _localizationController.GetString(LocalizationKeys.TextGroupProcesses,
                           LocalizationKeys.KeyProcessesSignatureConfirmButtonTitle);
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);
        }
    }
}
