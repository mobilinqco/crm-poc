using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using Microsoft.Data.Sqlite;

namespace ACRM.mobile.DataAccess.Local.CrmDataContext
{
    public class CrmDataSqlBuilder
    {
        private static string ConvertToSqlType(char fieldTypeChar)
        {
            switch (fieldTypeChar)
            {
                case 'L':
                case 'N':
                case 'K':
                case 'X':
                case 'S':
                case 'B':
                    return "INTEGER";
                case 'F':
                    return "REAL";
                case 'C':
                case 'Z':
                    return "TEXT COLLATE NOCASE";
                default:
                    return "TEXT";
            }
        }

        private static bool IsNumericSqlType(char fieldTypeChar)
        {
            switch (fieldTypeChar)
            {
                case 'L':
                case 'N':
                case 'K':
                case 'X':
                case 'S':
                case 'B':
                case 'F':
                    return true;
                default:
                    return false;
            }
        }

        public static string CreateTableSql(TableInfo tableInfo, bool dropIfExists)
        {
            string sqlString = "(recid TEXT PRIMARY KEY, title TEXT, sync TEXT, upd TEXT, lookup INTEGER";

            if(dropIfExists)
            {
                string dropTableIfExistsString = "DROP TABLE IF EXISTS " + tableInfo.DbStorageName() + "; " + 
                    "CREATE TABLE " + tableInfo.DbStorageName();
                sqlString = string.Format("{0} {1}", dropTableIfExistsString, sqlString);
            }
            else
            {
                string createTableIfNotExists = "CREATE TABLE IF NOT EXISTS " + tableInfo.DbStorageName() + ";";
                sqlString = string.Format("{0} {1}", createTableIfNotExists, sqlString);
            }

            if (tableInfo.Fields != null) {
                tableInfo.Fields.ForEach(field =>
                {
                    sqlString += ", " + field.FieldDbName() + " " + ConvertToSqlType(field.FieldType);
                });
            }

            string genericLinkStr = "";
            if (tableInfo.Links != null)
            {
                tableInfo.Links.ForEach(link =>
                {
                    if (link.HasColumn())
                    {
                        if (link.IsGenericLink())
                        {
                            genericLinkStr = ", " + link.GetInfoAreaColumnName() + " TEXT";
                            genericLinkStr += ", " + link.GetColumnName() + " TEXT";
                        }
                        else
                        {
                            sqlString += ", " + link.GetColumnName() + " TEXT";
                        }
                    }
                });
            }

            sqlString += genericLinkStr + ");";
            return sqlString;
        }
        
        public static string BuildQuery(QueryRequest queryRequest)
        {
            string sqlString = "SELECT " + queryRequest.MainTable + ".recid," + queryRequest.MainTable + ".title";

            queryRequest.Fields.ForEach(field =>
            {
                sqlString += ", " + field.QueryRepresentation();
            });

            sqlString += " FROM " + queryRequest.MainTable;

            if (queryRequest.Joins != null)
            {
                foreach (KeyValuePair<string, SqlQueryJoin> join in queryRequest.Joins)
                {
                    sqlString += " LEFT JOIN " + join.Value.TableName + " AS "
                        + join.Value.TableAlias + " ON ";

                    foreach(SqlQueryCondition condition in join.Value.Conditions)
                    {
                        sqlString += condition.Left + " " + condition.Operator + " " + condition.Right + " AND ";
                    }

                    sqlString = sqlString.Remove(sqlString.Length - 4);
                }
            }

            string orConditions = String.Empty;
            if (queryRequest.OrConditions != null && queryRequest.OrConditions.Count > 0)
            {
                orConditions += "(";
                queryRequest.OrConditions.ForEach(condition =>
                {
                    orConditions += condition.Left + " " + condition.Operator + " " + condition.Right + " OR ";
                });
                orConditions = orConditions.Remove(orConditions.Length - 3) + ") ";
            }

            string andConditions = String.Empty;
            if (queryRequest.AndConditions != null && queryRequest.AndConditions.Count > 0)
            {
                queryRequest.AndConditions.ForEach(condition =>
                {
                    andConditions += condition.Left + " " + condition.Operator + " " + condition.Right + " AND ";
                });
                andConditions = andConditions.Remove(andConditions.Length - 4);
            }

            if(!String.IsNullOrEmpty(orConditions) || !String.IsNullOrEmpty(andConditions))
            {
                sqlString += " WHERE ";
                if(!String.IsNullOrEmpty(andConditions))
                {
                    sqlString += andConditions;
                    if (!String.IsNullOrEmpty(orConditions))
                    {
                        sqlString += " AND " + orConditions;
                    }
                }
                else
                {
                    sqlString += orConditions;
                }
            }
            

            string sortString = string.Empty;
            if (queryRequest.SortFields != null)
            {
                queryRequest.SortFields.ForEach(field =>
                {
                    sortString += field.FieldName + " " + field.OrderString + ", ";
                });

                if (sortString != string.Empty)
                {
                    sqlString += " ORDER BY " + sortString.Remove(sortString.Length - 2);
                }
            }

            if(queryRequest.MaxResults > 0)
            {
                sqlString += " LIMIT " + queryRequest.MaxResults;
            }
            
            return sqlString;
        }

        public static SqliteCommand InsertRowStatement(SqliteCommand insertCommand, TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord record)
        {
            Dictionary<int, FieldInfo> fieldDefinitions = tableInfo.Fields.ToDictionary(field => field.FieldId);
            SqliteCommand cmd = CreateCommandSql(insertCommand, tableInfo.DbStorageName(), fieldDefinitions, dataMetaInfo);

            return AddParametersToSql(cmd, fieldDefinitions, dataMetaInfo, record);
        }

        public static SqliteCommand CreateCommandSql(SqliteCommand insertCommand, string tableName, Dictionary<int, FieldInfo> fieldDefinitions, DataSetMetaInfo dataMetaInfo)
        {
            string sqlString = "INSERT OR REPLACE INTO " + tableName + " (recid, title, sync, upd, lookup ";
            string dataString = " VALUES (@recid, @title, datetime('now'), datetime('now'), @lookup";
            if (dataMetaInfo.FieldIds != null)
            {
                dataMetaInfo.FieldIds.ForEach(fieldId =>
                {
                    if (fieldDefinitions.ContainsKey(fieldId))
                    {
                        FieldInfo fieldInfo = fieldDefinitions[fieldId];
                        if (fieldInfo == null)
                        {
                            throw new CrmException(fieldId + " not defined in the " + tableName + " data model.",
                                CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataFieldNotDefinedInModel);
                        }
                        sqlString += ", " + fieldInfo.FieldDbName();
                        dataString += ", @" + fieldInfo.FieldDbName();
                    }
                });
            }

            if (dataMetaInfo.LinkIds != null)
            {
                dataMetaInfo.LinkIds.ForEach(linkId =>
                {
                    if (linkId.Equals("LINK_RECORDID") && !dataMetaInfo.LinkIds.Contains("LINK_INFOAREA"))
                    {
                        sqlString += ", LINK_INFOAREA";
                        dataString += ", @LINK_INFOAREA";
                    }

                    sqlString += ", " + linkId;
                    dataString += ", @" + linkId;
                });
            }

            sqlString = sqlString + ")" + dataString + ")";

            insertCommand.CommandType = CommandType.Text;
            insertCommand.CommandText = sqlString;
            return insertCommand;
        }

        public static SqliteCommand UpdateRowStatement(SqliteCommand insertCommand, TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord record)
        {
            Dictionary<int, FieldInfo> fieldDefinitions = tableInfo.Fields.ToDictionary(field => field.FieldId);
            SqliteCommand cmd = CreateUpdateCommandSql(insertCommand, tableInfo.DbStorageName(), fieldDefinitions, dataMetaInfo);

            return AddParametersToSql(cmd, fieldDefinitions, dataMetaInfo, record, false);
        }

        public static SqliteCommand CreateUpdateCommandSql(SqliteCommand updateCommand, string tableName, Dictionary<int, FieldInfo> fieldDefinitions, DataSetMetaInfo dataMetaInfo)
        {
            string sqlString = "UPDATE " + tableName + " SET title = 'upd'";
            string dataString = " WHERE recid = @recid";
            if (dataMetaInfo.FieldIds != null)
            {
                dataMetaInfo.FieldIds.ForEach(fieldId =>
                {
                    if (fieldDefinitions.ContainsKey(fieldId))
                    {
                        FieldInfo fieldInfo = fieldDefinitions[fieldId];
                        if (fieldInfo == null)
                        {
                            throw new CrmException(fieldId + " not defined in the " + tableName + " data model.",
                                CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataFieldNotDefinedInModel);
                        }
                        sqlString += ", " + fieldInfo.FieldDbName() + "= @" + fieldInfo.FieldDbName();
                    }
                });
            }

            if (dataMetaInfo.LinkIds != null)
            {
                dataMetaInfo.LinkIds.ForEach(linkId =>
                {
                    if (linkId.Equals("LINK_RECORDID") && !dataMetaInfo.LinkIds.Contains("LINK_INFOAREA"))
                    {
                        sqlString += ", LINK_INFOAREA=@LINK_INFOAREA";
                    }

                    sqlString += ", " + linkId + "= @" + linkId;
                });
            }

            sqlString = sqlString + " " + dataString;

            updateCommand.CommandType = CommandType.Text;
            updateCommand.CommandText = sqlString;
            return updateCommand;
        }

        private static object FieldValue(FieldInfo fieldInfo, string value)
        {
            if(fieldInfo.IsBoolean)
            {
                bool.TryParse(value, out bool result);
                return result;
            }

            if (fieldInfo.IsReal)
            {
                double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double result);
                return result;
            }

            if (fieldInfo.IsCatalog || fieldInfo.IsNumeric)
            {
                // it looks like the integer fields could contain real numbers too :(
                // original inteligence from CRM.pad
                if(value.Contains("."))
                {
                    double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double dresult);
                    return dresult;
                }

                long.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out long result);
                return result;
            }

            if (fieldInfo.IsDate)
            {
                return value.Replace("-", "");
            }

            if (fieldInfo.IsTime)
            {
                return value.Replace(":", "");
            }


            return value;
        }

        public static SqliteCommand AddParametersToSql(SqliteCommand insertCommand, Dictionary<int, FieldInfo> fieldDefinitions, DataSetMetaInfo dataMetaInfo, DataSetRecord record, bool isInsert = true)
        {
            int i = 0;
            insertCommand.Parameters.Clear();
            insertCommand.Parameters.AddWithValue("@recid", record.RecordId);
            if (isInsert)
            {
                insertCommand.Parameters.AddWithValue("@title", string.IsNullOrWhiteSpace(record.InfoAreaId) ? "" : record.InfoAreaId);
                insertCommand.Parameters.AddWithValue("@lookup", 0);
            }

            if (dataMetaInfo.FieldIds != null)
            {
                dataMetaInfo.FieldIds.ForEach(fieldId =>
                {
                    if (fieldDefinitions.ContainsKey(fieldId))
                    {
                        FieldInfo fieldInfo = fieldDefinitions[fieldId];
                        insertCommand.Parameters.AddWithValue("@" + fieldInfo.FieldDbName(), FieldValue(fieldInfo, record.Values[i]));
                    }
                    i++;
                });
            }

            i = 0;
            if (dataMetaInfo.LinkIds != null)
            {
                dataMetaInfo.LinkIds.ForEach(linkId =>
                {
                    if (linkId.Equals("LINK_RECORDID") && !dataMetaInfo.LinkIds.Contains("LINK_INFOAREA"))
                    {
                        insertCommand.Parameters.AddWithValue("@LINK_INFOAREA", record.Links[i]);
                        i++;
                    }
                    insertCommand.Parameters.AddWithValue("@" + linkId, record.Links[i]);
                    i++;                    
                });
            }
            return insertCommand;
        }

        public static SqliteCommand RemoveRowStatement(SqliteCommand insertCommand, TableInfo tableInfo, string recordId)
        {
            string sqlString = $"DELETE FROM {tableInfo.DbStorageName()} WHERE recid='{recordId}'";

            insertCommand.CommandType = CommandType.Text;
            insertCommand.CommandText = sqlString;

            return insertCommand;
        }
    }
}
