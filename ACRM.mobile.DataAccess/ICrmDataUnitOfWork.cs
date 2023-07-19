using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.DataAccess
{
    public interface ICrmDataUnitOfWork
    {
        void GenerateSchema(List<TableInfo> tables, bool dropIfExists);
        void CreateDatabaseBackup();
        void RestoreDatabase();
        void AddRange(TableInfo tableInfo, DataSetData dataSet);
        void AddRangeWithTransaction(TableInfo tableInfo, DataSetData dataSet);
        void UpdateRange(TableInfo tableInfo, DataSetData dataSet);
        Task<DataTable> RetrieveData(QueryRequest queryRequest, CancellationToken cancellationToken);
        void AddRecord(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord);
        void UpdateRecord(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord);
        void DeleteRecord(TableInfo tableInfo, string recordId);
        Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken);
    }
}
