using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class LinkResolverService : ContentServiceBase, ILinkResolverService
    {
        protected ExpandComponent _expandComponent;

        public LinkResolverService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            ExpandComponent expandComponent) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _expandComponent = expandComponent;
        }

        public async Task<string> GetLinkedRecord(ParentLink parentLink, string infoAreaId, CancellationToken cancellationToken, RequestMode requestMode = RequestMode.Best)
        {
            if(parentLink != null && !string.IsNullOrWhiteSpace(infoAreaId))
            {
                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, cancellationToken).ConfigureAwait(false);

                if(tableInfo != null && tableInfo.Fields.Count > 0)
                {
                    var rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails
                        {
                            TableInfo = tableInfo,
                            Fields = new List<FieldControlField>()
                        },
                        parentLink, 1, requestMode);

                    if (rawData.Result != null && rawData.Result.Rows?.Count == 1)
                    {
                        var row = rawData.Result.Rows[0];
                        var record = row.GetColumnValue("recid", "-1");
                        return record.FormatedRecordId(infoAreaId);
                    }

                }

            }
            return null;
        }

        public async override Task PrepareContentAsync(CancellationToken cancellationToken)
        {
           
        }

        public async Task<string> GetLinkedRecordForAction(UserAction action, string parentLink, CancellationToken token)
        {
            var recordId = action.RecordId;
            var linkId = action.GetLinkId();
            int intLinkId;
            if(!int.TryParse(linkId, out intLinkId))
            {
                intLinkId = -1;
            }
            var recordInfoarea = action.SourceInfoArea;

            if(!string.IsNullOrEmpty(parentLink) && !parentLink.Equals(recordInfoarea))
            {
                var parentLinkObj = new ParentLink()
                {
                    ParentInfoAreaId = recordInfoarea,
                    RecordId = recordId,
                    LinkId = intLinkId
                };
                recordId = await GetLinkedRecord(parentLinkObj, parentLink, token);
            }

            return recordId.FormatedRecordId(parentLink);
        }

        public async Task<UserAction> ResolveRecordSwitch(UserAction userAction, CancellationToken token)
        {
            if(userAction != null)
            {
                var recordIdentification = userAction.RecordId;
                var sourceInfoArea = userAction.SourceInfoArea;
                var link = userAction.ViewReference.GetArgumentValue("Link");

                if (!string.IsNullOrWhiteSpace(link))
                {
                    int linkId = -1;
                    var linkParts = link.Split('#');

                    if(linkParts.Length > 1)
                    {
                        if (!int.TryParse(linkParts[1], out linkId))
                        {
                            linkId = -1;
                        }
                    }
                    string linkInfoAreaId = linkParts[0];
                    var existsAction = userAction.ViewReference.GetArgumentValue("ExistsAction");
                    var notExistsAction = userAction.ViewReference.GetArgumentValue("NotExistsAction");
                    var RecordIdType = userAction.ViewReference.GetArgumentValue("RecordId");
                    var requestMode = userAction.ViewReference.GetRequestMode(RequestMode.Best);

                    if (string.IsNullOrWhiteSpace(existsAction) || string.IsNullOrWhiteSpace(notExistsAction))
                    {
                        return userAction;
                    }

                    var parentLinkObj = new ParentLink()
                    {
                        ParentInfoAreaId = sourceInfoArea,
                        RecordId = recordIdentification,
                        LinkId = linkId
                    };

                    var linkRecordId = await GetLinkedRecord(parentLinkObj, linkInfoAreaId, token, requestMode);

                    Menu menu;
                    var recordId = recordIdentification;
                    var infoAreaId = sourceInfoArea;
                    if (string.IsNullOrWhiteSpace(linkRecordId))
                    {
                        menu = await _configurationService.GetMenu(notExistsAction, token);
                    }
                    else
                    {
                        menu = await _configurationService.GetMenu(existsAction, token);
                        recordId = linkRecordId;
                        infoAreaId = linkInfoAreaId;
                    }

                    if (menu?.ViewReference != null)
                    {
                        UserAction ua = _userActionBuilder.UserActionFromMenu(_configurationService, menu);
                        ua.RecordId = recordId;
                        ua.SourceInfoArea = infoAreaId;
                        ua.ActionType = _userActionBuilder.ResolveActionType(menu.ViewReference);
                        return ua;
                    }

                }
            }

            return userAction;
        }
    }
}
