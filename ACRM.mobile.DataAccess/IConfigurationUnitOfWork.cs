using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.DataAccess
{
    public interface IConfigurationUnitOfWork
    {
        public ILocalRepository<T> GenericRepository<T>() where T : class;
        void CreateTables();
        bool DatabaseExists();
        void Save();
        void CreateDatabaseBackup();
        void RestoreDatabase();

        Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken);
        Task<List<Menu>> GetMenus(CancellationToken cancellationToken);
        Task<List<Header>> GetHeaders(CancellationToken cancellationToken);
        Task<List<FieldControl>> GetFieldControls(CancellationToken cancellationToken);
        Task<List<TableInfo>> GetTableInfos(CancellationToken cancellationToken);
        Task<List<Catalog>> GetCatalogs(CancellationToken cancellationToken);
        Task<List<Button>> GetButtons(CancellationToken cancellationToken);
        Task<Form> GetForm(string unitName, CancellationToken cancellationToken);
        Task<List<Filter>> GetFilters(CancellationToken cancellationToken);
        Task<List<TableCaption>> GetTableCaptions(CancellationToken cancellationToken);
        Task<List<SearchAndList>> GetSearchAndLists(CancellationToken cancellationToken);
        Task<List<Query>> GetQueries(CancellationToken cancellationToken);
        Task<List<QuickSearch>> GetQuickSearch(CancellationToken cancellationToken);
        Task<WebConfigLayout> GetWebConfigLayout(string unitName, CancellationToken cancellationToken);
    }
}
