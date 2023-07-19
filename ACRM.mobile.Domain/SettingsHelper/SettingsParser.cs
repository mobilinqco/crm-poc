using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ACRM.mobile.Domain.SettingsHelper
{
    public class SettingsParser
    {
        static readonly string _settingsFile = "SettingsConfiguration.json";

        public static async Task<SettingsConfiguration> LoadSettingsAsync()
        {
            var settings = new SettingsConfiguration();
            try
            {
                string filePath = GetSettingsFileLocation();

                if (File.Exists(filePath))
                {
                    using var streamReader = new StreamReader(filePath);
                    settings = JsonConvert.DeserializeObject<SettingsConfiguration>(await streamReader.ReadToEndAsync());
                }
            }
            catch { }

            return settings;
        }

        static string GetSettingsFileLocation()
        {
            string applicationDataPathFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SettingFolder", _settingsFile);

            if (File.Exists(applicationDataPathFile))
            {
                return applicationDataPathFile;
            }
#if DEBUG
            string executingAssemblyPathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _settingsFile);
            if (File.Exists(executingAssemblyPathFile))
            {
                return executingAssemblyPathFile;
            }
#endif
            return string.Empty;
        }
    }
}
