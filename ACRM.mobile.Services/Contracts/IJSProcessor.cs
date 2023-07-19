using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IJSProcessor
    {
        Task InitJSRuntime(CancellationToken cancellationToken);
        Task<string> ExecuteJSEvaluation(string evaluationCode, EditTriggerUnit triggerUnit, Dictionary<string, string> allParameters, CancellationToken token);
        string ExecuteJSEvaluation(string result, List<string> fixedParameters, List<string> variableParameters);
    }
}
