using ACRM.mobile.Domain.Application.Questionnaire;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuestionnaireQuestionsService : IContentServiceBase
    {
        public List<QuestionnaireQuestion> GetQuestions(string questionnaireId);
    }
}
