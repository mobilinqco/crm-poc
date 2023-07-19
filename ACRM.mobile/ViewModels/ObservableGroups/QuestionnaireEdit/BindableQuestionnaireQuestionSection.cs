using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.ViewModels.Base;
using System.Collections.Generic;
using static ACRM.mobile.UIModels.QuestionnaireEditModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit
{
    public class BindableQuestionnaireQuestionSection : ExtendedBindableObject
    {
        public string Label { get; }
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
        public List<BindableQuestionnaireQuestionData> BindableQuestionnaireQuestionDataList { get; } = new List<BindableQuestionnaireQuestionData>();

        public BindableQuestionnaireQuestionSection(QuestionnaireQuestionSection questionnaireQuestionSection)
        {
            Label = questionnaireQuestionSection.QuestionnaireQuestion.Label;

            foreach (QuestionnaireQuestionData questionnaireQuestionData in questionnaireQuestionSection.QuestionnaireQuestionData)
            {
                if(questionnaireQuestionData.QuestionnaireAnswers.Count > 0)
                {
                    BindableQuestionnaireQuestionDataList.Add(new BindableQuestionnaireSelectableQuestionData(questionnaireQuestionData));
                }
                else if (questionnaireQuestionData.QuestionnaireAnswers.Count == 0)
                {
                    BindableQuestionnaireQuestionDataList.Add(new BindableQuestionnaireTextQuestionData(questionnaireQuestionData));
                }
            }
        }

        public void ApplyViewTypeFilter(QuestionnaireEditViewTypes viewType)
        {
            bool isVisible = false;
            foreach (BindableQuestionnaireQuestionData bindableQuestionnaireQuestionData in BindableQuestionnaireQuestionDataList)
            {
                isVisible = isVisible | bindableQuestionnaireQuestionData.ApplyViewTypeFilter(viewType);
            }
            IsVisible = isVisible;
        }
    }
}
