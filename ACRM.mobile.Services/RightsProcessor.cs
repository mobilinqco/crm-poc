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

namespace ACRM.mobile.Services
{
    public class RightsProcessor : ContentServiceBase, IRightsProcessor
    {
        public RightsProcessor(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
        }

        public async Task<(bool, bool, string)> EvaluateRightsFilter(UserAction userAction, CancellationToken cancellationToken, bool InvalidDefault = true, string filterNameKey = "")
        {
            bool result = false;
            bool status = false;
            string message = string.Empty;
            string rightsFilterName;

            if (string.IsNullOrWhiteSpace(filterNameKey))
            {
                rightsFilterName = userAction?.GetRightsFilter();
            }
            else
            {
                rightsFilterName = userAction?.ViewReference?.GetArgumentValue(filterNameKey);
            }

            if (!string.IsNullOrEmpty(rightsFilterName))
            {
                ParentLink rootRecord = _userActionBuilder.GetRootRecord(userAction);
                if (rootRecord != null && !string.IsNullOrWhiteSpace(rootRecord.RecordId))
                {
                    status = true;
                    (result, message) = await EvaluateRightsFilter(rightsFilterName, rootRecord.RecordId, cancellationToken, InvalidDefault);
                }
            }

            return (status, result, message);
        }

        public async Task<(bool, string)> EvaluateRightsFilter(string filterName, string rootRecordId, CancellationToken cancellationToken, bool InvalidDefault = true)
        {
            bool _permision = InvalidDefault;
            string _message;

            if (string.IsNullOrWhiteSpace(filterName))
            {
                return (_permision, "Invalid Filter");
            }

            var filter = await _filterProcessor.RetrieveFilterDetails(filterName, cancellationToken);

            if (filter == null)
            {
                return (_permision, "Invalid Filter");
            }

            var tableInfo = await _configurationService.GetTableInfoAsync(filter.InfoAreaId, cancellationToken);
            _message = filter.DisplayName;

            var rawData = await _crmDataService.GetData(cancellationToken,
                                    new DataRequestDetails
                                    {
                                        TableInfo = tableInfo,
                                        Fields = new List<FieldControlField>(),
                                        Filters = new List<Filter> { filter },
                                        RecordId = rootRecordId
                                    },
                                    null, 1, RequestMode.Best);;


            if (rawData?.Result != null
                && rawData.Result.Rows.Count > 0)
            {
                _permision = true;
            }
            else

            {
                _permision = false;
            }

            return (_permision, _message);
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

        }
    }
}
