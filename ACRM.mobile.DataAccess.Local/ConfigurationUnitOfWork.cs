using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.DataAccess.Local
{
    public class ConfigurationUnitOfWork: BaseUnitOfWork, IConfigurationUnitOfWork
    {
        protected ConfigurationContext _context;
        private Dictionary<string, object> _repositories;

        public ConfigurationUnitOfWork(ConfigurationContext context)
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

            return GenericRepository<T, ConfigurationContext>(_repositories, _context);
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

        public Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken)
        {
            return ExecuteRawQueryString(_context, queryString, cancellationToken);
        }

        public Task<List<Menu>> GetMenus(CancellationToken cancellationToken)
        {
            return _context.GetMenus(cancellationToken);
        }

        public Task<List<Header>> GetHeaders(CancellationToken cancellationToken)
        {
            return _context.GetHeaders(cancellationToken);
        }

        public Task<List<FieldControl>> GetFieldControls(CancellationToken cancellationToken)
        {
            return _context.GetFieldControls(cancellationToken);
        }

        public Task<List<Query>> GetQueries(CancellationToken cancellationToken)
        {
            return _context.GetQueries(cancellationToken);
        }

        public Task<List<QuickSearch>> GetQuickSearch(CancellationToken cancellationToken)
        {
            return _context.GetQuickSearch(cancellationToken);
        }

        public Task<List<TableInfo>> GetTableInfos(CancellationToken cancellationToken)
        {
            return _context.GetTables(cancellationToken);
        }

        public Task<List<Catalog>> GetCatalogs(CancellationToken cancellationToken)
        {
            return _context.GetCatalogs(cancellationToken);
        }

        public Task<List<Button>> GetButtons(CancellationToken cancellationToken)
        {
            return _context.GetButtons(cancellationToken);
        }

        public Task<Form> GetForm(string unitName, CancellationToken cancellationToken)
        {
            return _context.GetForm(unitName, cancellationToken);
        }

        public Task<List<Filter>> GetFilters(CancellationToken cancellationToken)
        {
            return _context.GetFiltersAsync(cancellationToken);
        }

        public Task<List<TableCaption>> GetTableCaptions(CancellationToken cancellationToken)
        {
            return _context.GetTableCaptions(cancellationToken);
        }

        public Task<List<SearchAndList>> GetSearchAndLists(CancellationToken cancellationToken)
        {
            return _context.GetSearchAndLists(cancellationToken);
        }

        public Task<WebConfigLayout> GetWebConfigLayout(string unitName, CancellationToken cancellationToken)
        {
            return _context.GetWebConfigLayout(unitName, cancellationToken);
        }
    }
}
