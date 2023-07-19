using ACRM.mobile.Domain.Application.Questionnaire;
using System;
using System.Collections.Generic;
using System.Text;
using static ACRM.mobile.UIModels.QuestionnaireEditModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit
{
    public class BindableQuestionnaireTextQuestionData : BindableQuestionnaireQuestionData
    {
        public string Label { get; }
        public BindableQuestionnaireTextAnswer BindableQuestionnaireTextAnswer { get; }

        private bool _isNew = true;
        private string _initialContentValue = "";

        public BindableQuestionnaireTextQuestionData(QuestionnaireQuestionData questionnaireQuestionData) : base(questionnaireQuestionData)
        {
            Label = questionnaireQuestionData.QuestionnaireQuestion.Label;
            string contentValue = "";
            if(questionnaireQuestionData.QuestionnaireAnswerData.ContainsKey(0))
            {
                _isNew = false;
                // AnswerNumber is always 0 for text question data.
                contentValue = questionnaireQuestionData.QuestionnaireAnswerData[0].Answer;
            }
            _initialContentValue = contentValue;
            BindableQuestionnaireTextAnswer = new BindableQuestionnaireTextAnswer(this, contentValue);
        }

        public override bool ApplyViewTypeFilter(QuestionnaireEditViewTypes viewType)
        {
            bool isVisible = BindableQuestionnaireTextAnswer.ApplyViewTypeFilter(viewType);
            IsVisible = isVisible;
            return isVisible;
        }

        public override bool IsCompleted()
        {
            return BindableQuestionnaireTextAnswer.IsCompleted();
        }

        public override bool IsAnswerModified(int answerNumber)
        {
            //In the case of text only records, the answer number should always be equal to 0.
            if(answerNumber == 0)
            {
                return _initialContentValue != BindableQuestionnaireTextAnswer.CurrentContentValue;
            }
            else
            {
                return false;
            }
        }

        public override bool IsAnswerNew(int answerNumber)
        {
            //In the case of text only records, the answer number should always be equal to 0.
            if (answerNumber == 0)
            {
                return _isNew;
            }
            else
            {
                return false;
            }
        }

        public override bool IsAnswerRemoved(int answerNumber)
        {
            //Text only records are not deleted, they are updated to empty string.
            //Answer number is irrelevant in this case for text only records.
            return false;
        }

        public override string GetAnswerRecordId(int answerNumber)
        {
            // Text only records do not have an AnswerRecordId
            //Answer number is irrelevant in this case for text only records.
            return null;
        }

        public override string GetCurrentAnswer(int answerNumber)
        {
            //In the case of text only records, the answer number should always be equal to 0.
            if (answerNumber == 0)
            {
                return BindableQuestionnaireTextAnswer.CurrentContentValue;
            }
            else
            {
                return "";
            }
        }

        public override string GetInitialAnswer(int answerNumber)
        {
            //In the case of text only records, the answer number should always be equal to 0.
            if (answerNumber == 0)
            {
                return _initialContentValue;
            }
            else
            {
                return "";
            }
        }
    }
}
