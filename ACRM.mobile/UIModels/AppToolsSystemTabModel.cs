using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;

namespace ACRM.mobile.UIModels
{
    public class AppToolsSystemTabModel: UIWidget
    {
        private IConfigurationService _configurationService;

        public string ApplicationName => "CRM.Client";

        private string _deviceModelText;
        public string DeviceModelText
        {
            get => _deviceModelText;
            set
            {
                _deviceModelText = value;
                RaisePropertyChanged(() => DeviceModelText);
            }
        }

        private string _operatingSystemText;
        public string OperatingSystemText
        {
            get => _operatingSystemText;
            set
            {
                _operatingSystemText = value;
                RaisePropertyChanged(() => OperatingSystemText);
            }
        }

        private string _appVersionText;
        public string AppVersionText
        {
            get => _appVersionText;
            set
            {
                _appVersionText = value;
                RaisePropertyChanged(() => AppVersionText);
            }
        }

        private string _aureaCRMVersionText;
        public string AureaCRMVersionText
        {
            get => _aureaCRMVersionText;
            set
            {
                _aureaCRMVersionText = value;
                RaisePropertyChanged(() => AureaCRMVersionText);
            }
        }

        private string _serverURLText;
        public string ServerURLText
        {
            get => _serverURLText;
            set
            {
                _serverURLText = value;
                RaisePropertyChanged(() => ServerURLText);
            }
        }

        private string _usernameText;
        public string UsernameText
        {
            get => _usernameText;
            set
            {
                _usernameText = value;
                RaisePropertyChanged(() => UsernameText);
            }
        }

        private string _rolesText;
        public string RolesText
        {
            get => _rolesText;
            set
            {
                _rolesText = value;
                RaisePropertyChanged(() => RolesText);
            }
        }

        private string _rightsText;
        public string RightsText
        {
            get => _rightsText;
            set
            {
                _rightsText = value;
                RaisePropertyChanged(() => RightsText);
            }
        }

        private string _configurationText;
        public string ConfigurationText
        {
            get => _configurationText;
            set
            {
                _configurationText = value;
                RaisePropertyChanged(() => ConfigurationText);
            }
        }

        private string _templateVersionText;
        public string TemplateVersionText
        {
            get => _templateVersionText;
            set
            {
                _templateVersionText = value;
                RaisePropertyChanged(() => TemplateVersionText);
            }
        }

        public AppToolsSystemTabModel(CancellationTokenSource parentCancellationTokenSource) : base(parentCancellationTokenSource)
        {
            _configurationService = AppContainer.Resolve<IConfigurationService>();
            InitProperties();
        }

        private void InitProperties()
        {
            DeviceModelText = DeviceInfo.Model;
            OperatingSystemText = GetOperatingSystemText();
            AppVersionText = GetAppVersionText();
            AureaCRMVersionText = _sessionContext.User.SessionInformation.Attributes.WebVersion;
            ServerURLText = _sessionContext.CrmInstance.Url;
            UsernameText = GetUsernameText();
            RolesText = _sessionContext.User.SessionInformation.Attributes.Roles;
            RightsText = _sessionContext.User.SessionInformation.Attributes.Rights;
            ConfigurationText = _sessionContext.User.SessionInformation.Attributes.ConfigurationNameNice;
            TemplateVersionText = _configurationService.GetConfigValue("Template.Version")?.Value;
        }

        private string GetOperatingSystemText()
        {
            return DeviceInfo.Platform + " (" + DeviceInfo.VersionString + ")";
        }

        private string GetAppVersionText()
        {
            if (DeviceInfo.Platform == DevicePlatform.UWP)
            {
                return ApplicationName +  " " + VersionTracking.CurrentVersion;
            }
            else
            {
                 return ApplicationName +  " " + VersionTracking.CurrentVersion +
                    " build " + VersionTracking.CurrentBuild;
            }
        }

        private string GetUsernameText()
        {
            return _sessionContext.User.SessionInformation.Attributes.RepName + " (" +
                _sessionContext.User.SessionInformation.Attributes.TenantNo + ")";
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }
    }
}
