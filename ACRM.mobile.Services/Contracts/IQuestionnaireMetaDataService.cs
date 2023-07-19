using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuestionnaireMetaDataService : IContentServiceBase
    {
        public string QuestionnaireLabel { get; }
        public string GetQuestionnaireModelRecordId();
    }
}
