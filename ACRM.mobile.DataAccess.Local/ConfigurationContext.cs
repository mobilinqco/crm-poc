using System;
using System.IO;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.Configuration.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ACRM.mobile.DataAccess.Local
{
    public class ConfigurationContext: DbContext
    {
        private readonly string _dbPath;
        private readonly string _dbBackupPath;

        public static readonly ILoggerFactory ConsoleLoggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                            category == DbLoggerCategory.Database.Command.Name
                            && level == LogLevel.Information)
                    .AddConsole();
            });

        // User Interface Configuration Entities
        public DbSet<Analysis> Analyses { get; set; }
        public DbSet<AnalysisCategory> AnalysisCategories { get; set; }
        public DbSet<Button> Buttons { get; set; }
        public DbSet<DataSet> DataSets { get; set; }
        public DbSet<Expand> Details { get; set; }
        public DbSet<FieldControl> FieldControls { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Header> Headers { get; set; }
        public DbSet<ConfigResource> Images { get; set; }
        public DbSet<InfoArea> InfoAreas { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Query> Queries { get; set; }
        public DbSet<QuickSearch> QuickSearch { get; set; }
        public DbSet<SearchAndList> Searches { get; set; }
        public DbSet<TableCaption> TableCaptions { get; set; }
        public DbSet<Textgroup> Textgroups { get; set; }
        public DbSet<Timeline> Timelines { get; set; }
        public DbSet<TreeView> TreeViews { get; set; }
        public DbSet<WebConfigLayout> WebConfigLayouts { get; set; }
        public DbSet<WebConfigValue> WebConfigValues { get; set; }

        // DataModel Configuration Entities
        public DbSet<TableInfo> Tables { get; set; }
        public DbSet<DataModel> DataModel { get; set; }
        public DbSet<QueryResult> QueryResults { get; set; }
        public DbSet<RollbackInfo> RollbackInfos { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }

        public ConfigurationContext(ISessionContext sessionContext)
        {
            SQLitePCL.Batteries_V2.Init();
            if (!Directory.Exists(sessionContext.LocalCrmInstancePath()))
            {
                Directory.CreateDirectory(sessionContext.LocalCrmInstancePath());
            }

            _dbPath = Path.Combine(sessionContext.LocalCrmInstancePath(), "config.db");
            _dbBackupPath = Path.Combine(sessionContext.LocalCrmInstancePath(), "configBackup.db");
        }

        ~ConfigurationContext()
        {
            Dispose();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseLoggerFactory(ConsoleLoggerFactory)
                //.EnableSensitiveDataLogging()
                .UseSqlite("Data Source=" + _dbPath);
                //.UseSqlite($"Filename={DbPath}");

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Expand>()//.HasOptional(e => e.AlternateExpands)
            //    .HasMany(e => e.AltenrateExpands);
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

        public Task<List<Menu>> GetMenus(CancellationToken cancellationToken)
        {
            return Menus
                .Include(m => m.ViewReference)
                .ThenInclude(v => v.Arguments)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<Header>> GetHeaders(CancellationToken cancellationToken)
        {
            return Headers
                .Include(h => h.SubViews)
                    .ThenInclude(s => s.ViewReference)
                        .ThenInclude(v => v.Arguments)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        
        public Task<List<FieldControl>> GetFieldControls(CancellationToken cancellationToken)
        {
            return FieldControls
                .Include(fc => fc.Tabs)
                    .ThenInclude(t => t.Fields)
                        .ThenInclude(fi => fi.Attributes)
                .Include(fc => fc.Tabs)
                    .ThenInclude(t => t.Attributes)
                .Include(fc => fc.SortFields)
                .Include(fc => fc.Attributes)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<Query>> GetQueries(CancellationToken cancellationToken)
        {
            return Queries
                .Include(q => q.QueryFields)
                .Include(q => q.SortFields)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<QuickSearch>> GetQuickSearch(CancellationToken cancellationToken)
        {
            return QuickSearch
                .Include(q => q.Entries)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<TableInfo>> GetTables(CancellationToken cancellationToken)
        {
            return Tables
                .Include(t => t.Fields)
                .Include(t => t.Links)
                .ThenInclude(l => l.LinkFields)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<Catalog>> GetCatalogs(CancellationToken cancellationToken)
        {
            return Catalogs
                .Include(c => c.CatalogValues)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<Button>> GetButtons(CancellationToken cancellationToken)
        {
            return Buttons
                .Include(b => b.ViewReference)
                .ThenInclude(v => v.Arguments)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<Form> GetForm(string unitName, CancellationToken cancellationToken)
        {
            return Forms
                .Where(a => a.UnitName.Equals(unitName))
                .Include(m => m.Tabs)
                    .ThenInclude(v => v.Rows)
                        .ThenInclude(i => i.Items)
                //.ThenInclude(j => j.ItemAttributes)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<Filter>> GetFiltersAsync(CancellationToken cancellationToken)
        {
            return Filters
                .ToListAsync(cancellationToken);
        }

        public Task<List<TableCaption>> GetTableCaptions(CancellationToken cancellationToken)
        {
            return TableCaptions
                .Include(tc => tc.SpecialDefs)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<List<SearchAndList>> GetSearchAndLists(CancellationToken cancellationToken)
        {
            return Searches
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<WebConfigLayout> GetWebConfigLayout(string unitName, CancellationToken cancellationToken)
        {
            return WebConfigLayouts
                .Where(a => a.Label.Equals(unitName))
                .Include(m => m.Tabs)
                 .ThenInclude(v => v.Fields)
                        .ThenInclude(i => i.options)
 
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
