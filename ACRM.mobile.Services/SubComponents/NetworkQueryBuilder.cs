using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.DataAccess.Local.CrmDataContext;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using Newtonsoft.Json;

namespace ACRM.mobile.Services.SubComponents
{
    public class NetworkQueryBuilder : QueryBuilderBase
    {
        public NetworkQueryBuilder(ICrmDataFieldResolver crmDataFieldResolver,
            ICacheService cacheService, ILogService logService) : base(crmDataFieldResolver, cacheService, logService)
        {

        }

        public SubNode GetQueryDetails(DataRequestDetails requestDetails,
            string recordId = null, ParentLink parentLink = null, int maxResults = 100)
        {
            Dictionary<string, string> fieldids = new Dictionary<string, string>();

            SubNode mainTable = new SubNode(requestDetails.TableInfo.InfoAreaId, "", requestDetails.TableInfo.InfoAreaId, -1);

            if (requestDetails?.Fields?.Count == 0)
            {
                var fi = requestDetails.TableInfo.Fields[0];
                if (fi != null)
                {
                    mainTable.AddField(fi.FieldId, fi.FieldDbName());
                }
            }

            requestDetails.Fields?.ForEach(field =>
            {
                //_logService.LogDebug($"Processing field {field}");
                if (field.InfoAreaId == requestDetails.TableInfo.InfoAreaId)
                {
                    FieldInfo fi = requestDetails.TableInfo.Fields.Find(f => f.FieldId == field.FieldId);
                    if (fi != null)
                    {
                        mainTable.AddField(fi.FieldId, fi.FieldDbName());
                    }
                    else
                    {
                        _logService.LogDebug($"Field {field} not found in the table definition");
                    }
                }
                else
                {
                    AddToSubTable(requestDetails, field, mainTable, "PLUS");
                }
            });

            if (requestDetails.SortFields != null)
            {
                foreach(var field in requestDetails.SortFields.OrderBy(f => f.OrderId))
                {
                    if (field.InfoAreaId == requestDetails.TableInfo.InfoAreaId)
                    {
                        FieldInfo fi = requestDetails.TableInfo.Fields.Find(f => f.FieldId == field.FieldIndex);
                        if (fi != null)
                        {
                            mainTable.AddSortField(fi.FieldId, !field.Descending, field.InfoAreaId);
                        }
                    }
                    else
                    {
                        if (field.FieldIndex > 0)
                        {
                            mainTable?.AddSortField(field.FieldIndex, !field.Descending, field.InfoAreaId);
                        }
                    }
                }
            }

            if (requestDetails.SearchFields != null && !string.IsNullOrWhiteSpace(requestDetails.SearchValue))
            {
                bool hasLinkedFields = requestDetails.SearchFields.Any(field => field.InfoAreaId != requestDetails.TableInfo.InfoAreaId);
                string searchValue = "*" + requestDetails.SearchValue.Replace(' ', '*') + "*";
                NodeCondition nodeConditionTree = new NodeCondition("OR");

                requestDetails.SearchFields.ForEach(field =>
                {
                    if(hasLinkedFields)
                    {
                        AddToSubTable(requestDetails, field, mainTable, "HAVINGOPTIONAL", searchValue);
                    }
                    else
                    {
                        FieldInfo fi = requestDetails.TableInfo.Fields.Find(f => f.FieldId == field.FieldId);
                        if(fi != null)
                        {
                            nodeConditionTree.AddSubCondition(new NodeCondition(field.InfoAreaId, field.FieldId, searchValue));
                        }
                    }
                });

                if (!hasLinkedFields)
                {
                    mainTable.AddCondition(nodeConditionTree);
                }
            }

            if(requestDetails.Filters != null)
            {
                foreach (var filter in requestDetails.Filters)
                {
                    if (filter.RootTable != null)
                    {
                        AddFilterDetails(filter.RootTable, mainTable);
                    }
                }
            }

            return mainTable;
        }

        private void AddFilterDetails(QueryTable queryTable, SubNode node)
        {
            if (queryTable.InfoAreaId.Equals(node.InfoAreaId))
            {
                if(queryTable.ExpandedConditions != null)
                {
                    node.AddCondition(queryTable.ExpandedConditions, "AND", false);
                }

                if (queryTable.SubTables != null && queryTable.SubTables.Count > 0)
                {
                    foreach(QueryTable subTable in queryTable.SubTables)
                    {
                        AddSubTable(node, subTable);
                    }
                }
            }
            else
            {
                AddSubTable(node, queryTable);
            }
        }

        private void AddSubTable(SubNode node, QueryTable subTable)
        {
            string relation = subTable.ParentRelation;
            if (relation.ToUpper().Equals("WITH"))
            {
                relation = "HAVING";
            }
            else if (relation.ToUpper().Equals("WITHOPTIONAL"))
            {
                relation = "HAVINGOPTIONAL";
            }

            string tableAlias = "CRM_" + subTable.InfoAreaId + "_" + subTable.LinkId + "_" + relation.ToUpper();
            SubNode subNode = node.GetSubNode(tableAlias);
            if (subNode == null)
            {
                subNode = new SubNode(tableAlias, relation.ToUpper(), subTable.InfoAreaId, subTable.LinkId);
                node.AddSubNode(subNode);
            }
            AddFilterDetails(subTable, subNode);
        }

        private void AddToSubTable(DataRequestDetails requestDetails, FieldControlField field, SubNode mainTable, string relation, string searchValue="")
        {
            if(string.IsNullOrWhiteSpace(relation))
            {
                return;
            }

            try
            {
                (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(requestDetails.TableInfo, field.InfoAreaId, field.LinkId);

                if (linkInfo != null)
                {
                    string tableAlias = relatedTable.DbStorageName() + "_" + linkInfo.LinkId + "_" + relation.ToUpper();

                    SubNode subTable = mainTable.GetSubNode(tableAlias);
                    if (subTable == null)
                    {
                        subTable = new SubNode(tableAlias, relation.ToUpper(), relatedTable.InfoAreaId, linkInfo.LinkId);
                        mainTable.AddSubNode(subTable);
                    }

                    FieldInfo lfi = relatedTable.Fields.Find(f => f.FieldId == field.FieldId);
                    if (lfi != null)
                    {
                        if (relation.ToUpper().Equals("PLUS"))
                        {
                            subTable.AddField(lfi.FieldId, field.QueryFieldName(true));
                        }
                        else if (relation.ToUpper().Equals("HAVINGOPTIONAL"))
                        {
                            subTable.AddCondition(new NodeCondition(field.InfoAreaId, field.FieldId, searchValue), "OR");
                        }
                    }
                    
                }
                else
                {
                    _logService.LogError($"We have encountered a field with no proper links and no parent info area. Field InfoAre: {field.ToString()}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"We have encountered an exception when processing: {field.ToString()}");
                _logService.LogError($"Exception: {ex.Message}");
            }
        }

        public List<string> GetModifyRequest(TableInfo tableInfo, OfflineRequest request)
        {
            List<string> requestDef = new List<string>();
            request.Records.ForEach(record =>
            {
                _logService.LogDebug($"Processing record {record}");

                string recordId = record.RecordId.FormatedRecordId(record.InfoAreaId);

                List<object> recDef = new List<object>
                {
                    record.Mode,
                    recordId
                };

                if(record.RecordFields != null && record.RecordFields.Count > 0)
                {
                    List<object> fields = new List<object>();
                    record.RecordFields.ForEach(field =>
                    {
                        //_logService.LogDebug($"Processing field {field}");
                        fields.Add(new List<object> { field.FieldId,
                            string.IsNullOrEmpty(field.NewValue) ? "" : field.NewValue,
                            string.IsNullOrEmpty(field.OldValue) ? "" : field.NewValue });
                    });
                    recDef.Add(fields);
                }
                else
                {
                    recDef.Add(null);
                }

                if (record.RecordLinks != null && record.RecordLinks.Count > 0)
                {
                    List<object> links = new List<object>();
                    record.RecordLinks.ForEach(link =>
                    {
                        //_logService.LogDebug($"Processing field {link}");
                        if (!link.RecordId.Contains("."))
                        {
                            links.Add(new List<object> { $"{link.InfoAreaId}.{link.RecordId}", link.LinkId });
                        }
                        else
                        {
                            links.Add(new List<object> { $"{link.RecordId}", link.LinkId });
                        }
                    });
                    recDef.Add(links);
                }
                else
                {
                    recDef.Add(null);
                }

                recDef.Add(null);

                requestDef.Add(JsonConvert.SerializeObject(recDef));
            });

            return requestDef;
        }

        public void ResolveParentLinkId(TableInfo tableInfo, ParentLink parentLink)
        {
            if(!string.IsNullOrWhiteSpace(parentLink.ParentInfoAreaId) && parentLink.LinkId < 0)
            {
                (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tableInfo, parentLink.ParentInfoAreaId, parentLink.LinkId);
                parentLink.LinkId = linkInfo.LinkId;
            }
        }
    }
}
