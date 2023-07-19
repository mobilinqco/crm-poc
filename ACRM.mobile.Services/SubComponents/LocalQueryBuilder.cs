using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACRM.mobile.DataAccess.Local.CrmDataContext;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using Microsoft.Extensions.Primitives;

namespace ACRM.mobile.Services.SubComponents
{
    public class LocalQueryBuilder: QueryBuilderBase
    {

        public LocalQueryBuilder(ICrmDataFieldResolver crmDataFieldResolver,
            ICacheService cacheService, ILogService logService) : base(crmDataFieldResolver, cacheService, logService)
        {
        }

        public QueryRequest GetQueryDetails(DataRequestDetails requestDetails,
            ParentLink parentLink = null, int maxResults = 100)
        {
            List<SqlQueryField> sqlFields = new List<SqlQueryField>();
            Dictionary<string, SqlQueryJoin> joins = new Dictionary<string, SqlQueryJoin>();
            
            List<SqlQueryCondition> sqlOrConditions = new List<SqlQueryCondition>();
            List<SqlQueryCondition> sqlAndConditions = new List<SqlQueryCondition>();
            List<SqlQuerySortField> sqlSortFields = new List<SqlQuerySortField>();

            ExtractFields(requestDetails.TableInfo, requestDetails.Fields, sqlFields, joins);
            ExtractSearchFields(requestDetails.TableInfo, requestDetails.SearchFields, requestDetails.SearchValue, joins, sqlOrConditions);
            ExtractSortFields(requestDetails.TableInfo, requestDetails.SortFields, sqlSortFields, joins);

            if (requestDetails.RecordId != null)
            {
                sqlAndConditions.Add(new SqlQueryCondition
                {
                    Left = requestDetails.TableInfo.DbStorageName() + ".recid",
                    Operator = " = ",
                    Right = "'" + requestDetails.RecordId + "'"
                });
            }

            if (parentLink != null)
            {
                List<SqlQueryCondition> parentLinkConditions = GetParentCondition(requestDetails.TableInfo, parentLink);
                if (parentLinkConditions != null && parentLinkConditions.Count > 0)
                {
                    sqlAndConditions.AddRange(parentLinkConditions);
                }
            }

            if(requestDetails.Filters != null && requestDetails.Filters.Count > 0)
            {
                sqlAndConditions.AddRange(ApplyFilters(requestDetails.Filters, requestDetails.TableInfo));
            }

            return new QueryRequest
            {
                MainTable = requestDetails.TableInfo.DbStorageName(),
                Fields = sqlFields,
                Joins = joins,
                OrConditions = sqlOrConditions,
                AndConditions = sqlAndConditions,
                SortFields = sqlSortFields,
                MaxResults = maxResults
            };
        }

        private void ExtractSortFields(TableInfo tableInfo, List<FieldControlSortField> sortFields, List<SqlQuerySortField> sqlSortFields, Dictionary<string, SqlQueryJoin> joins)
        {
            if (sortFields != null)
            {
                foreach(var field in sortFields.OrderBy(f => f.OrderId))
                {
                    if (field.InfoAreaId == tableInfo.InfoAreaId)
                    {
                        FieldInfo fi = tableInfo.Fields.Find(f => f.FieldId == field.FieldIndex);
                        if (fi != null)
                        {
                            sqlSortFields.Add(new SqlQuerySortField
                            {
                                FieldName = tableInfo.DbStorageName() + "." + fi.FieldDbName(),
                                OrderString = field.Descending ? "DESC" : "ASC"
                            });
                        }
                    }
                    else
                    {
                        
                       (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tableInfo, field.InfoAreaId, 0);

                        if (linkInfo != null)
                        {
                            string tableAlias = relatedTable.DbStorageName() + "_" + linkInfo.LinkId;

                            if (joins.ContainsKey(tableAlias))
                            {
                                var subTable = joins[tableAlias];
                                FieldInfo lfi = relatedTable.Fields.Find(f => f.FieldId == field.FieldIndex);

                                if (lfi != null)
                                {
                                    sqlSortFields.Add(new SqlQuerySortField
                                    {
                                        FieldName = subTable.TableAlias + "." + lfi.FieldDbName(),
                                        OrderString = field.Descending ? "DESC" : "ASC"
                                    });
                                }
                            }

                        }

                    }
                }
            }
        }

        private void ExtractSearchFields(TableInfo tableInfo, List<FieldControlField> searchFields, string searchValue, Dictionary<string, SqlQueryJoin> joins, List<SqlQueryCondition> sqlOrConditions)
        {
            if (searchFields != null && searchValue != null)
            {
                bool hasOnlyOwnInfoAreaFields = false;
                // if only 3 same info are fields are used we are doing a more
                // complex search
                if (searchFields.Count < 4)
                {
                    hasOnlyOwnInfoAreaFields = true;
                    searchFields.ForEach(field =>
                    {
                        if (field.InfoAreaId != tableInfo.InfoAreaId)
                        {
                            hasOnlyOwnInfoAreaFields = false;
                        }
                    });
                }

                searchFields.ForEach(field =>
                {
                    if (field.InfoAreaId == tableInfo.InfoAreaId)
                    {
                        FieldInfo fi = tableInfo.Fields.Find(f => f.FieldId == field.FieldId);
                        if (fi != null)
                        {
                            if (hasOnlyOwnInfoAreaFields)
                            {
                                foreach (string search in searchValue.Split(' '))
                                {
                                    sqlOrConditions.Add(new SqlQueryCondition
                                    {
                                        Left = tableInfo.DbStorageName() + "." + fi.FieldDbName(),
                                        Operator = " LIKE ",
                                        Right = "'%" + search + "%'"
                                    });
                                }
                            }
                            else
                            {
                                sqlOrConditions.Add(new SqlQueryCondition
                                {
                                    Left = tableInfo.DbStorageName() + "." + fi.FieldDbName(),
                                    Operator = " LIKE ",
                                    Right = "'%" + searchValue + "%'"
                                });
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tableInfo, field.InfoAreaId, field.LinkId);

                            if (linkInfo != null)
                            {
                                string tableAlias = relatedTable.DbStorageName() + "_" + linkInfo.LinkId;
                                FieldInfo lfi = relatedTable.Fields.Find(f => f.FieldId == field.FieldId);

                                sqlOrConditions.Add(new SqlQueryCondition
                                {
                                    Left = tableAlias + "." + lfi.FieldDbName(),
                                    Operator = " LIKE ",
                                    Right = "'%" + searchValue + "%'"
                                });

                                if (!joins.ContainsKey(tableAlias))
                                {
                                    SqlQueryJoin join = GetSqlJoin(tableInfo, linkInfo, relatedTable, tableAlias);
                                    joins[tableAlias] = join;
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
                });
            }
        }

        private void ExtractFields(TableInfo tableInfo, List<FieldControlField> fields, List<SqlQueryField> sqlFields, Dictionary<string, SqlQueryJoin> joins)
        {
            fields.ForEach(field =>
            {
                //_logService.LogDebug($"Processing field {field}");
                if (field.InfoAreaId == tableInfo.InfoAreaId)
                {
                    FieldInfo fi = tableInfo.Fields.Find(f => f.FieldId == field.FieldId);
                    if (fi != null)
                    {
                        SqlQueryField sqlField = new SqlQueryField();
                        sqlField.FieldName = fi.FieldDbName();
                        sqlField.Alias = fi.FieldDbName();
                        sqlField.TableName = tableInfo.DbStorageName();
                        sqlFields.Add(sqlField);
                    }
                }
                else
                {
                    try
                    {
                        (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tableInfo, field.InfoAreaId, field.LinkId);

                        if (linkInfo != null)
                        {
                            string tableAlias = relatedTable.DbStorageName() + "_" + linkInfo.LinkId;

                            FieldInfo lfi = relatedTable.Fields.Find(f => f.FieldId == field.FieldId);

                            SqlQueryField sqlField = new SqlQueryField
                            {
                                FieldName = lfi.FieldDbName(),
                                Alias = field.QueryFieldName(true),
                                TableName = tableAlias
                            };

                            sqlFields.Add(sqlField);

                            if (!joins.ContainsKey(tableAlias))
                            {
                                string relatedRecIdAlias = relatedTable.InfoAreaId + "_" + linkInfo.LinkId + "_recid";

                                SqlQueryField linkedRecIdField = new SqlQueryField
                                {
                                    FieldName = "recid",
                                    Alias = relatedRecIdAlias,
                                    TableName = tableAlias
                                };

                                sqlFields.Add(linkedRecIdField);
                                SqlQueryJoin join = GetSqlJoin(tableInfo, linkInfo, relatedTable, tableAlias);
                                joins[tableAlias] = join;
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
            });
        }

        private SqlQueryJoin GetSqlJoin(TableInfo tableInfo, LinkInfo linkInfo, TableInfo relatedTable, string tableAlias)
        {
            List<SqlQueryCondition> conditions = new List<SqlQueryCondition>();
            if (linkInfo.HasColumn())
            {
                if (linkInfo.LinkId != 126 && linkInfo.LinkId != 127)
                {
                    conditions.Add(new SqlQueryCondition
                    {
                        Left = tableAlias + ".recid",
                        Right = tableInfo.DbStorageName() + "." + linkInfo.GetColumnName(),
                        Operator = "="
                    });
                }
                else
                {
                    conditions.Add(new SqlQueryCondition
                    {
                        Left = tableAlias + ".recid",
                        Right = tableInfo.DbStorageName() + "." + linkInfo.GetColumnName(),
                        Operator = "="
                    });

                    conditions.Add(new SqlQueryCondition
                    {
                        Left = "'" + relatedTable.InfoAreaId + "'",
                        Right = tableInfo.DbStorageName() + "." + linkInfo.GetInfoAreaColumnName(),
                        Operator = "="
                    });
                }
            }
            else if (linkInfo.HasLinkFields())
            {
                foreach(LinkFields lField in linkInfo.LinkFields)
                {
                    conditions.Add(new SqlQueryCondition
                    {
                        Left = tableAlias + "." + lField.GetDestField(),
                        Right = tableInfo.DbStorageName() + "." + lField.GetSoruceField(),
                        Operator = "="
                    });
                }
            }
            else if (linkInfo.HasDestSrcFields())
            {
                conditions.Add(new SqlQueryCondition
                {
                    Left = tableAlias + "." + linkInfo.GetDestField(),
                    Right = tableInfo.DbStorageName() + "." + linkInfo.GetSoruceField(),
                    Operator = "="
                });
            }
            else
            {
                conditions.Add(new SqlQueryCondition
                {
                    Left = tableAlias + "." + linkInfo.GetReverseLinkColumnName(tableInfo.InfoAreaId),
                    Right = tableInfo.DbStorageName() + ".recid",
                    Operator = "="
                });
            }

            SqlQueryJoin join = new SqlQueryJoin
            {
                TableName = relatedTable.DbStorageName(),
                TableAlias = tableAlias,
                Conditions = conditions
            };
            return join;
        }

        private List<SqlQueryCondition> GetParentCondition(TableInfo tableInfo, ParentLink parentLink)
        {
            (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tableInfo, parentLink.ParentInfoAreaId, parentLink.LinkId);

            if (linkInfo != null && (linkInfo.HasColumn() || linkInfo.UseLinkFields))
            {
                List<SqlQueryCondition> conditions = new List<SqlQueryCondition>();

                if (linkInfo.UseLinkFields)
                {
                    string subSql = " (SELECT * FROM ";
                    string subTableAlias = "SUB_" + conditions.Count.ToString();
                    subSql += relatedTable.DbStorageName() + " AS " + subTableAlias;
                    subSql += " WHERE ";

                    linkInfo.LinkFields.ForEach(linkInfo =>
                    {
                        subSql += tableInfo.DbStorageName() + ".F" + linkInfo.SourceFieldId + " = ";
                        subSql += subTableAlias + ".F" + linkInfo.DestFieldId + " AND ";
                    });

                    subSql += subTableAlias + ".recid='" + parentLink.RecordId + "') ";

                    conditions.Add(new SqlQueryCondition
                    {
                        Left = " ",
                        Right = subSql,
                        Operator = " EXISTS "
                    });
                }
                else
                {
                    if (linkInfo.LinkId != 126 && linkInfo.LinkId != 127)
                    {
                        conditions.Add(new SqlQueryCondition
                        {
                            Left = tableInfo.DbStorageName() + "." + linkInfo.GetColumnName(),
                            Right = "'" + parentLink.RecordId + "'",
                            Operator = "="
                        });
                    }
                    else
                    {
                        conditions.Add(new SqlQueryCondition
                        {
                            Left = tableInfo.DbStorageName() + "." + linkInfo.GetColumnName(),
                            Right = "'" + parentLink.RecordId + "'",
                            Operator = "="
                        });

                        conditions.Add(new SqlQueryCondition
                        {
                            Left = tableInfo.DbStorageName() + "." + linkInfo.GetInfoAreaColumnName(),
                            Right = "'" + relatedTable.InfoAreaId + "'",
                            Operator = "="
                        });
                    }
                }

                return conditions;
            }

            return null;
        }

        private List<SqlQueryCondition> ApplyFilter(Filter filter, TableInfo mainTable)
        {
            List<SqlQueryCondition> conditions = new List<SqlQueryCondition>();
            if (filter != null
                && filter.RootTable != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(filter.RootTable.Conditions) || filter.RootTable.ExpandedConditions != null)
                    {
                        conditions.AddRange(ApplyConditions(filter.RootTable));
                    }

                    if (filter.RootTable.SubTables != null)
                    {
                        conditions.AddRange(ApplySubTablesConditions(filter.RootTable.SubTables, mainTable));
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Filter processing request failed with {ex}");
                }
            }

            return conditions;
        }


        private List<SqlQueryCondition> ApplyConditions(QueryTable queryTable)
        {
            List<SqlQueryCondition> conditions = new List<SqlQueryCondition>();

            if (queryTable.ExpandedConditions != null && !string.IsNullOrEmpty(queryTable.ExpandedConditions.Relation))
            {
                ApplyCondition(queryTable.InfoAreaId, conditions, queryTable.ExpandedConditions);
            }

            return conditions;
        }


        private void ApplyCondition(string infoAreaId, List<SqlQueryCondition> conditions, NodeCondition condition)
        {
            if(!string.IsNullOrEmpty(condition.Relation))
            {
                switch (condition.Relation.ToUpper())
                {
                    case "AND":
                        foreach (var subCondition in condition.Conditions)
                        {
                            ApplyCondition(infoAreaId, conditions, subCondition);
                        }
                        break;
                    case "LEAF":
                        AddLeafFilterCondition(infoAreaId, conditions, condition);
                        break;
                    default:
                        break;

                }
            }
        }

        private void AddLeafFilterCondition(string infoAreaId, List<SqlQueryCondition> conditions, NodeCondition condition)
        {
            if (condition.FieldId >= 0
                && !string.IsNullOrEmpty(condition.CompareOperator)
                && ((condition.FieldValues != null && condition.FieldValues.Count > 0) || !string.IsNullOrEmpty(condition.FunctionName)))
            {
                TableInfo tableInfo = TableInfoForInfoArea(infoAreaId);
                if (tableInfo != null)
                {
                    SqlQueryCondition sqlCondition = ExtractSql(condition, tableInfo, tableInfo.DbStorageName());
                    if (sqlCondition != null)
                    {
                        conditions.Add(sqlCondition);
                    }
                }
            }
        }

        private SqlQueryCondition ExtractSql(NodeCondition condition, TableInfo tableInfo, string tableAlias)
        {
            FieldInfo fi = tableInfo.Fields.Find(f => f.FieldId == condition.FieldId);
            if (fi != null)
            {
                string left = tableAlias + "." + fi.FieldDbName();
                string rightValues = "'" + condition.FieldValues[0] + "'";

                if (fi.IsCatalog)
                {
                    rightValues = "'" + condition.FieldValues[0].CurratedCrmCatalog() + "'";
                }

                if (fi.IsBoolean)
                {
                    rightValues = "'" + condition.FieldValues[0].CrmDbBool() + "'";
                }

                string condOperator = condition.CompareOperator;

                if (condition.FieldValues.Count > 1)
                {
                    rightValues = "(";
                    foreach (var val in condition.FieldValues)
                    {
                        if (fi.IsBoolean)
                        {
                            rightValues = "'" + val.CrmDbBool() + "', ";
                        }
                        else if (fi.IsCatalog)
                        {
                            rightValues += "'" + val.CurratedCrmCatalog() + "', ";
                        }
                        else
                        {
                            rightValues += "'" + val + "', ";
                        }
                    }

                    rightValues = rightValues.Remove(rightValues.Length - 2) + ")";

                    if (condition.CompareOperator.Equals("="))
                    {
                        condOperator = "IN";
                    }
                    else
                    {
                        condOperator = " ";
                        rightValues = "(" + left + " NOT IN " + rightValues + " OR " + left + " IS NULL)";
                        left = " ";
                    }
                }
                else
                {
                    if (condition.CompareOperator.Equals("<>"))
                    {
                        condOperator = " ";
                        rightValues = "(" + left + " <> " + rightValues + " OR " + left + " IS NULL)";
                        left = " ";
                    }
                }

                return new SqlQueryCondition
                {
                    Left = left,
                    Operator = condOperator,
                    Right = rightValues
                };
            }

            return null;
        }

        private List<SqlQueryCondition> ApplySubTablesConditions(List<QueryTable> subConditions, TableInfo mainTable)
        {
            List<SqlQueryCondition> conditions = new List<SqlQueryCondition>();
            if (subConditions != null)
            {
                foreach(var condition in subConditions)
                {
                    if(condition.IsExistsCondition())
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        PrepareSubTableConditionQuery(condition, mainTable, mainTable.DbStorageName(), stringBuilder);
                        string subSql = stringBuilder.ToString();
                        if (!string.IsNullOrWhiteSpace(subSql) && subSql.Count() > 12)
                        {
                            conditions.Add(new SqlQueryCondition
                            {
                                Left = " ",
                                Right = subSql,
                                Operator = condition.ParentRelation.StartsWith("WITHOUT") ? " NOT EXISTS" : " EXISTS"
                            });
                        }
                    }
                }
            }

            return conditions ;
        }

        private (string, string) GetLeftRightTableNames(LinkInfo linkInfo, string mainTable, string subTable)
        {
            if(linkInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(linkInfo.RelationType) && linkInfo.RelationType.Equals("OneToMany"))
                {
                    return (mainTable, subTable);
                    
                }
            }
            return (subTable, mainTable);
        }

        private void PrepareSubTableConditionQuery(QueryTable condition, TableInfo mainTable, string mainTableAlias, StringBuilder stringBuilder)
        {
            TableInfo tableInfo = TableInfoForInfoArea(condition.InfoAreaId);
            if (tableInfo != null)
            {
                stringBuilder.Append(" (SELECT * FROM ");
                stringBuilder.Append(tableInfo.DbStorageName());
                stringBuilder.Append(" AS ");
                string subTableAlias = "SUB_" + tableInfo.DbStorageName() + "_" + condition.Id.ToString();
                stringBuilder.Append(subTableAlias);

                LinkInfo linkInfo = mainTable.GetLinkInfo(condition.InfoAreaId, condition.LinkId);
                (string leftTable, string rightTable) = GetLeftRightTableNames(linkInfo, mainTableAlias, subTableAlias);

                if (linkInfo == null)
                {
                    linkInfo = tableInfo.GetLinkInfo(mainTable.InfoAreaId, condition.LinkId);
                    leftTable = mainTableAlias;
                    rightTable = subTableAlias;
                }

                stringBuilder.Append(" WHERE ");
                bool hasLinkData = false;
                if (linkInfo != null)
                {
                    if (linkInfo.UseLinkFields)
                    {
                        linkInfo.LinkFields.ForEach(linkInfo =>
                        {
                            stringBuilder.Append(leftTable + ".F" + linkInfo.SourceFieldId + " = ");
                            stringBuilder.Append(rightTable + ".F" + linkInfo.DestFieldId);
                            stringBuilder.Append(" AND ");
                        });
                        stringBuilder.Length -= 5;
                    }
                    else
                    {
                        if (linkInfo.LinkId != 126 && linkInfo.LinkId != 127)
                        {
                            stringBuilder.Append(leftTable + ".recid = ");
                            string columnName = subTableAlias.Equals(leftTable) ? linkInfo.GetColumnName() : linkInfo.GetColumnName(mainTable.InfoAreaId);
                            stringBuilder.Append(rightTable + "." + columnName);
                            //CRM_FI.recid = SUB_CRM_MA_0.LINK_MA_0
                        }
                        else
                        {
                            stringBuilder.Append(leftTable + ".recid = ");
                            stringBuilder.Append(rightTable + "." + linkInfo.GetColumnName());

                            stringBuilder.Append(leftTable + "." + linkInfo.GetInfoAreaColumnName() + " = ");
                            stringBuilder.Append("'" + linkInfo.TargetInfoAreaId + "'");
                        }
                    }
                    hasLinkData = true;
                }

                if (condition.ExpandedConditions != null)
                {
                    if (hasLinkData)
                    {
                        stringBuilder.Append(" AND ");
                    }
                    AddConditionToStatement(condition.ExpandedConditions, stringBuilder, tableInfo, subTableAlias, string.Empty);
                }

                if (condition.SubTables != null && condition.SubTables.Count > 0)
                {
                    bool isAndNeeded = false;
                    if (condition.ExpandedConditions != null || hasLinkData)
                    {
                        isAndNeeded = true;
                        
                    }

                    foreach (QueryTable subCond in condition.SubTables)
                    {
                        if(isAndNeeded)
                        {
                            stringBuilder.Append(" AND ");
                        }

                        stringBuilder.Append(subCond.ParentRelation.StartsWith("WITHOUT") ? " NOT EXISTS" : " EXISTS");
                        PrepareSubTableConditionQuery(subCond, tableInfo, subTableAlias, stringBuilder);
                        isAndNeeded = true;
                    }
                }

                stringBuilder.Append(")");
            }
        }


        public void AddConditionToStatement(NodeCondition condition, StringBuilder sb, TableInfo tableInfo, string subTableAlias, string logicalOperator)
        {
            if (!string.IsNullOrEmpty(condition.Relation))
            {
                switch (condition.Relation.ToUpper())
                {
                    case "AND":
                    case "OR":
                        sb.Append(" (");
                        foreach (var subCondition in condition.Conditions)
                        {
                            AddConditionToStatement(subCondition, sb, tableInfo, subTableAlias, condition.Relation);
                            sb.Append(" " + condition.Relation + " ");
                        }
                        sb.Length = sb.Length - condition.Relation.Length - 2;
                        sb.Append(") ");
                        break;
                    case "LEAF":
                        SqlQueryCondition cond = ExtractSql(condition, tableInfo, subTableAlias);
                        sb.Append(cond.Left);
                        sb.Append(cond.Operator);
                        sb.Append(cond.Right);
                        break;
                    default:
                        break;

                }
            }
        }


        private List<SqlQueryCondition> ApplyFilters(List<Filter> filters, TableInfo mainTable)
        {
            List<SqlQueryCondition> conditions = new List<SqlQueryCondition>();
            foreach (var filter in filters)
            {
                conditions.AddRange(ApplyFilter(filter, mainTable));
            }

            return conditions;
        }


        public (DataSetMetaInfo, DataSetRecord) PrepareInsertData(TableInfo tableInfo, OfflineRecord record)
        {
            if (record == null || tableInfo.InfoAreaId != record.InfoAreaId)
            {
                throw new CrmException("Record InfoArea: " + record.InfoAreaId
                    + " not matching table definition InfoArea: " + tableInfo.InfoAreaId,
                    CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataTableInfoDataSetMismatch);
            }

            DataSetMetaInfo dataMetaInfo = new DataSetMetaInfo() { InfoAreaId = record.InfoAreaId };
            DataSetRecord dataSetRecord = new DataSetRecord() { InfoAreaId = record.InfoAreaId, RecordId = record.RecordId };

            if (record.RecordFields != null && record.RecordFields.Count > 0)
            {
                dataMetaInfo.FieldIds = new List<int>(record.RecordFields.Count);
                dataSetRecord.Values = new List<string>(record.RecordFields.Count);
                record.RecordFields.ForEach(field =>
                {
                    dataMetaInfo.FieldIds.Add(field.FieldId);
                    dataSetRecord.Values.Add(field.NewValue);
                });
            }

            if (record.RecordLinks != null && record.RecordLinks.Count > 0)
            {
                dataMetaInfo.LinkIds = new List<string>();
                dataSetRecord.Links = new List<string>();
                record.RecordLinks.ForEach(field =>
                {
                    (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tableInfo, field.InfoAreaId, field.LinkId);

                    if (linkInfo != null && linkInfo.HasColumn())
                    {
                        if (linkInfo.IsGenericLink())
                        {
                            dataMetaInfo.LinkIds.Add(linkInfo.GetInfoAreaColumnName());

                            if (linkInfo.TargetInfoAreaId == null)
                            {
                                linkInfo.TargetInfoAreaId = Convert.ToString(DBNull.Value);
                            }                            
                            dataSetRecord.Links.Add(linkInfo.TargetInfoAreaId);
                        }

                        dataMetaInfo.LinkIds.Add(linkInfo.GetColumnName());
                        dataSetRecord.Links.Add(field.RecordId);
                    }
                });
            }

            return (dataMetaInfo, dataSetRecord);
        }
    }
}
