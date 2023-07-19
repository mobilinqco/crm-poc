using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using Microsoft.Data.Sqlite;
using NLog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ACRM.mobile.DataAccess.Local.CrmDataContext
{
    public class CrmDataUnitOfWork: BaseUnitOfWork, ICrmDataUnitOfWork
    {
        protected CrmDataContext _context;
        private readonly ILogService _logService;

        public CrmDataUnitOfWork(CrmDataContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public void GenerateSchema(List<TableInfo> tables, bool dropIfExists)
        {
            tables.ForEach(table =>
            {
                string createTableSql = CrmDataSqlBuilder.CreateTableSql(table, dropIfExists);
                _logService.LogDebug($"Creating table with SQL: {createTableSql}");
                _context.SqlStatement(createTableSql);
            });

        }

        public void CreateDatabaseBackup()
        {
            _context.CreateDatabaseBackup();
        }

        public void RestoreDatabase()
        {
            _context.RestoreDatabase();
        }

        public void AddRangeWithTransaction(TableInfo tableInfo, DataSetData dataSet)
        {
            if (tableInfo.InfoAreaId != dataSet.InfoAreaId)
            {
                throw new CrmException("Data set InfoArea: " + dataSet.InfoAreaId
                    + " not matching table definition InfoArea: " + tableInfo.InfoAreaId,
                    CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataTableInfoDataSetMismatch);
            }

            if (dataSet.Rows != null)
            {
                _logService.LogDebug($"Synchronization of {dataSet.DataSetName} returned {dataSet.RowCount} records");
                using (var transaction = _context.Connection.BeginTransaction())
                {
                    SqliteCommand insertCommand = _context.RetrieveCommand();
                    Dictionary<int, FieldInfo> fieldDefinitions = tableInfo.Fields.ToDictionary(field => field.FieldId);
                    insertCommand = CrmDataSqlBuilder.CreateCommandSql(insertCommand, tableInfo.DbStorageName(), fieldDefinitions, dataSet.MetaInfos[0]);

                    dataSet.Rows.ForEach(row =>
                    {
                        insertCommand = CrmDataSqlBuilder.AddParametersToSql(insertCommand, fieldDefinitions, dataSet.MetaInfos[0], row.DataSetRecord);
                        _context.ExecuteCommand(insertCommand);
                    });

                    transaction.Commit();
                }

            }
            else
            {
                _logService.LogDebug($"Synchronization of {dataSet.DataSetName}: no records");
            }
        }

        public void AddRange(TableInfo tableInfo, DataSetData dataSet)
        {
            if (tableInfo.InfoAreaId != dataSet.InfoAreaId)
            {
                throw new CrmException("Data set InfoArea: " + dataSet.InfoAreaId
                    + " not matching table definition InfoArea: " + tableInfo.InfoAreaId,
                    CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataTableInfoDataSetMismatch);
            }

            if (dataSet.Rows != null)
            {
                _logService.LogDebug($"Synchronization of {dataSet.DataSetName} returned {dataSet.RowCount} records");
                SqliteCommand insertCommand = _context.RetrieveCommand();
                Dictionary<int, FieldInfo> fieldDefinitions = tableInfo.Fields.ToDictionary(field => field.FieldId);
                insertCommand = CrmDataSqlBuilder.CreateCommandSql(insertCommand, tableInfo.DbStorageName(), fieldDefinitions, dataSet.MetaInfos[0]);

                dataSet.Rows.ForEach(row =>
                {
                    insertCommand = CrmDataSqlBuilder.AddParametersToSql(insertCommand, fieldDefinitions, dataSet.MetaInfos[0], row.DataSetRecord);
                    _context.ExecuteCommand(insertCommand);
                });

            }
            else
            {
                _logService.LogDebug($"Synchronization of {dataSet.DataSetName}: no records");
            }
        }

        public void UpdateRange(TableInfo tableInfo, DataSetData dataSet)
        {
            if (tableInfo.InfoAreaId != dataSet.InfoAreaId)
            {
                throw new CrmException("Data set InfoArea: " + dataSet.InfoAreaId
                    + " not matching table definition InfoArea: " + tableInfo.InfoAreaId,
                    CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataTableInfoDataSetMismatch);
            }

            if (dataSet.Rows != null)
            {
                //_context.BeginTransaction();
                _logService.LogDebug($"{ dataSet.DataSetName + " : " + dataSet.RowCount + " records"}");
                SqliteCommand insertCommand = _context.RetrieveCommand();
                Dictionary<int, FieldInfo> fieldDefinitions = tableInfo.Fields.ToDictionary(field => field.FieldId);
                insertCommand = CrmDataSqlBuilder.CreateCommandSql(insertCommand, tableInfo.DbStorageName(), fieldDefinitions, dataSet.MetaInfos[0]);

                dataSet.Rows.ForEach(row =>
                {
                    DeleteRecord(tableInfo, row.DataSetRecord.RecordId);
                    insertCommand = CrmDataSqlBuilder.AddParametersToSql(insertCommand, fieldDefinitions, dataSet.MetaInfos[0], row.DataSetRecord);
                    //_logger.Debug($"{tableInfo.InfoAreaId + " query: " + insertCommand.CommandText}");
                    _context.ExecuteCommand(insertCommand);
                });

            }
            else
            {
                _logService.LogDebug($"{ dataSet.DataSetName + " : has no records"}");
            }
        }

        public async Task<DataTable> RetrieveData(QueryRequest queryRequest, CancellationToken cancellationToken)
        {
            string sql = CrmDataSqlBuilder.BuildQuery(queryRequest);
            _logService.LogDebug($"{queryRequest.MainTable + " query: " + sql}");
            return await _context.QueryAsync(sql, cancellationToken);
        }

        public void AddRecord(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord)
        {
            SqliteCommand insertCommand = _context.RetrieveCommand();
            insertCommand = CrmDataSqlBuilder.InsertRowStatement(insertCommand, tableInfo, dataMetaInfo, dataSetRecord);
            _logService.LogDebug($"{tableInfo.InfoAreaId + " query: " + insertCommand.CommandText}");
            _context.ExecuteCommand(insertCommand);
        }

        public void UpdateRecord(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord)
        {
            SqliteCommand updateCommand = _context.RetrieveCommand();
            updateCommand = CrmDataSqlBuilder.UpdateRowStatement(updateCommand, tableInfo, dataMetaInfo, dataSetRecord);
            _logService.LogDebug($"{tableInfo.InfoAreaId + " query: " + updateCommand.CommandText}");
            _context.ExecuteCommand(updateCommand);
        }

        public void DeleteRecord(TableInfo tableInfo, string recordId)
        {
            SqliteCommand deleteCommand = _context.RetrieveCommand();
            deleteCommand = CrmDataSqlBuilder.RemoveRowStatement(deleteCommand, tableInfo, recordId);
            _logService.LogDebug($"{tableInfo.InfoAreaId + " query: " + deleteCommand.CommandText}");
            _context.ExecuteCommand(deleteCommand);
        }

        public async Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken)
        {
            return await _context.ExecuteRawQueryString(queryString, cancellationToken);
        }
    }
}
