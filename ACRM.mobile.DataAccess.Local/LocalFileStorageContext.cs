using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using Newtonsoft.Json;

namespace ACRM.mobile.DataAccess.Local
{
    public class LocalFileStorageContext : ILocalFileStorageContext
    {
        private readonly ISessionContext _sessionContext;

        public LocalFileStorageContext(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        public T GetContent<T>(string fileName)
        {
            return Read<T>(Path.Combine(_sessionContext.LocalCrmInstancePath(), fileName));
        }

        public async Task<T> SaveContent<T>(T content, string fileName)
        {
            string filePath = Path.Combine(_sessionContext.LocalCrmInstancePath(), fileName);
            await Save(content, filePath);
            return Read<T>(filePath);
        }

        private async Task Save<T>(T content, string filePath)
        {
            var serializedObject = JsonConvert.SerializeObject(content);

            await Task.Run(() => File.WriteAllText(filePath,
                serializedObject));
        }

        private T Read<T>(string filePath)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
            }

            return default(T);
        }
    }
}
