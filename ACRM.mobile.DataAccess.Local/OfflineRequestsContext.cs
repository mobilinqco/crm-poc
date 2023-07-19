using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.OfflineSync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACRM.mobile.DataAccess.Local
{
    public class OfflineRequestsContext : DbContext
    {
        private readonly string _dbPath;
        private readonly string _dbBackupPath;

        public DbSet<OfflineRequest> Requests { get; set; }
        public DbSet<RequestControl> RequestControl { get; set; }
        public DbSet<SyncHistory> SyncHistory { get; set; }
        public DbSet<SyncInfo> SyncInfo { get; set; }

        public OfflineRequestsContext(ISessionContext sessionContext)
        {
            SQLitePCL.Batteries_V2.Init();
            if (!Directory.Exists(sessionContext.LocalCrmInstancePath()))
            {
                Directory.CreateDirectory(sessionContext.LocalCrmInstancePath());
            }

            _dbPath = Path.Combine(sessionContext.LocalCrmInstancePath(), "offline.db");
            _dbBackupPath = Path.Combine(sessionContext.LocalCrmInstancePath(), "offlineBackup.db");
            
        }

        ~OfflineRequestsContext()
        {
            Dispose();
        }

        //public static readonly ILoggerFactory ConsoleLoggerFactory
        //    = LoggerFactory.Create(builder =>
        //    {
        //        builder
        //            .AddFilter((category, level) =>
        //                    category == DbLoggerCategory.Database.Command.Name
        //                    && level == LogLevel.Information)
        //            .AddConsole();
        //    });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseLoggerFactory(ConsoleLoggerFactory)
                .UseSqlite("Data Source=" + _dbPath);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<OfflineRecord>().HasOne(p => p.OfflineRequest).WithMany(b => b.Records)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<DocumentUpload>().HasOne(p => p.OfflineRequest).WithMany(b => b.DocumentUploads)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<OfflineRecordLink>().HasOne(p => p.OfflineRecord).WithMany(b => b.RecordLinks)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<OfflineRecordField>().HasOne(p => p.OfflineRecord).WithMany(b => b.RecordFields)
                .OnDelete(DeleteBehavior.ClientCascade);

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
            if(File.Exists(_dbBackupPath))
            {
                File.Copy(_dbBackupPath, _dbPath, true);
            }
        }

        public Task<List<OfflineRequest>> GetAllRequests(CancellationToken cancellationToken)
        {
            return Requests
                .Include(or => or.Records)
                    .ThenInclude(r => r.RecordFields)
                .Include(or => or.Records)
                    .ThenInclude(r => r.RecordLinks)
                .Include(or => or.DocumentUploads)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}