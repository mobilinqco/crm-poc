using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit;
using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class QuestionnaireEditModel : UIWidget
    {
        public enum QuestionnaireEditViewTypes
        {
            All, Mandatory, Summary
        }

        public ICommand SelectionChangedCommand => new Command<Syncfusion.XForms.Buttons.SelectionChangedEventArgs>((args) => OnSelectionChanged(args));
        public ICommand QuestionnaireSelectableAnswerTappedCommand => new Command<BindableQuestionnaireSelectableAnswer>((args) => OnQuestionnaireSelectableAnswerTapped(args));
        public ICommand QuestionnaireTextAnswerChangedCommand => new Command<BindableQuestionnaireTextAnswer>((args) => OnQuestionnaireTextAnswerChanged(args));
        public ICommand OnFinalizeCommand => new Command(async () => await FinalizeQuestionnaire());

        private readonly IQuestionnaireContentService _contentService;
        private readonly BackgroundSyncManager _backgroundSyncManager;

        private readonly UserAction _userAction;

        private bool _noContent = true;
        public bool NoContent
        {
            get => _noContent;
            set
            {
                _noContent = value;
                RaisePropertyChanged(() => NoContent);
            }
        }

        private string _noContentString = "";
        public string NoContentString
        {
            get => _noContentString;
            set
            {
                _noContentString = value;
                RaisePropertyChanged(() => NoContentString);
            }
        }

        private bool _isFinalized = false;
        public bool IsFinalized
        {
            get => _isFinalized;
            set
            {
                _isFinalized = value;
                RaisePropertyChanged(() => IsFinalized);
            }
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                RaisePropertyChanged(() => IsReadOnly);
            }
        }

        public ObservableCollection<ErrorInfo> ErrorsInfo { get; set; }

        private string _questionnaireLabel = "";
        public string QuestionnaireLabel
        {
            get => _questionnaireLabel;
            set
            {
                _questionnaireLabel = value;
                RaisePropertyChanged(() => QuestionnaireLabel);
            }
        }

        private int _segmentedControlIndex = 0;
        public int SegmentedControlIndex
        {
            get
            {
                return _segmentedControlIndex;
            }
            set
            {
                _segmentedControlIndex = value;
                RaisePropertyChanged(() => SegmentedControlIndex);
            }
        }

        private ObservableCollection<SfSegmentItem> _viewTypes;
        public ObservableCollection<SfSegmentItem> EditViewTypeItems
        {
            get => _viewTypes;
            set
            {
                _viewTypes = value;
                RaisePropertyChanged(() => EditViewTypeItems);
            }
        }

        private int _viewModesCounter;
        public int ViewModesCounter
        {
            get => _viewModesCounter;
            set
            {
                _viewModesCounter = value;
                RaisePropertyChanged(() => ViewModesCounter);
            }
        }

        private bool _isTypeSelectionVisible = true;
        public bool IsTypeSelectionVisible
        {
            get => _isTypeSelectionVisible;
            set
            {
                _isTypeSelectionVisible = value;
                RaisePropertyChanged(()=>IsTypeSelectionVisible);
            }
        }

        private ObservableCollection<BindableQuestionnaireQuestionSection> _bindableQuestionnaireQuestionSections = new ObservableCollection<BindableQuestionnaireQuestionSection>();
        public ObservableCollection<BindableQuestionnaireQuestionSection> BindableQuestionnaireQuestionSections
        {
            get => _bindableQuestionnaireQuestionSections;
            set
            {
                _bindableQuestionnaireQuestionSections = value;
                RaisePropertyChanged(() => BindableQuestionnaireQuestionSections);
            }
        }

        private string _completionPercentageString = "";
        public string CompletionPercentageString
        {
            get => _completionPercentageString;
            set
            {
                _completionPercentageString = value;
                RaisePropertyChanged(() => CompletionPercentageString);
            }
        }

        private string _finalizeStateString = "";
        public string FinalizeStateString
        {
            get => _finalizeStateString;
            set
            {
                _finalizeStateString = value;
                RaisePropertyChanged(() => FinalizeStateString);
            }
        }

        private bool _allMandatoryCompleted = false;
        public bool AllMandatoryCompleted
        {
            get => _allMandatoryCompleted;
            set
            {
                _allMandatoryCompleted = value;
                RaisePropertyChanged(() => AllMandatoryCompleted);
            }
        }

        public QuestionnaireEditModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            IsLoading = true;
            _contentService = AppContainer.Resolve<IQuestionnaireContentService>();
            _backgroundSyncManager = AppContainer.Resolve<BackgroundSyncManager>();
            if (widgetArgs != null && widgetArgs is UserAction userAction)
            {
                _userAction = userAction;
            }
        }

        public override async ValueTask<bool> InitializeControl()
        {
            IsLoading = true;
            IsBusy = true;
            if (_userAction != null)
            {
                _contentService.SetSourceAction(_userAction);
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                InitiateBindings();
                RegisterMessages();
            }
            IsBusy = false;
            IsLoading = false;
            return true;
        }

        private void InitiateBindings()
        {
            QuestionnaireLabel = _contentService.QuestionnaireLabel;

            EditViewTypeItems = new ObservableCollection<SfSegmentItem>
            {
                new SfSegmentItem { Text = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesQuestionnaireSurveyAllQuestions), StyleId = QuestionnaireEditViewTypes.All.ToString() },
                new SfSegmentItem { Text = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesQuestionnaireMandatory), StyleId = QuestionnaireEditViewTypes.Mandatory.ToString() },
                new SfSegmentItem { Text = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesQuestionnaireSummary), StyleId = QuestionnaireEditViewTypes.Summary.ToString() }
            };
            ViewModesCounter = EditViewTypeItems.Count;

            List<QuestionnaireQuestionSection> questionnaireQuestionSections = _contentService.GetQuestionnaireQuestionSections();
            if (questionnaireQuestionSections.Count != 0)
            {
                NoContent = false;
                foreach (QuestionnaireQuestionSection questionnaireQuestionSection in questionnaireQuestionSections)
                {
                    _bindableQuestionnaireQuestionSections.Add(new BindableQuestionnaireQuestionSection(questionnaireQuestionSection));
                }
                BindableQuestionnaireQuestionSections = _bindableQuestionnaireQuestionSections;
            }
            else
            {
                NoContentString = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesQuestionnaireNotAvailable);
            }

            IsFinalized = _contentService.IsQuestionnaireFinalized();
            IsReadOnly = _contentService.IsQuestionnaireReadOnly();

            if (IsFinalized || IsReadOnly)
            {
                SegmentedControlIndex = 2;
                IsTypeSelectionVisible = false;
            }

            SfSegmentItem selectedMode = EditViewTypeItems[SegmentedControlIndex];
            if (Enum.TryParse(selectedMode.StyleId.ToString(), true, out QuestionnaireEditViewTypes viewType))
            {
                FilterQuestionnaireQuestionSectionsByViewType(viewType);
            }

            ComputeCompletionPercentage();

            if (!_isReadOnly)
            {
                SetFinalizeStateString();
            }
        }

        private void OnSelectionChanged(Syncfusion.XForms.Buttons.SelectionChangedEventArgs selectionChangedEventArgs)
        {
            SfSegmentItem selectedMode = EditViewTypeItems[selectionChangedEventArgs.Index];
            if (Enum.TryParse(selectedMode.StyleId.ToString(), true, out QuestionnaireEditViewTypes viewType))
            {
                FilterQuestionnaireQuestionSectionsByViewType(viewType);
            }
        }

        private void OnQuestionnaireSelectableAnswerTapped(BindableQuestionnaireSelectableAnswer bindableQuestionnaireSelectableAnswer)
        {
            bindableQuestionnaireSelectableAnswer.IsSelected = !bindableQuestionnaireSelectableAnswer.IsSelected;
            ComputeCompletionPercentage();
        }

        private void OnQuestionnaireTextAnswerChanged(BindableQuestionnaireTextAnswer bindableQuestionnaireTextAnswer)
        {
            ComputeCompletionPercentage();
        }

        private void FilterQuestionnaireQuestionSectionsByViewType(QuestionnaireEditViewTypes viewType)
        {
            foreach (BindableQuestionnaireQuestionSection bindableQuestionnaireQuestionSection in _bindableQuestionnaireQuestionSections)
            {
                bindableQuestionnaireQuestionSection.ApplyViewTypeFilter(viewType);
            }
            BindableQuestionnaireQuestionSections = _bindableQuestionnaireQuestionSections;
        }

        private void ComputeCompletionPercentage()
        {
            int total = 0;
            int completed = 0;
            bool allMandatoryCompleted = true;

            foreach (BindableQuestionnaireQuestionSection bindableQuestionnaireQuestionSection in _bindableQuestionnaireQuestionSections)
            {
                foreach (BindableQuestionnaireQuestionData bindableQuestionnaireQuestionData in bindableQuestionnaireQuestionSection.BindableQuestionnaireQuestionDataList)
                {
                    total += 1;
                    if (bindableQuestionnaireQuestionData.IsCompleted())
                    {
                        completed += 1;
                    }
                    else
                    {
                        if (bindableQuestionnaireQuestionData.IsMandatory())
                        {
                            allMandatoryCompleted = false;
                        }
                    }
                }
            }

            if (total > 0)
            {
                CompletionPercentageString = string.Format("{0}%", Math.Ceiling(decimal.Divide(completed, total) * 100));
            }

            AllMandatoryCompleted = allMandatoryCompleted;
        }

        private void SetFinalizeStateString()
        {
            if (!_isFinalized)
            {
                FinalizeStateString = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesQuestionnaireFinalize);
            }
            else
            {
                FinalizeStateString = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesQuestionnaireFinalized);
            }
        }

        private void RegisterMessages()
        {
            RegisterMessage(WidgetEventType.QuestionnaireDataSaveInitiated, "QuestionnaireEditPageViewModel", OnQuestionnaireDataSaveInitiated);
        }

        private async Task OnQuestionnaireDataSaveInitiated(WidgetMessage widgetMessage)
        {
            await SaveQuestionnaireData();
        }

        private async Task SaveQuestionnaireData(bool isFinalSave = false)
        {
            if (!NoContent && !IsFinalized && !IsLoading)
            {
                IsLoading = true;
                try
                {
                    foreach (BindableQuestionnaireQuestionSection bindableQuestionnaireQuestionSection in _bindableQuestionnaireQuestionSections)
                    {
                        foreach (BindableQuestionnaireQuestionData bindableQuestionnaireQuestionData in bindableQuestionnaireQuestionSection.BindableQuestionnaireQuestionDataList)
                        {

                            if (bindableQuestionnaireQuestionData.QuestionnaireQuestionData.QuestionnaireAnswers.Count > 0)
                            {
                                foreach (KeyValuePair<int, QuestionnaireAnswer> entry in bindableQuestionnaireQuestionData.QuestionnaireQuestionData.QuestionnaireAnswers)
                                {
                                    await HandleAnswerState(bindableQuestionnaireQuestionData, entry.Key);
                                }
                            }
                            else
                            {
                                await HandleAnswerState(bindableQuestionnaireQuestionData, 0);
                            }
                        }
                    }
                    if (isFinalSave)
                    {
                        await _contentService.SaveQuestionnaireState(_cancellationTokenSource.Token);
                    }

                    WidgetMessage widgetMessage = new WidgetMessage
                    {
                        ControlKey = "QuestionnaireEditModel",
                        EventType = WidgetEventType.QuestionnaireDataSaveCompleted
                    };

                    await PublishMessage(widgetMessage);
                }
                catch (CrmException ex)
                {
                    if (ex.ExceptionSubType == CrmExceptionSubType.CrmDataRequestError)
                    {
                        _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                    }
                    _logService.LogError("Error saving the record.");

                    WidgetMessage widgetMessage = new WidgetMessage
                    {
                        ControlKey = "QuestionnaireEditModel",
                        Data = ex,
                        EventType = WidgetEventType.ErrorOccurred
                    };

                    await PublishMessage(widgetMessage);
                }
                catch (Exception ex)
                {
                    _logService.LogError("Error saving the record to the network.");

                    WidgetMessage widgetMessage = new WidgetMessage
                    {
                        ControlKey = "QuestionnaireEditModel",
                        Data = ex,
                        EventType = WidgetEventType.ErrorOccurred
                    };

                    await PublishMessage(widgetMessage);
                }
            }
        }

        private async Task HandleAnswerState(BindableQuestionnaireQuestionData bindableQuestionnaireQuestionData, int answerNumber)
        {
            if (bindableQuestionnaireQuestionData.IsAnswerNew(answerNumber) || bindableQuestionnaireQuestionData.IsAnswerModified(answerNumber))
            {
                await _contentService.SaveAnswerData(bindableQuestionnaireQuestionData.GetQuestionNumber(), answerNumber.ToString(),
                    bindableQuestionnaireQuestionData.GetCurrentAnswer(answerNumber), bindableQuestionnaireQuestionData.GetInitialAnswer(answerNumber),
                    bindableQuestionnaireQuestionData.GetAnswerDataRecordId(answerNumber), bindableQuestionnaireQuestionData.GetQuestionRecordId(),
                    bindableQuestionnaireQuestionData.GetAnswerRecordId(answerNumber), _cancellationTokenSource.Token);
            }
            else if (bindableQuestionnaireQuestionData.IsAnswerRemoved(answerNumber))
            {
                await _contentService.DeleteAnswerData(bindableQuestionnaireQuestionData.GetAnswerDataRecordId(answerNumber), _cancellationTokenSource.Token);
            }
        }

        private async Task FinalizeQuestionnaire()
        {
            if(!_isFinalized)
            {
                WidgetMessage widgetMessage = new WidgetMessage
                {
                    ControlKey = "QuestionnaireEditModel",
                    EventType = WidgetEventType.QuestionnaireFinalizeInitiated
                };

                await PublishMessage(widgetMessage);

                await SaveQuestionnaireData(true);
            }
        }

        public string GetPageTitle()
        {
            return _contentService.PageTitle();
        }

        public string GetInfoAreaColor()
        {
            return _contentService.PageAccentColor();
        }
    }
}
