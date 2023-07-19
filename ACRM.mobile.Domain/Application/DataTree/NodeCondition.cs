using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;


namespace ACRM.mobile.Domain.Application.DataTree
{
    [Serializable]
    public class NodeCondition
    {
        public string InfoAreaId { private set; get; }
        public int FieldId { private set; get; }
        public string FieldValue { private set; get; }
        public List<string> FieldValues { set; get; }
        public string CompareOperator { private set; get; }
        public string FunctionName { private set; get; }

        public List<NodeCondition> Conditions { set; get; }
        public string Relation { private set; get; }

        [JsonIgnore]
        public List<NodeCondition> ChangedSubConditions { private set; get; }

        public bool IsLeaf()
        {
            if(!string.IsNullOrWhiteSpace(Relation) && Relation.ToLower().Equals("leaf"))
            {
                return true;
            }

            return false;
        }

        public bool IsOrCondition()
        {
            if (!string.IsNullOrWhiteSpace(Relation) && Relation.ToLower().Equals("or"))
            {
                return true;
            }

            return false;
        }

        public string FirstValue()
        {
            if(!string.IsNullOrEmpty(FieldValue))
            {
                return FieldValue;
            }

            if(FieldValues != null && FieldValues.Count > 0)
            {
                return FieldValues[0];
            }

            return string.Empty;
        }

        public string FieldIdentification()
        {
            return InfoAreaId + "." + FieldId;
        }

        public void UpdateWithTableInfoAreaId(string infoAreaId)
        {
            InfoAreaId = infoAreaId;
        }

        public NodeCondition(string relation)
        {
            Relation = relation;
            Conditions = new List<NodeCondition>();
        }

        public NodeCondition(string relation, NodeCondition left, NodeCondition right): this(relation)
        {
            Conditions.Add(left);
            Conditions.Add(right);
        }


        public NodeCondition(string infoAreaId, int fieldId, string value) : this(infoAreaId, fieldId, "=", value) { }

        public NodeCondition(string infoAreaId, int fieldId, string compareOperator, string value)
        {
            Relation = "LEAF";
            InfoAreaId = infoAreaId;
            FieldId = fieldId;
            CompareOperator = compareOperator;
            FieldValue = value;
            FieldValues = new List<string> { value };
        }

        public NodeCondition(string infoAreaId, int fieldId, string compareOperator, List<string> values)
        {
            Relation = "LEAF";
            InfoAreaId = infoAreaId;
            FieldId = fieldId;
            CompareOperator = compareOperator;
            if (values.Count == 1)
            {
                FieldValue = values[0];
                FieldValues = new List<string> { FieldValue };
            }
            else
            {
                if (values.Count > 0)
                {
                    FieldValue = values[0];
                    FieldValues = values;
                }
                else
                {
                    FieldValue = "";
                    FieldValues = new List<string>();
                }
            }
        }

        public NodeCondition(NodeCondition parent, List<NodeCondition> changedSubConditions)
        {
            Relation = parent.Relation;
            InfoAreaId = parent.InfoAreaId;
            FieldId = parent.FieldId;
            CompareOperator = parent.CompareOperator;
            FunctionName = parent.FunctionName;

            if (parent.FieldValues != null)
            {
                if (parent.FieldValues.Count == 1)
                {
                    FieldValue = parent.FieldValues[0];
                    FieldValues = new List<string> { FieldValue };
                }
                else
                {
                    if (parent.FieldValues.Count > 0)
                    {
                        FieldValue = parent.FieldValue;
                        FieldValues = parent.FieldValues;
                    }
                    else
                    {
                        FieldValue = "";
                        FieldValues = new List<string>();
                    }
                }
            }
            else
            {
                FieldValue = parent.FieldValue;
                FieldValues = new List<string> { parent.FieldValue };
            }


            Conditions = parent.Conditions;
            ChangedSubConditions = ChangedSubConditions;
        }


        public NodeCondition(JArray definition, string infoAreaId)
        {
            if (definition == null || definition.Count == 0)
            {
                Relation = "EMPTY";
            }
            else
            {
                Relation = definition[0].ToObject<string>();
                if (!string.IsNullOrEmpty(Relation))
                {
                    if (Relation.Equals("LEAF"))
                    {
                        FieldId = definition[1].ToObject<int>();
                        CompareOperator = definition[2].ToObject<string>();
                        FunctionName = definition[3].ToObject<string>();
                        FieldValues = definition[4].ToObject<List<string>>();
                        InfoAreaId = infoAreaId;
                    }
                    else
                    {
                        List<NodeCondition> subConditions = new List<NodeCondition>();
                        for (int i = 1; i < definition.Count; i++)
                        {
                            subConditions.Add(new NodeCondition(definition[i].ToObject<JArray>(), infoAreaId));
                        }

                        Conditions = subConditions;
                    }
                }
            }
        }

        public string GetJsonRepresentation()
        {
            return PrepareJArray().ToString(Newtonsoft.Json.Formatting.None);
        }

        private JArray PrepareJArray()
        {
            JArray jArray = new JArray();
            jArray.Add(Relation);
            if (!Relation.Equals("LEAF"))
            {
                foreach (var node in Conditions)
                {
                    jArray.Add(node.PrepareJArray());
                }
            }
            else
            {
                jArray.Add(FieldId);
                jArray.Add(CompareOperator);
                jArray.Add(FunctionName);
                jArray.Add(new JArray(FieldValues));
            }
            return jArray;
        }

        public void AddSubCondition(NodeCondition condition)
        {
            Conditions.Add(condition);
        }

        public NodeCondition AppendCondition(NodeCondition condition)
        {
            if(Relation.ToLower().Equals("and") || Relation.ToLower().Equals("or"))
            {
                AddSubCondition(condition);
                return this;
            }

            return new NodeCondition(Relation, this, condition);
        }

        public List<object> ToArrayRepresentation()
        {
            if (IsLeaf())
            {
                if (FieldValues.Count > 0)
                {
                    return new List<object> { Relation, FieldId, CompareOperator, FieldValues };
                }

                return new List<object> { Relation, FieldId, CompareOperator, new List<object> { FieldValue } };
            }
            else
            {
                List<object> subConditions = new List<object>();
                foreach (NodeCondition cond in Conditions)
                {
                    subConditions.Add(cond.ToArrayRepresentation());
                }

                return new List<object> { Relation, subConditions };
            }
        }

        public NodeCondition GetCondition(int fieldId)
        {
            if (Conditions == null || Conditions.Count == 0)
            {
                return FieldId == fieldId ? this : null;
            }

            List<NodeCondition> changedSubConditions = null;
            for(int i = 0; i < Conditions.Count; i++)
            {
                var currentCondition = Conditions[i];
                var conditionForFieldId = currentCondition.GetCondition(fieldId);

                if (changedSubConditions != null || conditionForFieldId != currentCondition)
                {
                    if(changedSubConditions == null)
                    {
                        changedSubConditions = new List<NodeCondition>();
                        for (int j = 0; j < i; j++)
                        {
                            changedSubConditions.Add(Conditions[j]);
                        }
                    }
                    
                    if(conditionForFieldId != null)
                    {
                        changedSubConditions.Add(conditionForFieldId);
                    }
                }
            }

            if(changedSubConditions == null)
            {
                return this;
            }

            if(changedSubConditions.Count > 0)
            {
                return new NodeCondition(this, changedSubConditions);
            }

            return null;
        }
    }
}
