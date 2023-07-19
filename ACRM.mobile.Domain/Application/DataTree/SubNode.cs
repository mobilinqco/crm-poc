using System;
using System.Collections.Generic;
using System.Linq;

namespace ACRM.mobile.Domain.Application.DataTree
{
    public class SubNode
    {
        private string _name;
        private string _parentRelation;
        public string InfoAreaId { private set; get; }
        private int _linkId;
        private Dictionary<int, string> _fieldIds;
        private Dictionary<string, SubNode> _subNodes;

        private List<object> _sortFieldIds;
        private List<object> _searchFieldIds;
        private List<List<object>> _filters;

        private NodeCondition _condition;

        public string Name => _name;
        public List<object> SortFields => _sortFieldIds;

        public SubNode(string name, string parentRelation, string infoAreaId, int linkId)
        {
            _name = name;
            _parentRelation = parentRelation;
            InfoAreaId = infoAreaId;
            _linkId = linkId;
            _fieldIds = new Dictionary<int, string>();
            _subNodes = new Dictionary<string, SubNode>();
            _sortFieldIds = new List<object>();
            _searchFieldIds = new List<object>();
            _filters = new List<List<object>>();
        }

        public void AddField(int fieldId, string fieldAlias)
        {
            if(!_fieldIds.ContainsKey(fieldId))
            {
                _fieldIds.Add(fieldId, fieldAlias);
            }
        }

        public SubNode GetSubNode(string subNodeName)
        {
            if (_subNodes.ContainsKey(subNodeName))
            {
                return _subNodes[subNodeName];
            }
            else
            {
                foreach(SubNode subNode in SubNodes())
                {
                    SubNode subSubNode = subNode.GetSubNode(subNodeName);
                    if ( subSubNode != null)
                    {
                        return subSubNode;
                    }
                }
            }
            return null;
        }

        public void AddSubNode(SubNode subNode)
        {
            if (!_subNodes.ContainsKey(subNode.Name))
            {
                _subNodes.Add(subNode.Name, subNode);
            }
        }

        public void AddSortField(int fieldId, bool ascending, string infoAreaId )
        {
            _sortFieldIds.Add(new List<object> { fieldId, !ascending, infoAreaId });
        }

        public void AddFilter(List<object> filterDefinition)
        {
            _filters.Add(filterDefinition);

        }

        public List<object> ToArrayRepresentation()
        {
            var fields = _fieldIds.Keys.ToList();
            var subnodes = new List<object>();

            foreach (var node in _subNodes)
            {
                subnodes.Add(node.Value.ToArrayRepresentation());
            }

            return new List<object>
            {
                _parentRelation,
                InfoAreaId,
                _linkId,
                fields.Count > 0 ? _fieldIds.Keys.ToList() : null,
                _condition != null ?  _condition.ToArrayRepresentation() : null,
                subnodes.Count > 0 ? subnodes : null
            };
        }

        public List<SubNode> SubNodes()
        {
            return _subNodes.Values.ToList();
        }

        public List<string> OnlySubNodeFieldsAlias()
        {
            return _fieldIds.Values.ToList();
        }

        public List<string> FieldsAliasList()
        {
            var fields = _fieldIds.Values.ToList();

            foreach (var node in _subNodes)
            {
                fields.AddRange(node.Value.FieldsAliasList());
            }

            return fields;
        }

        public IEnumerable<string> SubNodesRecIdAliasList()
        {
            var recIds = new List<string>();

            foreach (var node in _subNodes)
            {
                if(node.Value.HasOutRecordId())
                {
                    recIds.Add(node.Value.RecIdAlias());
                }
            }

            return recIds.Distinct();
        }

        public bool HasOutRecordId()
        {
            if(!string.IsNullOrWhiteSpace(_parentRelation) &&
                _parentRelation.ToUpper().Equals("PLUS"))
            {
                return true;
            }
            return false;
        }

        public string RecIdAlias()
        {
            return InfoAreaId + "_" + _linkId + "_recId";
        }

        public NodeCondition AddCondition(NodeCondition condition, string defaultRelation = "AND", bool isSubcondition = true)
        {
            if (_condition == null)
            {
                _condition = condition;
            }
            else
            {
                if (condition.IsLeaf())
                {
                    if(_condition.IsLeaf())
                    {
                        _condition = new NodeCondition(defaultRelation, _condition, condition);
                    }
                    else
                    {
                        if(isSubcondition)
                        {
                            _condition.AddSubCondition(condition);
                        }
                        else
                        {
                            _condition = new NodeCondition(defaultRelation, _condition, condition);
                        }
                        
                    }
                }
                else
                {
                    if(_condition.IsLeaf())
                    {
                        if (isSubcondition)
                        {
                            condition.AddSubCondition(_condition);
                            _condition = condition;
                        }
                        else
                        {
                            _condition = new NodeCondition(defaultRelation, _condition, condition);
                        }
                    }
                    else
                    {
                        _condition = new NodeCondition(defaultRelation, _condition, condition);
                    }
                }
            }

            return _condition;
        }
    }
}
