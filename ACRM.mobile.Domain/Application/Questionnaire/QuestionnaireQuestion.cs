using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Questionnaire
{
    public class QuestionnaireQuestion
    {
        public string QuestionnaireQuestionRecordId { get; private set; }
        public string QuestionnaireID { get; private set; }
        public int QuestionNumber { get; private set; }
        public string Label { get; private set; }
        public bool Multiple { get; private set; }
        public string InfoAreaId { get; private set; }
        public string FieldId { get; private set; }
        public int FollowUpNumber { get; private set; }
        public bool NewSection { get; private set; }
        public bool Mandatory { get; private set; }
        public string Default { get; private set; }
        public bool Read { get; private set; }
        public bool Save { get; private set; }
        public bool Hide { get; private set; }

        public QuestionnaireQuestion(string questionnaireQuestionRecordId, string questionnaireID, int questionNumber, string label, bool multiple, string infoAreaId,
            string fieldId, int followUpNumber, bool newSection, bool mandatory, string def, bool read, bool save, bool hide)
        {
            QuestionnaireQuestionRecordId = questionnaireQuestionRecordId;
            QuestionnaireID = questionnaireID;
            QuestionNumber = questionNumber;
            Label = label;
            Multiple = multiple;
            InfoAreaId = infoAreaId;
            FieldId = fieldId;
            FollowUpNumber = followUpNumber;
            NewSection = newSection;
            Mandatory = mandatory;
            Default = def;
            Read = read;
            Save = save;
            Hide = hide;
        }
    }
}
