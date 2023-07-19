using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Questionnaire
{
    public class QuestionnaireQuestionData
    {
        public QuestionnaireQuestion QuestionnaireQuestion { get; }
        public readonly Dictionary<int, QuestionnaireAnswer> QuestionnaireAnswers = new Dictionary<int, QuestionnaireAnswer>();
        public readonly Dictionary<int, QuestionnaireAnswerData> QuestionnaireAnswerData = new Dictionary<int, QuestionnaireAnswerData>();

        public QuestionnaireQuestionData(QuestionnaireQuestion questionnaireQuestion, 
            List<QuestionnaireAnswer> questionnaireAnswers, List<QuestionnaireAnswerData> questionnaireAnswerDataList)
        {
            QuestionnaireQuestion = questionnaireQuestion;
            
            foreach(QuestionnaireAnswer questionnaireAnswer in questionnaireAnswers)
            {
                QuestionnaireAnswers.Add(questionnaireAnswer.AnswerNumber, questionnaireAnswer);
            }

            if(questionnaireAnswerDataList != null)
            {
                foreach (QuestionnaireAnswerData questionnaireAnswerData in questionnaireAnswerDataList)
                {
                    QuestionnaireAnswerData.Add(questionnaireAnswerData.AnswerNumber, questionnaireAnswerData);
                }
            }
        }
    }
}
