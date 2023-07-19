using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class DocumentUploadPageViewModel : BaseViewModel
    {
        protected IConfigurationService _configurationService;
        public ICommand OnCloseButtonTapped => new Command(async () =>
        {
            await _navigationController.PopAllPopupAsync(null);
        });

        public ListDisplayField DocField { get; set; }
        private IImageSelectorInterface _iImageSelectorInterface;
        private UserAction _userAction;
        private FileResult _fileResult;
        public FileResult FileResult
        {
            get => _fileResult;
            set
            {
                _fileResult = value;
                RaisePropertyChanged(() => FileResult);
                RaisePropertyChanged(() => FileSelected);
            }
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        private bool _fileSelected = false;
        public bool FileSelected
        {
            get => _fileSelected;
            set
            {
                _fileSelected = value;
                RaisePropertyChanged(() => FileSelected);
            }
        }

        private ImageSource _fileSource;
        public ImageSource FileSource
        {
            get => _fileSource;
            set
            {
                _fileSource = value;
                RaisePropertyChanged(() => FileSource);
                if (FileSelected && _iImageSelectorInterface != null )
                {
                    _iImageSelectorInterface.FileSelected(this);
                    _navigationController.PopAllPopupAsync(null);
                }
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

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged(() => Message);
                RaisePropertyChanged(() => MessageShown);
            }
        }

        private bool _isPhotoUploadOnly = false;
        public bool IsPhotoUploadOnly
        {
            get => _isPhotoUploadOnly;
            set
            {
                _isPhotoUploadOnly = value;
                RaisePropertyChanged(() => IsPhotoUploadOnly);
            }
        }

        public bool MessageShown
        {
            get => !string.IsNullOrEmpty(Message);
        }

        public ICommand FilePickCommand => new Command(async () => await FilePickCommandHandler());
        public ICommand ImagePickCommand => new Command(async () => await ImagePickCommandHandler());
        public ICommand FileUploadCommand => new Command(async () => await FileUploadCommandHandler());
        public ICommand CaptureImageCommand => new Command(async () => await CaptureImageCommandHandler());
        public ICommand VideoPickCommand => new Command(async () => await VideoPickCommandHandler());
        private readonly IDocumentService _docService;
        private readonly BackgroundSyncManager _backgroundSyncManager;
        private List<string> ParentCalogs = new List<string>();
        private long _maxUploadSize = 10000; // 10 MB - Default
        public DocumentObject documentObject = null;

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

        private string _pickAFileText;
        public string PickAFileText
        {
            get => _pickAFileText;
            set
            {
                _pickAFileText = value;
                RaisePropertyChanged(() => PickAFileText);
            }
        }

        private string _pickAnImageText;
        public string PickAnImageText
        {
            get => _pickAnImageText;
            set
            {
                _pickAnImageText = value;
                RaisePropertyChanged(() => PickAnImageText);
            }
        }

        private string _pickAVideoText;
        public string PickAVideoText
        {
            get => _pickAVideoText;
            set
            {
                _pickAVideoText = value;
                RaisePropertyChanged(() => PickAVideoText);
            }
        }


        private string _captureAnImageText;
        public string CaptureAnImageText
        {
            get => _captureAnImageText;
            set
            {
                _captureAnImageText = value;
                RaisePropertyChanged(() => CaptureAnImageText);
            }
        }

        private string _uploadText;
        public string UploadText
        {
            get => _uploadText;
            set
            {
                _uploadText = value;
                RaisePropertyChanged(() => UploadText);
            }
        }

        public DocumentUploadPageViewModel(IDocumentService docService,
            BackgroundSyncManager backgroundSyncManager,
            IConfigurationService configurationService)
        {
            IsLoading = true;
            _docService = docService;
            _backgroundSyncManager = backgroundSyncManager;
            _configurationService = configurationService;
            RegisterMessages();
            InitialiseProperties();
            SetConfigParams();
        }

        private void InitialiseProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            PickAFileText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicAddFile);
            PickAnImageText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicAddPhoto);
            PickAVideoText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicAddVideo);
            CaptureAnImageText = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesUploadPhotoTakePhoto);
            UploadText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicUpload);
        }

        private void SetConfigParams()
        {
            if (_configurationService.GetConfigValue("Document.MaxUploadSize") != null)
            {
                if (!long.TryParse(_configurationService.GetConfigValue("Document.MaxUploadSize").Value, out _maxUploadSize))
                {
                    _maxUploadSize = 10000;
                }
            }
        }

        private void RegisterMessages()
        {
            RegisterMessage(WidgetEventType.AddParentCatalog, null, AddParentCatalogEventHandler);
        }

        public async Task SetUserAction(UserAction ua)
        {
            _userAction = ua;
            _docService.SetSourceAction(ua);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            if (navigationData is UserAction ua)
            {
                _userAction = ua;
                Title = ua?.ActionDisplayName;
                _docService.SetSourceAction(ua);
                await _docService.PrepareContentAsync(_cancellationTokenSource.Token);
                await UpdateBindingsAsync();
            }
            else if (navigationData is IImageSelectorInterface imageSelectorInterface)
            {
                _iImageSelectorInterface = imageSelectorInterface;
                IsPhotoUploadOnly = true;
            }
            await base.InitializeAsync(navigationData);
            IsLoading = false;
        }

        private Task AddParentCatalogEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string catalogId && !ParentCalogs.Contains(catalogId))
            {
                ParentCalogs.Add(catalogId);
            }

            return Task.CompletedTask;
        }

        private async Task UpdateBindingsAsync()
        {
            var pnls = await _docService.PanelsAsync(_cancellationTokenSource.Token);
            if (pnls != null && pnls.Count > 0)
            {
                Widgets = await pnls.BuildWidgetsAsyc(this, _cancellationTokenSource);
            }

            // Intalize Parent Catalog fields

            foreach (var catlaogs in ParentCalogs)
            {
                await PublishMessage(new WidgetMessage
                {
                    ControlKey = catlaogs,
                    Data = null,
                    EventType = WidgetEventType.InitalizeParentCatalog
                }, MessageDirections.ToChildren);
            }

            // Intalize the selected items for child catalogs
            await PublishMessage(new WidgetMessage
            {
                ControlKey = null,
                EventType = WidgetEventType.InitalizeSelectedItem
            }, MessageDirections.ToChildren);

            if (_userAction.ViewReference.IsImageUploadAction())
            {
                IsPhotoUploadOnly = true;
            }

        }

        private async Task FilePickCommandHandler()
        {
            try
            {
                FileSource = null;
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf", "public.image", "public.jpeg", "public.png",
                        "public.txt", "public.pdf", "public.doc", "public.text", "public.mp3", "public.mpeg4",
                        "public.mpg", "public.wav", "public.audio", "public.video", "public.bmp", "public.gif",
                        "public.mov", "public.avi", "public.mpeg",
                        "com.microsoft.word.doc", "org.openxmlformats.wordprocessingml.document",
                        "com.microsoft.powerpoint.​ppt", "org.openxmlformats.spreadsheetml.sheet",
                        "org.openxmlformats.presentationml.presentation", "com.microsoft.excel.xls"} },
                    { DevicePlatform.Android, new[] { "application/pdf", "image/*", "*/*" } },
                    { DevicePlatform.UWP, new[] { ".pdf", ".jpg", ".png" } },
                    { DevicePlatform.Tizen, new[] { "*/*" , ".png" } },
                    { DevicePlatform.macOS, new[] { "pdf" , "public.image" } },
                });

                var fResult = await FilePicker.PickAsync(new PickOptions { FileTypes = customFileType });
                if (fResult != null)
                {
                    (bool valid, long size) = await ValidateDocument(fResult);
                    if (!valid)
                    {
                        return;
                    }

                    FileSelected = true;
                    FileResult = fResult;
                    documentObject = FileResult.GetDocumentObject();
                    documentObject.Size = size;
                    FileName = $"File Name: {documentObject.FileName}";

                    if (documentObject.IsImage())
                    {
                        var stream = await FileResult.OpenReadAsync();
                        FileSource = ImageSource.FromStream(() => stream);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"FilePick command failed with {ex}");
                await _dialogContorller.ShowAlertAsync(_localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsLocalFileAccessProblems),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitleCouldNotOpenUrl),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
        }

        private async Task VideoPickCommandHandler()
        {
            try
            {
                FileSource = null;
                var fResult = await MediaPicker.PickVideoAsync(new MediaPickerOptions
                {
                    Title = "Please pick video"
                });

                if (fResult != null)
                {
                    (bool valid, long size) = await ValidateDocument(fResult);
                    if (!valid)
                    {
                        return;
                    }

                    FileSelected = true;
                    FileResult = fResult;
                    documentObject = FileResult.GetDocumentObject();
                    documentObject.Size = size;
                    FileName = $"File Name: {documentObject.FileName}";
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"VideoPick command failed with {ex}");
                await _dialogContorller.ShowAlertAsync(_localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsLocalFileAccessProblems),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitleCouldNotOpenUrl),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
        }

        private async Task ImagePickCommandHandler()
        {
            try
            {
                FileSource = null;
                var fResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Please pick photo"
                });

                if (fResult != null)
                {
                    (bool valid, long size) = await ValidateDocument(fResult);
                    if (!valid)
                    {
                        return;
                    }

                    FileResult = fResult;
                    documentObject = FileResult.GetDocumentObject();
                    documentObject.Size = size;
                    FileName = $"File Name: {documentObject.FileName}";

                    if (documentObject.IsImage())
                    {
                        FileSelected = true;
                        var stream = await FileResult.OpenReadAsync();
                        FileSource = ImageSource.FromStream(() => stream);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"ImagePick command failed with {ex}");
                await _dialogContorller.ShowAlertAsync(_localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsLocalFileAccessProblems),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicErrorTitleCouldNotOpenUrl),
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
            }
        }

        private async Task CaptureImageCommandHandler()
        {
            FileResult fResult = null;
            if (Device.RuntimePlatform != Device.UWP)
            {
                if (CrossMedia.Current.IsTakePhotoSupported && MediaPicker.IsCaptureSupported)
                {
                    FileSource = null;
                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Directory = "Photographs",
                        SaveToAlbum = false,
                        CompressionQuality = 40,
                        CustomPhotoSize = 35,
                        PhotoSize = PhotoSize.MaxWidthHeight,
                        MaxWidthHeight = 2000,
                        DefaultCamera = CameraDevice.Rear
                    }).ConfigureAwait(true);

                    if (file != null)
                    {
                        fResult = new FileResult(file.Path);
                    }
                }
                else
                {
                    await _dialogContorller.ShowAlertAsync("Device do not support Image Capture", "", _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                    return;
                }
            }
            else
            {
                if (MediaPicker.IsCaptureSupported)
                {
                    FileSource = null;
                    fResult = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                    {
                        Title = "Please capture a photo"
                    });
                }
                else
                {
                    await _dialogContorller.ShowAlertAsync("Device do not support Image Capture", "", _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                    return;
                }
            }

            if (fResult != null)
            {
                (bool valid, long size) = await ValidateDocument(fResult);
                if(!valid)
                {
                    return;
                }
                FileResult = fResult;
                documentObject = FileResult.GetDocumentObject();
                documentObject.Size = size;
                FileName = $"File Name: {documentObject.FileName}";
                if (documentObject.IsImage())
                {
                    FileSelected = true;
                    var stream = await FileResult.OpenReadAsync();
                    FileSource = ImageSource.FromStream(() => stream);
                }
            }

        }

        private async Task<(bool,long)> ValidateDocument(FileResult fResult)
        {
            bool valid = true;
            long filesize = 0;
            using (var stream = await fResult.OpenReadAsync())
            {
                filesize = stream.Length;
                var sizekb = filesize / 1000;
                if (sizekb > _maxUploadSize)
                {
                    await _dialogContorller.ShowAlertAsync(
                    $"Document {fResult.FileName} exceed maximum allowed size of {_maxUploadSize / 1000} MB to be uploaded.",
                    "",
                    _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                    valid = false;
                }
            }
            return (valid, filesize);
        }

        public async Task<bool> FileUploadCommandHandler()
        {
            IsLoading = true;
            bool result = false;
            if (documentObject != null)
            {
                List<PanelData> inputPanels = Widgets.GetPanelDatas();

                if (inputPanels != null && inputPanels.Count > 0)
                {
                    if (!_docService.IsMandatoryDataReady(inputPanels))
                    {
                        // Mandatory field Message;
                        var mandatoryMessage = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsEditMandatoryFieldNotSet);
                        await _dialogContorller.ShowAlertAsync(
                        mandatoryMessage,
                        "",
                        _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
                        IsLoading = false;
                        Message = mandatoryMessage;
                        return result;
                    }
                }

                try
                {

                    if(DocField != null)
                    {
                        documentObject.FieldId = DocField.Config.FieldConfig.FieldId;
                    }

                    using (documentObject.FileStream = await FileResult.OpenReadAsync())
                    {
                        result = await _docService.UploadDocument(documentObject, inputPanels, _cancellationTokenSource.Token);
                    }

                    if (result)
                    {
                        if (!_sessionContext.IsInOfflineMode && _iImageSelectorInterface == null)
                        {
                            Message = $"Document {_fileResult.FileName} uploaded successfully.";
                            await _navigationController.RefreshPopupDisplayPage();
                        }
                        else
                        {
                            Message = $"Document {_fileResult.FileName} will be uploaded, when back online.";

                        }
                    }
                    else
                    {
                        Message = $"Upload of Document {_fileResult.FileName} failed.";
                    }
                }
                catch (CrmException ex)
                {
                    if (ex.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                    {
                        _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);

                    }

                    Message = $"Upload of Document {_fileResult.FileName} failed.";
                }
                catch (Exception ex)
                {
                    Message = $"Upload of Document {_fileResult.FileName} failed.";
                }

            }
            IsLoading = false;
            return result;
        }
    }
}
