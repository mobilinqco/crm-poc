using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ACRM.mobile.Domain.SettingsHelper
{
    public static class SettingsService
    {
        static readonly string _settingsFile = "SettingsConfiguration.json";

        public static async Task SaveSettingsAsync(SettingsConfiguration settings)
        {
            var settingsConfigurationFilePath = GetSettingsFileLocation();

            if(!File.Exists(settingsConfigurationFilePath))
            {
                var fileCreated = File.Create(settingsConfigurationFilePath);
                fileCreated.Close();
            }

            if(File.Exists(settingsConfigurationFilePath))
            {
                var jsonData = JsonConvert.SerializeObject(settings);
                using var streamWriter = new StreamWriter(settingsConfigurationFilePath);
                await streamWriter.WriteAsync(jsonData);
                streamWriter.Close();
            }
        }

        private static string GetSettingsFileLocation()
        {
            string applicationDataPathFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SettingFolder", _settingsFile);

            if (File.Exists(applicationDataPathFile))
            {
                return applicationDataPathFile;
            }

            string executingAssemblyPathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _settingsFile);
            if (File.Exists(executingAssemblyPathFile))
            {
                return executingAssemblyPathFile;
            }

            return string.Empty;
        }
    }
}
