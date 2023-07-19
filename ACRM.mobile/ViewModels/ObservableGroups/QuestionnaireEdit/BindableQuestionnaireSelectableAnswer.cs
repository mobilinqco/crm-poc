using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.ViewModels.Base;
using static ACRM.mobile.UIModels.QuestionnaireEditModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit
{
    public class BindableQuestionnaireSelectableAnswer : ExtendedBindableObject
    {
        public string Label { get; }
        public bool IsSingleSelection { get; }
        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _bindableQuestionnaireSelectableQuestionData.SetSelectedItem(
                    QuestionnaireAnswer.QuestionnaireAnswerRecordId, QuestionnaireAnswer.AnswerNumber, value);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }
        public QuestionnaireAnswer QuestionnaireAnswer { get; }

        private BindableQuestionnaireSelectableQuestionData _bindableQuestionnaireSelectableQuestionData;

        public BindableQuestionnaireSelectableAnswer(BindableQuestionnaireSelectableQuestionData bindableQuestionnaireSelectableQuestionData, QuestionnaireAnswer questionnaireAnswer, bool isSelected)
        {
            _bindableQuestionnaireSelectableQuestionData = bindableQuestionnaireSelectableQuestionData;
            QuestionnaireAnswer = questionnaireAnswer;
            Label = QuestionnaireAnswer.Label;
            IsSingleSelection = !bindableQuestionnaireSelectableQuestionData.QuestionnaireQuestionData.QuestionnaireQuestion.Multiple;
            IsSelected = isSelected;
        }

        public bool ApplyViewTypeFilter(QuestionnaireEditViewTypes viewType)
        {
            switch (viewType)
            {
                case QuestionnaireEditViewTypes.Mandatory:
                    IsVisible = _bindableQuestionnaireSelectableQuestionData.IsMandatory();
                    IsEnabled = true;
                    break;
                case QuestionnaireEditViewTypes.Summary:
                    IsVisible = IsSelected;
                    IsEnabled = false;
                    break;
                case QuestionnaireEditViewTypes.All:
                default:
                    IsVisible = true;
                    IsEnabled = true;
                    break;
            }
            return IsVisible;
        }

        public bool IsCompleted()
        {
            return IsSelected;
        }
    }
}
