using ACRM.mobile.Domain.Application.Questionnaire;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuestionnaireAnswersService : IContentServiceBase
    {
        public Dictionary<int, List<QuestionnaireAnswer>> GetQuestionnaireQuestionsAnswers(string questionnaireId);
    }
}
