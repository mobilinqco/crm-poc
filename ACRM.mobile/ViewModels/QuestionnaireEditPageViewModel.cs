using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit;
using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class QuestionnaireEditPageViewModel : NavigationBarBaseViewModel
    {
        public ICommand OnCancelCommand => new Command(async () => await OnCancel());
        public ICommand OnSaveCommand => new Command(async () => await OnSave());

        private Color _infoAreaColor = Color.LightGray;
        public Color InfoAreaColor
        {
            get => _infoAreaColor;
            set
            {
                _infoAreaColor = value;
                RaisePropertyChanged(() => InfoAreaColor);
            }
        }

        public ObservableCollection<ErrorInfo> ErrorsInfo { get; set; }

        private QuestionnaireEditModel _questionnaireEditModel;
        public QuestionnaireEditModel QuestionnaireEditModel
        {
            get => _questionnaireEditModel;
            set
            {
                _questionnaireEditModel = value;
                RaisePropertyChanged(()=>QuestionnaireEditModel);
            }
        }

        private string _cancelButtonTitle;
        public string CancelButtonTitle
        {
            get
            {
                return _cancelButtonTitle;
            }

            set
            {
                _cancelButtonTitle = value;
                RaisePropertyChanged(() => CancelButtonTitle);
            }
        }

        private bool _isCancelButtonEnabled = true;
        public bool IsCancelButtonEnabled
        {
            get => _isCancelButtonEnabled;
            set
            {
                _isCancelButtonEnabled = value;
                RaisePropertyChanged(() => IsCancelButtonEnabled);
            }
        }

        private string _saveButtonTitle;
        public string SaveButtonTitle
        {
            get
            {
                return _saveButtonTitle;
            }

            set
            {
                _saveButtonTitle = value;
                RaisePropertyChanged(() => SaveButtonTitle);
            }
        }

        private bool _isErrorMessageVisible = false;
        public bool IsErrorMessageVisible
        {
            get => _isErrorMessageVisible;
            set
            {
                _isErrorMessageVisible = value;
                RaisePropertyChanged(() => IsErrorMessageVisible);
            }
        }

        private bool _isSaveButtonEnabled = false;
        public bool IsSaveButtonEnabled
        {
            get => _isSaveButtonEnabled;
            set
            {
                _isSaveButtonEnabled = value;
                RaisePropertyChanged(() => IsSaveButtonEnabled);
            }
        }

        public QuestionnaireEditPageViewModel()
        {
            IsLoading = true;
            ErrorsInfo = new ObservableCollection<ErrorInfo> { new ErrorInfo() };
            RegisterMessages();
        }

        private void RegisterMessages()
        {
            RegisterMessage(WidgetEventType.ErrorOccurred, "QuestionnaireEditModel", OnErrorOccurred);
            RegisterMessage(WidgetEventType.QuestionnaireDataSaveCompleted, "QuestionnaireEditModel", OnQuestionnaireDataSaveCompleted);
            RegisterMessage(WidgetEventType.QuestionnaireFinalizeInitiated, "QuestionnaireEditModel", OnQuestionnaireFinalizeInitiated);
        }

        private Task OnErrorOccurred(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is CrmException crmException)
            {
                IsErrorMessageVisible = true;
                ErrorsInfo[0].Name = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                ErrorsInfo[0].Description = $"{crmException.Content}";
            }
            else if(widgetMessage.Data is Exception exception)
            {
                IsErrorMessageVisible = true;
                ErrorsInfo[0].Name = _localizationController.GetString(LocalizationKeys.TextGroupErrors, LocalizationKeys.KeyErrorsCouldNotBeSavedDetailMessage);
                ErrorsInfo[0].Description = $"{exception.Message}";
            }

            return Task.CompletedTask;
        }

        private async Task OnQuestionnaireDataSaveCompleted(WidgetMessage widgetMessage)
        {
            IsLoading = false;
            IsSaveButtonEnabled = true;
            await OnCancel();
        }

        private Task OnQuestionnaireFinalizeInitiated(WidgetMessage widgetMessage)
        {
            IsLoading = true;
            IsSaveButtonEnabled = false;
            return Task.CompletedTask;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsBackButtonVisible = true;
            IsLoading = true;
            IsSaveButtonEnabled = false;
            if (navigationData is UserAction)
            {
                IsBusy = true;
                QuestionnaireEditModel = new QuestionnaireEditModel(navigationData, _cancellationTokenSource);
                QuestionnaireEditModel.ParentBaseModel = this;
                await QuestionnaireEditModel.InitializeControl();
                Widgets = new ObservableCollection<UIWidget>();
                Widgets.Add(QuestionnaireEditModel);
                InitiateBindings();
                IsBusy = false;
            }
            IsLoading = false;
            IsSaveButtonEnabled = true;
            await base.InitializeAsync(navigationData);
        }

        private void InitiateBindings()
        {
            PageTitle = QuestionnaireEditModel.GetPageTitle();
            InfoAreaColor = Color.FromHex(QuestionnaireEditModel.GetInfoAreaColor());

            CancelButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);

            if (!QuestionnaireEditModel.NoContent && !QuestionnaireEditModel.IsFinalized)
            {
                SaveButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSave);
            }
            else
            {
                SaveButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            }

            if (QuestionnaireEditModel.NoContent || QuestionnaireEditModel.IsFinalized)
            {
                IsCancelButtonEnabled = false;
            }
        }

        private async Task OnSave(bool isFinalSave = false)
        {
            if(!QuestionnaireEditModel.IsFinalized && !QuestionnaireEditModel.NoContent)
            {
                IsLoading = true;
                IsSaveButtonEnabled = false;

                WidgetMessage widgetMessage = new WidgetMessage
                {
                    ControlKey = "QuestionnaireEditPageViewModel",
                    EventType = WidgetEventType.QuestionnaireDataSaveInitiated
                };

                await PublishMessage(widgetMessage, MessageDirections.ToChildren);
            }
            else
            {
                await OnCancel();
            }
        }

        private async Task OnCancel()
        {
            await _navigationController.BackAsync();
        }
    }
}
