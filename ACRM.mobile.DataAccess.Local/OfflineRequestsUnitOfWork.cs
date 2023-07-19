using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using Microsoft.EntityFrameworkCore;

namespace ACRM.mobile.DataAccess.Local
{
    public class OfflineRequestsUnitOfWork : BaseUnitOfWork, IOfflineRequestsUnitOfWork
    {
        protected OfflineRequestsContext _context;
        private Dictionary<string, object> _repositories;

        public OfflineRequestsUnitOfWork(OfflineRequestsContext context)
        {
            _context = context;
        }

        public void CreateTables()
        {
            CreateTables(_context);
        }

        public bool DatabaseExists()
        {
            return DatabaseExists(_context);
        }

        public ILocalRepository<T> GenericRepository<T>() where T : class
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<string, object>();
            }

            return GenericRepository<T, OfflineRequestsContext>(_repositories, _context);
        }

        public void Save()
        {
            Save(_context);
        }

        public void CreateDatabaseBackup()
        {
            _context.CreateDatabaseBackup();
        }

        public void RestoreDatabase()
        {
            _context.RestoreDatabase();
        }

        public Task<List<OfflineRequest>> GetAllRequests(CancellationToken cancellationToken)
        {
            return _context.GetAllRequests(cancellationToken);
        }

        public async Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken)
        {
            return await ExecuteRawQueryString(_context, queryString, cancellationToken);
        }

        public int MaxRequestId()
        {
            try
            {
                int maxId = _context.Requests.Max(req => req.Id);
                if (maxId > -1)
                {
                    return maxId + 1;
                }
                    
            }
            catch(Exception e)
            {

            }

            return 1;
        }

        public int MaxRecordId()
        {
            return 1;
        }
    }
}
