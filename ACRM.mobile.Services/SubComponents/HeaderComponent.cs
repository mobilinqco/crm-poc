using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services.SubComponents
{
    public class HeaderComponent
    {
        private readonly IConfigurationService _configurationService;
        private readonly ICrmDataService _crmDataService;
        private readonly ILogService _logService;
        private readonly IUserActionBuilder _userActionBuilder;

        private Header _header;
        private UserAction _action;

        public Header Header
        {
            get
            {
                return _header;
            }
        }

        public HeaderComponent(IConfigurationService configurationService,
            ICrmDataService crmDataService,
            IUserActionBuilder userActionBuilder,
            ILogService logService)
        {
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _userActionBuilder = userActionBuilder;
            _logService = logService;
        }

        public void InitializeContext(Header header, UserAction action)
        {
            _header = header;
            _action = action;
        }

        public async Task<List<UserAction>> HeaderButtons(CancellationToken cancellationToken, string recordId = null, string recordInfoArea = null, string rawRecordId = null, bool isRecordRetrievedOnline = false)
        {
            List<UserAction> buttons = new List<UserAction>();

            if (_header == null)
            {
                return buttons;
            }

            List<string> buttonNames = _configurationService.ExtractConfigFromJsonString(_header.ButtonNames);

            foreach (var buttonName in buttonNames)
            {
                Button button = await _configurationService.GetButton(buttonName, cancellationToken);
                if (button != null
                    && !button.UnitName.StartsWith("GroupStart")
                    && !button.UnitName.StartsWith("GroupEnd"))
                {
                    buttons.Add(_userActionBuilder.UserActionFromButton(_configurationService, button, recordId, recordInfoArea, rawRecordId));
                }
            }

            return buttons;
        }

        public List<UserAction> HeaderRelatedInfoAreas(UserAction action = null)
        {
            string recordId = null;
            if (action != null)
            {
                recordId = action.RecordId;
            }
            List<UserAction> tabs = new List<UserAction>();
            if (_action == null)
            {
                return tabs;
            }

            UserAction defaultAction = null;

            if (_action.IsRecordListView())
            {
                defaultAction = new UserAction
                {
                    ActionDisplayName = "All",
                    ActionTaget = UserActionTarget.Tab,
                    ActionType = UserActionType.RecordLists,
                    ActionUnitName = action != null ? action.ActionUnitName : "",
                    SourceInfoArea = _header != null ? _header.InfoAreaId : "",
                    InfoAreaUnitName = action != null ? action.InfoAreaUnitName : "",
                    SubActionUnitId = -1,
                    RecordId = recordId,
                    ViewReference = action != null ? action.ViewReference : null,
                    IsSelected = true,
                    ResolvedExpandName = action != null ? action.ResolvedExpandName : null

                };

            }
            else
            {
                defaultAction = new UserAction
                {
                    ActionDisplayName = "Overview",
                    ActionTaget = UserActionTarget.Tab,
                    ActionType = UserActionType.ShowRecord,
                    ActionUnitName = action != null ? action.ActionUnitName : "",
                    SourceInfoArea = _header != null ? _header.InfoAreaId : "",
                    InfoAreaUnitName = action != null ? action.InfoAreaUnitName : "",
                    SubActionUnitId = -1,
                    RecordId = recordId,
                    ViewReference = action != null ? action.ViewReference : null,
                    IsSelected = true,
                    ResolvedExpandName = action != null ? action.ResolvedExpandName : null
                };
                
            }
            bool selectedTabSet = false;
            if(_action?.ViewReference?.GetArgumentValue("skipDefaultTab") != "true")
            {
                tabs.Add(defaultAction);
                selectedTabSet = true;
            }
            
            if (_header != null)
            {
                foreach (var infoAreaSubView in _header.SubViews.OrderBy(sv => sv.OrderId))
                {
                    string resolvedExpand = null;
                    if (action != null && action.InfoAreaUnitName.Equals(infoAreaSubView.InfoAreaId))
                    {
                        resolvedExpand = action.ResolvedExpandName;
                    }

                    tabs.Add(new UserAction
                    {
                        ActionDisplayName = infoAreaSubView.Label,
                        ActionTaget = UserActionTarget.Tab,
                        ActionType = _userActionBuilder.ResolveActionType(infoAreaSubView.ViewReference),
                        ActionUnitName = "",
                        SubActionUnitId = infoAreaSubView.Id,
                        SourceInfoArea = _header.InfoAreaId,
                        ViewReference = infoAreaSubView.ViewReference,
                        RecordId = recordId,
                        InfoAreaUnitName = infoAreaSubView.InfoAreaId,
                        IsSelected = !selectedTabSet,
                        ResolvedExpandName = resolvedExpand
                    });

                    selectedTabSet = true;
                }
            }

            return tabs;
        }

        public List<UserAction> HeaderRelatedAction(UserAction action = null)
        {
            string recordId = null;
            string recordSourceInfoArea = string.Empty;
            List<UserAction> tabs = new List<UserAction>();
            if (action != null)
            {
                recordId = action.RecordId;
                recordSourceInfoArea = action.SourceInfoArea;
            }
            else if (_action != null)
            {
                recordId = _action.RecordId;
                recordSourceInfoArea = _action.SourceInfoArea;
            }

            if (_header != null)
            {
                foreach (var infoAreaSubView in _header.SubViews.OrderBy(sv => sv.OrderId))
                {
                    string resolvedExpand = null;
                    if(action != null && action.InfoAreaUnitName.Equals(infoAreaSubView.InfoAreaId))
                    {
                        resolvedExpand = action.ResolvedExpandName;
                    }

                    tabs.Add(new UserAction
                    {
                        ActionDisplayName = infoAreaSubView.Label,
                        ActionTaget = UserActionTarget.Tab,
                        ActionType = _userActionBuilder.ResolveActionType(infoAreaSubView.ViewReference),
                        ActionUnitName = "",
                        SubActionUnitId = infoAreaSubView.Id,
                        SourceInfoArea = recordSourceInfoArea,
                        ViewReference = infoAreaSubView.ViewReference,
                        RecordId = recordId,
                        InfoAreaUnitName = infoAreaSubView.InfoAreaId,
                        IsSelected = false,
                        ResolvedExpandName = resolvedExpand
                    });
                }
            }

            return tabs;
        }
    }
}
