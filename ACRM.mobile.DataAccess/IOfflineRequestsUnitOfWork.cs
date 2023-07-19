using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.DataAccess
{
    public interface IOfflineRequestsUnitOfWork
    {
        public ILocalRepository<T> GenericRepository<T>() where T : class;
        void CreateTables();
        bool DatabaseExists();
        void Save();
        void CreateDatabaseBackup();
        void RestoreDatabase();

        Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken);

        int MaxRequestId();
        int MaxRecordId();

        Task<List<OfflineRequest>> GetAllRequests(CancellationToken cancellationToken);
    }
}
