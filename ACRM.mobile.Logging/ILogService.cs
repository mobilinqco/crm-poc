using NLog;
using System.Reflection;

namespace ACRM.mobile.Logging
{
    public interface ILogService
    {
        void Initialize(Assembly assembly, string assemblyName);

        void LogDebug(string message);

        void LogError(string message);

        void LogFatal(string message);

        void LogInfo(string message);

        void LogWarning(string message);

        void ChangeLogLevel(LogLevel level);

        void AddDinamicFilter(string filterCondition);

        void RemoveDinamicFilter(string filterCondition);
    }
}
