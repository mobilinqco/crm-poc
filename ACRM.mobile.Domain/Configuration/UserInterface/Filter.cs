using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [Serializable]
    [JsonConverter(typeof(JsonArrayToObjectConverter<Filter>))]
    public class Filter: QueryFilterBase
    {
        [JsonArrayIndex(1)]
        public string DisplayName { get; set; }
        [JsonArrayIndex(2)]
        public string InfoAreaId { get; set; }

        public Filter()
        {
        }

        public List<NodeCondition> GetParValueNodes()
        {
            List<NodeCondition> nodeConditions = new List<NodeCondition>();
            AddParValueNodes(this.RootTable, nodeConditions);
            return nodeConditions;
        }

        private void AddParValueNodes(QueryTable rootTable, List<NodeCondition> nodeConditions)
        {
            if (rootTable != null)
            {
                AddParValueNodes(rootTable.ExpandedConditions, nodeConditions);

            }

            if (rootTable.SubTables?.Count > 0)
            {
                foreach (var subTable in rootTable.SubTables)
                {
                    AddParValueNodes(subTable, nodeConditions);
                }

            }

        }
        private void AddParValueNodes(NodeCondition expandedConditions, List<NodeCondition> nodeConditions)
        {
            if (expandedConditions != null)
            {
                if (expandedConditions.Conditions?.Count > 0)
                {
                    foreach (var condition in expandedConditions.Conditions)
                    {
                        AddParValueNodes(condition, nodeConditions);
                    }
                }
                else
                {
                    if (expandedConditions.FieldValues.Count > 0 && expandedConditions.FieldValues[0].StartsWith("$parValue"))
                    {
                        nodeConditions.Add(expandedConditions);
                    }
                }
            }
        }
    }
}
