using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
    public class QueryRequest
    {
        public string MainTable { get; set; }
        public List<SqlQueryField> Fields { get; set; }
        public Dictionary<string, SqlQueryJoin> Joins { get; set; }
        public List<SqlQueryCondition> OrConditions { get; set; }
        public List<SqlQueryCondition> AndConditions { get; set; }
        public List<SqlQuerySortField> SortFields { get; set; }

        public int MaxResults { get; set; }

        public QueryRequest()
        {
        }
    }
}
