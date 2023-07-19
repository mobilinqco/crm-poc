using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class QuestionnaireTemplate : ActionTemplateBase
    {

        public QuestionnaireTemplate(ViewReference viewReference) : base(viewReference)
        {

        }

        public bool QuestionnaireReadOnly()
        {
            bool.TryParse(GetValue("QuestionnaireReadOnly"), out bool questionnaireReadOnly);
            return questionnaireReadOnly;
        }

        public string ConfirmButtonName()
        {
            return GetValue("ConfirmButtonName");
        }

        public string SurveySearchAndListName()
        {
            return GetValue("SurveySearchAndListName");
        }
    }
}
