using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class QueryActionTemplate : ActionTemplateBase
    {
        // Query Parameters:
        // Query
        // CopySourceFieldGroupName
        // CopySourceRecordId
        // AdditionalParameters
        // Options
        public QueryActionTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string Query()
        {
            return GetValue("Query");
        }

        public string CopySourceFieldGroupName()
        {
            return GetValue("CopySourceFieldGroupName");
        }

        public string CopySourceRecordId()
        {
            return GetValue("CopySourceRecordId");
        }

        public int MaxResults()
        {
            object val = GetOption("MaxResults");
            if (val != null)
            {
                if (val is int)
                {
                    return (int)val;
                }
            }

            return 100;
        }

        public bool FixedSumRow()
        {
            object val = GetOption("FixedSumRow");
            if (val != null)
            {
                if (val is bool)
                {
                    return (bool)val;
                }
            }

            return false;
        }

        public bool ShowEmpty()
        {
            object val = GetOption("ShowEmpty");
            if (val != null)
            {
                if (val is bool)
                {
                    return (bool)val;
                }
            }

            return false;
        }
    }
}

