using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Questionnaire;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuestionnaireContentService : IContentServiceBase
    {
        public string QuestionnaireLabel { get; }
        public List<QuestionnaireQuestionSection> GetQuestionnaireQuestionSections();
        public bool IsQuestionnaireFinalized();
        public bool IsQuestionnaireReadOnly();
        public Task<ModifyRecordResult> SaveAnswerData(string _questionNumber, string _answerNumber, string answer,
                    string oldAnswer, string recordId, string questionRecordId, string answerRecordId, CancellationToken cancellationToken);
        public Task<ModifyRecordResult> DeleteAnswerData(string recordId, CancellationToken cancellationToken);
        public Task<ModifyRecordResult> SaveQuestionnaireState(CancellationToken cancellationToken);
    }
}
