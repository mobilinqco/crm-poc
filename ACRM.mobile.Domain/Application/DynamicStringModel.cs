using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application
{
    public class DynamicStringModel
    {

        public Dictionary<string, string> Values { get; private set; }

        public DynamicStringModel(Dictionary<string, string> values)
        {
            Values = values;
        }

        public override string ToString()
        {
            string valuesString = "";
            foreach(KeyValuePair<string, string> keyValue in Values)
            {
                valuesString += $"{keyValue.Key}: {keyValue.Value}; ";
            }
            return valuesString;
        }
    }
}
