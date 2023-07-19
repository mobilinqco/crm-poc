using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class QuestionnaireEditTemplate : ActionTemplateBase
    {

        public QuestionnaireEditTemplate(ViewReference viewReference) : base(viewReference)
        {

        }

        public string ConfirmButtonName()
        {
            return GetValue("ConfirmButtonName");
        }

        public string ConfirmedFilterName()
        {
            return GetValue("ConfirmedFilterName");
        }

        public string SurveySearchAndListName()
        {
            return GetValue("SurveySearchAndListName");
        }

        public string SurveyAnswerSearchAndListName()
        {
            return GetValue("SurveyAnswerSearchAndListName");
        }
    }
}
