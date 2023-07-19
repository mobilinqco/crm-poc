using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Questionnaire
{
    public class QuestionnaireQuestionSection
    {
        public QuestionnaireQuestion QuestionnaireQuestion { get; }
        public List<QuestionnaireQuestionData> QuestionnaireQuestionData = new List<QuestionnaireQuestionData>();

        public QuestionnaireQuestionSection(QuestionnaireQuestion questionnaireQuestion)
        {
            QuestionnaireQuestion = questionnaireQuestion;
        }

        public void AddquestionnaireQuestionData(QuestionnaireQuestionData questionnaireQuestionData)
        {
            QuestionnaireQuestionData.Add(questionnaireQuestionData);
        }
    }
}
