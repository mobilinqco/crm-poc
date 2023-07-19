using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public class EditTriggerUnit
    {
        public int Key { get; set; }
        public List<string> VariableParameters { get; set; }
        public List<string> FixedParameters { get; set; }
        public Dictionary<string, string> Evaluations { get; set; }

        public EditTriggerUnit()
        {
            VariableParameters = new List<string>();
            Evaluations = new Dictionary<string, string>();
            FixedParameters = new List<string>();
        }

        public List<string> getParameters(Dictionary<string, string> parameters)
        {
            List<string> results = new List<string>();

            foreach(var item in VariableParameters)
            {
                if(parameters.ContainsKey(item))
                {
                    string value = parameters[item];
                    if(string.IsNullOrWhiteSpace(value))
                    {
                        value = "";
                    }

                    results.Add(value);
                }
                else
                {
                    results.Add(string.Empty);
                }
            }
            return results;
        }
    }
}
