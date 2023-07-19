using ACRM.mobile.Domain.Application.Questionnaire;
using System;
using System.Collections.Generic;
using System.Text;
using static ACRM.mobile.UIModels.QuestionnaireEditModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit
{
    public class BindableQuestionnaireSelectableQuestionData : BindableQuestionnaireQuestionData
    {
        public string Label { get; }
        public List<BindableQuestionnaireSelectableAnswer> BindableQuestionnaireSelectableAnswers { get; } = new List<BindableQuestionnaireSelectableAnswer>();

        private bool _isMultipleSelection = false;
        private readonly HashSet<int> _initiallySelectedAnswerNumbers = new HashSet<int>();
        private readonly HashSet<int> _currentlySelectedAnswerNumbers = new HashSet<int>();

        public BindableQuestionnaireSelectableQuestionData(QuestionnaireQuestionData questionnaireQuestionData) : base(questionnaireQuestionData)
        {
            Label = questionnaireQuestionData.QuestionnaireQuestion.Label;
            _isMultipleSelection = questionnaireQuestionData.QuestionnaireQuestion.Multiple;
            foreach (KeyValuePair<int, QuestionnaireAnswer> entry in questionnaireQuestionData.QuestionnaireAnswers)
            {
                bool isSelected = questionnaireQuestionData.QuestionnaireAnswerData.ContainsKey(entry.Value.AnswerNumber);
                if(isSelected)
                {
                    _initiallySelectedAnswerNumbers.Add(entry.Value.AnswerNumber);
                    _currentlySelectedAnswerNumbers.Add(entry.Value.AnswerNumber);
                }
                BindableQuestionnaireSelectableAnswers.Add(new BindableQuestionnaireSelectableAnswer(this, entry.Value, isSelected));
            }
        }

        public void SetSelectedItem(string selectedQuestionnaireAnswerRecordId, int answerNumber, bool isSelected)
        {
            if (isSelected)
            {
                _currentlySelectedAnswerNumbers.Add(answerNumber);

                if (!_isMultipleSelection)
                {
                    foreach (BindableQuestionnaireSelectableAnswer bindableQuestionnaireSelectableAnswer in BindableQuestionnaireSelectableAnswers)
                    {
                        if (bindableQuestionnaireSelectableAnswer.QuestionnaireAnswer.QuestionnaireAnswerRecordId != selectedQuestionnaireAnswerRecordId &&
                            bindableQuestionnaireSelectableAnswer.IsSelected)
                        {
                            bindableQuestionnaireSelectableAnswer.IsSelected = false;
                            _currentlySelectedAnswerNumbers.Remove(bindableQuestionnaireSelectableAnswer.QuestionnaireAnswer.AnswerNumber);
                        }
                    }
                }
            }
            else
            {
                _currentlySelectedAnswerNumbers.Remove(answerNumber);
            }
        }

        public override bool ApplyViewTypeFilter(QuestionnaireEditViewTypes viewType)
        {
            bool isVisible = false;
            foreach (BindableQuestionnaireSelectableAnswer bindableQuestionnaireSelectableAnswer in BindableQuestionnaireSelectableAnswers)
            {
                isVisible = isVisible | bindableQuestionnaireSelectableAnswer.ApplyViewTypeFilter(viewType);
            }
            IsVisible = isVisible;
            return isVisible;
        }

        public override bool IsCompleted()
        {
            foreach (BindableQuestionnaireSelectableAnswer bindableQuestionnaireSelectableAnswer in BindableQuestionnaireSelectableAnswers)
            {
                if(bindableQuestionnaireSelectableAnswer.IsCompleted())
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsAnswerModified(int answerNumber)
        {
            //Selectable answers cannot be modified, they are either selected or not
            return false;
        }

        public override bool IsAnswerNew(int answerNumber)
        {
            return !_initiallySelectedAnswerNumbers.Contains(answerNumber) && _currentlySelectedAnswerNumbers.Contains(answerNumber);
        }

        public override bool IsAnswerRemoved(int answerNumber)
        {
            return _initiallySelectedAnswerNumbers.Contains(answerNumber) && !_currentlySelectedAnswerNumbers.Contains(answerNumber);
        }

        public override string GetAnswerRecordId(int answerNumber)
        {
            if(QuestionnaireQuestionData.QuestionnaireAnswers.ContainsKey(answerNumber))
            {
                return QuestionnaireQuestionData.QuestionnaireAnswers[answerNumber].QuestionnaireAnswerRecordId;
            }
            return "";
        }
    }
}
