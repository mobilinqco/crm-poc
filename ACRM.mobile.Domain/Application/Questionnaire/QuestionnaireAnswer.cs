using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Questionnaire
{
    public class QuestionnaireAnswer
    {
        public string QuestionnaireAnswerRecordId { get; private set; }
        public string QuestionnaireID { get; private set; }
        public int QuestionNumber { get; private set; }
        public int AnswerNumber { get; private set; }
        public string Label { get; private set; }
        public int FollowUpNumber { get; private set; }

        public QuestionnaireAnswer(string questionnaireAnswerRecordId, string questionnaireID, int questionNumber, int answerNumber, string label, int followUpNumber)
        {
            QuestionnaireAnswerRecordId = questionnaireAnswerRecordId;
            QuestionnaireID = questionnaireID;
            QuestionNumber = questionNumber;
            AnswerNumber = answerNumber;
            Label = label;
            FollowUpNumber = followUpNumber;
        }
    }
}
