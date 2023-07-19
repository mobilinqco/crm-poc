using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public class QueryCondition
    {
        public int Id { get; set; }
        public string Relation { get; set; }
        public string InfoAreaId { get; set; }
        public int FieldId { get; set; }
        public string CompareOperator { get; set; }
        public string FunctionName { get; set; }
        public string FieldValues { get; set; }
        public List<QueryCondition> SubConditions { get; set; }

        public QueryCondition()
        {
        }
    }
}
