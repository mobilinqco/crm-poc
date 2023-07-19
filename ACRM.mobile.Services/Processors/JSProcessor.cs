using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using Jint;

namespace ACRM.mobile.Services.Processors
{
    public class JSProcessor : IJSProcessor
    {
        private readonly ILogService _logService;
        private readonly ISessionContext _sessionContext;
        private readonly IConfigurationService _configurationService;
        private Engine engine = null;

        public JSProcessor(IConfigurationService configurationService,
            ISessionContext sessionContext,
            ILogService logService)
        {
            _configurationService = configurationService;
            _sessionContext = sessionContext;
            _logService = logService;
        }

        public async Task InitJSRuntime(CancellationToken cancellationToken)
        {
            if (engine == null)
            {
                WebConfigValue globalJSFiles = _configurationService.GetConfigValue("System.JavascriptGlobals");
                List<string> jsFileNames = new List<string>();
                if(!string.IsNullOrWhiteSpace(globalJSFiles.Value))
                {
                    string[] resourceNames = globalJSFiles.Value.Split(',');
                    foreach(var resourceName in resourceNames)
                    {
                        var resource = _configurationService.GetConfigResource(resourceName);
                        if(!string.IsNullOrWhiteSpace(resource?.FileName))
                        {
                            jsFileNames.Add(resource.FileName);
                        }
                    }
                }
                else
                {
                    jsFileNames.Add("CRMpadFunctions.js");
                }

                engine = new Engine();
                foreach (string fileName in jsFileNames)
                {
                    var jsRuntimeFile = _sessionContext.ResourcePath(fileName);
                    if (!string.IsNullOrWhiteSpace(jsRuntimeFile) && File.Exists(jsRuntimeFile))
                    {
                        using (StreamReader streamReader = new StreamReader(jsRuntimeFile))
                        {
                            string script = await streamReader.ReadToEndAsync();
                            try
                            {
                                engine.Execute(script);
                            }
                            catch (Exception ex)
                            {
                                _logService.LogError("Error processing jsRuntimeFile");
                            }
                            streamReader.Close();
                        }
                    }
                }
            }
        }

        public async Task<string> ExecuteJSEvaluation(string evaluationCode, EditTriggerUnit triggerUnit, Dictionary<string, string> allParameters, CancellationToken token)
        {
            // Set Variable Parameters
            var parameters = triggerUnit.getParameters(allParameters);
            if (parameters?.Count > 0)
            {
                StringBuilder strParams = new StringBuilder("const v = [");
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        strParams.Append($", ");
                    }

                    strParams.Append($"\"{parameters[i]}\"");
                }
                strParams.Append("];");
                var paraArrayStr = strParams.ToString();
                engine.Execute(paraArrayStr);
                object result = engine.GetValue("v");
            }

            // Set Fixed Parameters
            if (triggerUnit.FixedParameters?.Count > 0)
            {

                StringBuilder strParams = new StringBuilder("const f = [");
                for (int i = 0; i < triggerUnit.FixedParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        strParams.Append($", ");
                    }

                    strParams.Append($"\"{triggerUnit.FixedParameters[i]}\"");

                }
                strParams.Append("];");
                var paraArrayStr = strParams.ToString();
                engine.Execute(paraArrayStr);
                object result = engine.GetValue("f");
            }

            try
            {
                engine.Execute(evaluationCode);
            }
            catch (Exception ex)
            {
                return "Trigger filter evaluation error";
            }

            var value = engine.GetCompletionValue();
            return value.ToString();
        }

        public string ExecuteJSEvaluation(string evaluationCode, List<string> fixedParameters, List<string> variableParameters)
        {
            // Set Variable Parameters

            if (variableParameters?.Count > 0)
            {
                StringBuilder strParams = new StringBuilder("const v = [");
                for (int i = 0; i < variableParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        strParams.Append($", ");
                    }

                    strParams.Append($"\"{variableParameters[i]}\"");
                }
                strParams.Append("];");
                var paraArrayStr = strParams.ToString();
                engine.Execute(paraArrayStr);
                object result = engine.GetValue("v");
            }

            // Set Fixed Parameters
            if (fixedParameters?.Count > 0)
            {

                StringBuilder strParams = new StringBuilder("const f = [");
                for (int i = 0; i < fixedParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        strParams.Append($", ");
                    }

                    strParams.Append($"\"{fixedParameters[i]}\"");

                }
                strParams.Append("];");
                var paraArrayStr = strParams.ToString();
                engine.Execute(paraArrayStr);
                object result = engine.GetValue("f");
            }

            try
            {
                engine.Execute(evaluationCode);
            }
            catch (Exception ex)
            {
                return "Trigger filter evaluation error";
            }

            var value = engine.GetCompletionValue();
            return value.ToString()?.ToLower();
        }
    }
}
