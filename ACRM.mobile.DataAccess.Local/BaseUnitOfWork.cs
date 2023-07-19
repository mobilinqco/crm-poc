using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace ACRM.mobile.DataAccess.Local
{
    public class BaseUnitOfWork
    {
        public ILocalRepository<T> GenericRepository<T, TContext>(Dictionary<string, object> repositories, TContext context) where T : class where TContext : DbContext
        {
            var type = typeof(T).Name;
            if (!repositories.ContainsKey(type))
            {
                var repositoryType = typeof(LocalRepository<T, TContext>);
                //var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
                var repositoryInstance = Activator.CreateInstance(repositoryType, context);
                repositories.Add(type, repositoryInstance);
            }
            return (LocalRepository<T, TContext>)repositories[type];
        }

        public void CreateTables(DbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        public void Save(DbContext context)
        {
            context.SaveChanges();
        }

        public bool DatabaseExists(DbContext context)
        {
            return (context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
        }

        public async Task<List<DynamicStringModel>> ExecuteRawQueryString(DbContext context,
            string queryString,
            CancellationToken cancellationToken)
        {
            using (DbCommand command = context.Database.GetDbConnection().CreateCommand())
            {
                command.Connection.Open();
                command.CommandText = queryString;
                command.CommandType = System.Data.CommandType.Text;

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
