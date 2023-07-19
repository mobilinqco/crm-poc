using NLog;
using NLog.Config;
using NLog.Filters;
using System;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ACRM.mobile.Logging
{
    public class LogService : ILogService
    {
        private Logger logger;

        public void Initialize(Assembly assembly, string assemblyName)
        {
            var location = $"{assemblyName}.NLog.config";
            var stream = assembly.GetManifestResourceStream(location);
            LogManager.Configuration = new XmlLoggingConfiguration(XmlReader.Create(stream), null);
            logger = LogManager.GetCurrentClassLogger();
        }

        public void LogDebug(string message)
        {
             logger.Debug(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
        }

        public void LogFatal(string message)
        {
            logger.Fatal(message);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogWarning(string message)
        {
            logger.Warn(message);
        }

        public void ChangeLogLevel(LogLevel level)
        {

            if (level == LogLevel.Off)
            {
                _ = LogManager.SuspendLogging();
            }
            else
            {
                if (!LogManager.IsLoggingEnabled())
                {
                    LogManager.ResumeLogging();
                }

                int maxLogLevelOrdinal = LogLevel.AllLoggingLevels.Max(p => p.Ordinal);
                int minLogLevelOrdinal = LogLevel.AllLoggingLevels.Min(p => p.Ordinal);

                foreach (var rule in LogManager.Configuration.LoggingRules)
                {
                    rule.DisableLoggingForLevels(LogLevel.FromOrdinal(minLogLevelOrdinal), level);
                    rule.EnableLoggingForLevels(level, LogLevel.FromOrdinal(maxLogLevelOrdinal));
                }
            }

            LogManager.ReconfigExistingLoggers();
        }

        public void AddDinamicFilter(string filterCondition)
        {
            var filter = new ConditionBasedFilter();
            filter.Condition = filterCondition;
            filter.Action = FilterResult.Ignore;

            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.Filters.Add(filter);
            }

            LogManager.ReconfigExistingLoggers();
        }

        public void RemoveDinamicFilter(string filterCondition)
        {
            //string filterCondition = "equals('${logger}','NetworkLogger')";

            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                var filterToRemove = rule.Filters.Where(filter => (filter as ConditionBasedFilter).Condition.ToString().RemoveWhitespace() == filterCondition).FirstOrDefault();

                if (filterToRemove != null)
                {
                    rule.Filters.Remove(filterToRemove);
                }
            }

            LogManager.ReconfigExistingLoggers();
        }
    }

    public static class StringExtension
    {
        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }
    }
}
