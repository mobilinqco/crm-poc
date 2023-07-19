using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [Serializable]
    [JsonConverter(typeof(JsonArrayToObjectConverter<QueryTable>))]
    public class QueryTable
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public int LinkId { get; set; }
        [JsonArrayIndex(2)]
        public string ParentRelation { get; set; }
        [JsonArrayIndex(3)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string Conditions { get; set; }
        [JsonArrayIndex(4)]
        public List<QueryTable> SubTables { get; set; }
        [JsonArrayIndex(5)]
        public string Alias { get; set; }

        [JsonIgnore]
        [NotMapped]
        public NodeCondition ExpandedConditions { get; set; }

        public QueryTable()
        {
        }

        private List<object> ToArrayRepresentation()
        {
            List<object> result = new List<object>();

            if(string.IsNullOrEmpty(ParentRelation))
            {
                ConditionToArrayRepresentation(result);
                SubTablesToArrayRepresentation(result);
            }
            else
            {
                result.Add(ParentRelation);
                result.Add(InfoAreaId);
                result.Add(LinkId);
                ConditionToArrayRepresentation(result);
                SubTablesToArrayRepresentation(result);
                result.Add(null);
            }

            return result;
        }

        private void SubTablesToArrayRepresentation(List<object> result)
        {
            if (SubTables != null)
            {
                foreach (var table in SubTables)
                {
                    result.Add(table.ToArrayRepresentation());
                }
            }
        }

        private void ConditionToArrayRepresentation(List<object> result)
        {
            if (ExpandedConditions == null)
            {
                result.Add(null);
            }
            else
            {
                result.Add(ExpandedConditions.GetJsonRepresentation());
            }
        }

        public List<NodeCondition> GetCondition(int fieldId, string infoAreaId)
        {
            List<NodeCondition> conditions = new List<NodeCondition>();
            if(ExpandedConditions != null)
            {
                NodeCondition condition = ExpandedConditions.GetCondition(fieldId);
                if (condition != null)
                {
                    condition.UpdateWithTableInfoAreaId(infoAreaId);
                    conditions.Add(condition);
                }
            }

            if(SubTables != null)
            {
                foreach (var subTable in SubTables)
                {
                    if (infoAreaId.Equals(subTable.InfoAreaId))
                    {
                        conditions.AddRange(subTable.GetCondition(fieldId, infoAreaId));
                    }
                }
            }

            return conditions;
        }

        public bool IsExistsCondition()
        {
            return !string.IsNullOrWhiteSpace(ParentRelation)
                   && (ParentRelation.StartsWith("WITHOUT")
                   || ParentRelation.StartsWith("HAVING")
                   || (SubTables != null && ParentRelation.StartsWith("WITH")));
        }
    }
}
