using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using AsyncAwaitBestPractices;

namespace ACRM.mobile.Services
{
    public abstract class ContentServiceBase: IContentServiceBase
    {
        protected readonly IConfigurationService _configurationService;
        protected readonly ICrmDataService _crmDataService;
        protected readonly ILogService _logService;
        protected readonly ISessionContext _sessionContext;
        protected readonly IUserActionBuilder _userActionBuilder;
        protected readonly HeaderComponent _headerComponent;
        protected readonly FieldGroupComponent _fieldGroupComponent;
        protected readonly WeakEventManager _dataReadyEventManager = new WeakEventManager();
        protected readonly ImageResolverComponent _imageResolverComponent;
        protected readonly IFilterProcessor _filterProcessor;
        protected Dictionary<string, string> _additionalParams;

        protected UserAction _action;
        protected InfoArea _infoArea;
        protected DataResponse _rawData;
        protected List<UserAction> _headerButtons;

        public string InfoAreaId
        {
            get
            {
                return _infoArea?.UnitName;
            }
        }

        public event EventHandler DataReady
        {
            add => _dataReadyEventManager.AddEventHandler(value);
            remove => _dataReadyEventManager.RemoveEventHandler(value);
        }

        protected void OnDataReady() => _dataReadyEventManager.RaiseEvent(this, EventArgs.Empty, nameof(DataReady));

        public ContentServiceBase(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor)
        {
            _sessionContext = sessionContext;
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _logService = logService;
            _userActionBuilder = userActionBuilder;
            _headerComponent = headerComponent;
            _fieldGroupComponent = fieldGroupComponent;
            _imageResolverComponent = imageResolverComponent;
            _filterProcessor = filterProcessor;
        }

        abstract public Task PrepareContentAsync(CancellationToken cancellationToken);

        public void SetSourceAction(UserAction action)
        {
            _action = action;
        }

        public string PageAccentColor(InfoArea infoArea = null)
        {
            var infoAreaObj = _infoArea;
            if (infoArea != null)
            {
                infoAreaObj = infoArea;
            }
             
            if (infoAreaObj != null)
            {
                return infoAreaObj.PageAccentColor();
            }

            return "#E4E4E4";
        }

        public string PageTitle()
        {
            string title = string.Empty;
            if(_headerComponent.Header != null && !string.IsNullOrEmpty(_headerComponent.Header.Label))
            {
                return _headerComponent.Header.Label;
            }

            if (_infoArea != null)
            {
                if (_action.ActionType == UserActionType.ShowRecord)
                {
                    title = _infoArea.DetailsPageTitle();
                }
                else if (_action.ActionType == UserActionType.RecordLists)
                {
                    title = _infoArea.ListPageTitle();
                }
            }

            if(string.IsNullOrWhiteSpace(title))
            {
                if(_fieldGroupComponent != null
                    && _fieldGroupComponent.TableInfo != null
                    && _fieldGroupComponent.TableInfo.Name != null)
                {
                    title = _fieldGroupComponent.TableInfo.Name;
                }
                else
                {
                    title = string.Empty;
                }
            }

            if(string.IsNullOrWhiteSpace(title))
            {
                title = _action.ActionDisplayName;
            }

            return title;
        }

        public List<UserAction> HeaderRelatedInfoAreas()
        {
            return _headerComponent.HeaderRelatedInfoAreas();
        }

        public async Task<List<UserAction>> HeaderButtons(CancellationToken cancellationToken)
        {
            return await _headerComponent.HeaderButtons(cancellationToken);
        }

        public RequestMode DetermineRequestMode(ActionTemplateBase actionTemplate)
        {
            if(_sessionContext.IsInOfflineMode || !_sessionContext.HasNetworkConnectivity)
            {
                return RequestMode.Offline;
            }

            return actionTemplate.GetRequestMode();
        }

        public bool AreResultsRetrievedOnline()
        {
            if (_rawData != null)
            {
                return _rawData.IsRetrievedOnline;
            }
            return false;
        }

        public List<UserAction> HeaderButtons()
        {
            if (_headerButtons != null)
            {
                return _headerButtons;
            }

            return new List<UserAction>();
        }

        public void SetAdditionalParams(Dictionary<string, string> additionalParams)
        {
            _additionalParams = additionalParams;
        }
    }
}
