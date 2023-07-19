namespace ACRM.mobile.Domain.SettingsHelper
{
    public class SettingsConfiguration
    {
        public ViewMode ViewMode { get; set; }
        public Logging LoggingSettings { get; set; }
    }

    public class ViewMode
    {
        public bool HideEmptyFields { get; set; }
        public bool PresentationMode { get; set; }
    }

    public class Logging
    {
        public bool OverrideServerLogLevel { get; set; }
        public LogLevel LogLevel { get; set; }
        public bool RequestLog { get; set; }
        public bool ConfigLog { get; set; }
        public bool NetworkLog { get; set; }
        public bool ResultLog { get; set; }
        public bool QuestionnaireLog { get; set; }
        public bool QuickAddLog { get; set; }
        public bool UpSyncLog { get; set; }
    }

    public enum LogLevel
    {
        Debug,
        Information,
        Error,
        Critical
    }
}
