using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Questionnaire
{
    public class QuestionnaireAnswerData
    {
        public string RecordId { get; private set; }
        public int QuestionNumber { get; private set; }
        public int AnswerNumber { get; private set; }
        public string Answer { get; private set; }

        public QuestionnaireAnswerData(string recordId, int questionNumber, int answerNumber, string answer)
        {
            RecordId = recordId;
            QuestionNumber = questionNumber;
            AnswerNumber = answerNumber;
            Answer = answer;
        }
    }
}
