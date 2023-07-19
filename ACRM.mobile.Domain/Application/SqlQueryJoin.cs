using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application
{
    public class SqlQueryJoin
    {
        public string TableName { get; set; }
        public string TableAlias { get; set; }

        public List<SqlQueryCondition> Conditions { get; set; }

        public SqlQueryJoin()
        {
        }
    }
}