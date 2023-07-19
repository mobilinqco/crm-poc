using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Logging;
using Microsoft.Data.Sqlite;

namespace ACRM.mobile.DataAccess.Local.CrmDataContext
{
    public class CrmDataContext
    {
        private string _dbPath;
        private string _dbBackupPath;
        private SqliteConnection _connection;
        private SqliteTransaction _sqliteTransaction;
        protected readonly ILogService _logService;
        private readonly ISessionContext _sessionContext;

        public SqliteConnection Connection
        {
            get => _connection;
        }

        public CrmDataContext(ISessionContext sessionContext, ILogService logService)
        {
            _logService = logService;
            _logService.LogDebug("Createing CrmDataContext");
            SQLitePCL.Batteries_V2.Init();
            _sessionContext = sessionContext;
            _sessionContext.PropertyChanged += ISessionContext_CrmInstanceChanged;
            SetupConnection();
        }

        private void SetupConnection()
        {
            if(_connection != null)
            {
                _connection.Dispose();
            }

            if (!Directory.Exists(_sessionContext.LocalCrmInstancePath()))
            {
                Directory.CreateDirectory(_sessionContext.LocalCrmInstancePath());
            }

            _dbPath = Path.Combine(_sessionContext.LocalCrmInstancePath(), "crmData.db");
            _dbBackupPath = Path.Combine(_sessionContext.LocalCrmInstancePath(), "crmDataBackup.db");
            _connection = new SqliteConnection("Data Source=" + _dbPath);
            _connection.Open();
            //SqlStatement("PRAGMA synchronous = OFF; PRAGMA page_size = 32768; PRAGMA journal_mode=MEMORY;");
        }

        private void ISessionContext_CrmInstanceChanged(object sender, PropertyChangedEventArgs e)
        {

            _logService.LogDebug("CrmDataContext Session Context has changed");
            if(e != null && e.PropertyName.Equals("CrmInstance"))
            {
                if (_sessionContext.CrmInstance == null)
                {
                    _connection.Dispose();
                }
                else
                {
                    SetupConnection();
                }
            }
        }

        ~CrmDataContext()
        {
            _logService.LogDebug("Disposing CrmDataContext");
            _connection.Dispose();
        }

        public void CreateDatabaseBackup()
        {
            if (File.Exists(_dbPath))
            {
                File.Copy(_dbPath, _dbBackupPath, true);
            }
        }

        public void RestoreDatabase()
        {
            if (File.Exists(_dbBackupPath))
            {
                File.Copy(_dbBackupPath, _dbPath, true);
            }
        }

        public void SqlStatement(string sql)
        {
            var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQueryAsync();
        }

        public void BeginTransaction()
        {
            _sqliteTransaction = _connection.BeginTransaction();
        }

        public void ExecuteTransactionSql(string query)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }


        public SqliteCommand RetrieveCommand()
        {
            return _connection.CreateCommand();
        }

        public void ExecuteCommand(SqliteCommand command)
        {
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{ex.GetType().Name + " : " + ex.Message}");
                throw ex;
            }
        }

        public void CommitTransaction()
        {
            _sqliteTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            _sqliteTransaction.Rollback();
        }

        public async Task<DataTable> QueryAsync(string query, CancellationToken cancellationToken)
        {
            try
            {

                    var command = _connection.CreateCommand();
                    command.CommandText = query;

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (reader.HasRows)
                        {
                            DataTable result = new DataTable();
                            using (DataSet ds = new DataSet() { EnforceConstraints = false })
                            {
                                lock (_connection)
                                {
                                    ds.Tables.Add(result);
                                    result.Load(reader, LoadOption.OverwriteChanges);
                                    ds.Tables.Remove(result);
                                }
                            }

                            return result;
                        }
                    };

            }
            catch (SqliteException ex)
            {
                _logService.LogError($"{ex.GetType().Name + " : " + ex.Message}");
                
            }
            catch (ConstraintException ex)
            {
                _logService.LogError($"{ex.GetType().Name + " : " + ex.Message}");
            }
            catch(Exception ex)
            {
                _logService.LogError($"{ex.GetType().Name + " : " + ex.Message}");
            }

            return null;
        }

        public async Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString,
            CancellationToken cancellationToken)
        {
            using (DbCommand command = _connection.CreateCommand())
            {
                command.Connection.Open();
                command.CommandText = queryString;
                command.CommandType = CommandType.Text;
                
                using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    List<DynamicStringModel> modelList = new List<DynamicStringModel>();

                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, string> currentModelDictionary = new Dictionary<string, string>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            currentModelDictionary.Add(reader.GetName(i), reader.GetValue(i).ToString().Replace("\n", ""));
                        }

                        modelList.Add(new DynamicStringModel(currentModelDictionary));
                    }

                    return modelList;
                }
            }
        }
    }
}
