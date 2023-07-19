using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Questionnaire;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuestionnaireAnswerDataService : IContentServiceBase
    {
        public List<QuestionnaireAnswerData> GetQuestionnaireQuestionsAnswersData(int questionNumber);
        public Task<ModifyRecordResult> SaveAnswerData(string _questionNumber, string _answerNumber, string answer,
            string oldAnswer, string recordId, string questionRecordId, string answerRecordId, CancellationToken cancellationToken);
        Task<ModifyRecordResult> DeleteAnswerData(string recordId, CancellationToken cancellationToken);
    }
}
