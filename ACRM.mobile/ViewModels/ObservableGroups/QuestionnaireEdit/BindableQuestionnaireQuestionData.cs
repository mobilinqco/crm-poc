using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.ViewModels.Base;
using System.Collections.Generic;
using static ACRM.mobile.UIModels.QuestionnaireEditModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit
{
    public abstract class BindableQuestionnaireQuestionData : ExtendedBindableObject
    {
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
        public QuestionnaireQuestionData QuestionnaireQuestionData { get; }

        public BindableQuestionnaireQuestionData(QuestionnaireQuestionData questionnaireQuestionData)
        {
            QuestionnaireQuestionData = questionnaireQuestionData;
        }

        public abstract bool ApplyViewTypeFilter(QuestionnaireEditViewTypes viewType);

        public abstract bool IsCompleted();

        public bool IsMandatory()
        {
            return QuestionnaireQuestionData.QuestionnaireQuestion.Mandatory;
        }

        public abstract bool IsAnswerModified(int answerNumber);

        public abstract bool IsAnswerNew(int answerNumber);

        public abstract bool IsAnswerRemoved(int answerNumber);

        public string GetAnswerDataRecordId(int answerNumber)
        {
            if (QuestionnaireQuestionData.QuestionnaireAnswerData.ContainsKey(answerNumber))
            {
                return QuestionnaireQuestionData.QuestionnaireAnswerData[answerNumber].RecordId;
            }
            return null;
        }

        public string GetQuestionRecordId()
        {
            return QuestionnaireQuestionData.QuestionnaireQuestion.QuestionnaireQuestionRecordId;
        }

        public abstract string GetAnswerRecordId(int answerNumber);

        public string GetQuestionNumber()
        {
            return QuestionnaireQuestionData.QuestionnaireQuestion.QuestionNumber.ToString();
        }

        public virtual string GetCurrentAnswer(int answerNumber)
        {
            return "";
        }

        public virtual string GetInitialAnswer(int answerNumber)
        {
            return "";
        }
    }
}
