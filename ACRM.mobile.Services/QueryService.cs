using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Processors;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;
using Jint.Parser;
using NLog.Filters;
using Filter = ACRM.mobile.Domain.Configuration.UserInterface.Filter;

namespace ACRM.mobile.Services
{
    public class QueryService: ContentServiceBase, IQueryService
    {
        protected IFieldGroupDataService _fieldGroupDataService;
        protected QueryActionTemplate _queryActionTemplate = null;

        public QueryService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IFieldGroupDataService fieldGroupDataService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _fieldGroupDataService = fieldGroupDataService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            string queryName = _action.ActionUnitName;
            
            Query query = null;

            if(_action.ViewReference != null)
            {
                _queryActionTemplate = new QueryActionTemplate(_action.ViewReference);
                if(!string.IsNullOrWhiteSpace(_queryActionTemplate.Query()))
                {
                    queryName = _queryActionTemplate.Query();
                }
            }

            if(!string.IsNullOrWhiteSpace(queryName))
            {
                query = await _configurationService.GetQuery(queryName, cancellationToken);
            }

            if(query != null)
            {
                query = (Query)_filterProcessor.ExpandRootTable(query);

                if (!string.IsNullOrWhiteSpace(_action.RecordId) && _queryActionTemplate != null && !string.IsNullOrWhiteSpace(_queryActionTemplate.CopySourceRecordId()))
                {
                    _filterProcessor.SetAdditionalFilterParams(await _fieldGroupDataService.GetSourceFieldGroupData(_action.RecordId,
                        _queryActionTemplate.CopySourceFieldGroupName(), DetermineRequestMode(_queryActionTemplate), cancellationToken));
                }

                if (_action.AdditionalArguments != null)
                {
                    _filterProcessor.SetAdditionalFilterParams(_action.AdditionalArguments);
                }

                query = (Query)_filterProcessor.ResolveFilterTokens(query);
                string infoAreaId = query.RootTable.InfoAreaId;

                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, cancellationToken).ConfigureAwait(false);

                ParentLink parentLink = null;

                if (!string.IsNullOrWhiteSpace(_action.RecordId))
                {
                    parentLink = new ParentLink
                    {
                        LinkId = -1,
                        ParentInfoAreaId = infoAreaId,
                        RecordId = _action.RecordId
                    };
                }

                List<Filter> filters = new List<Filter>();
                filters.Add(new Filter { DisplayName = "Query", InfoAreaId = infoAreaId, RootTable = query.RootTable, Definition = query.Definition });

                try
                {
                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails
                        {
                            TableInfo = tableInfo,
                            Fields = await GetFields(query.QueryFields, cancellationToken),
                            SortFields = await GetSortFields(query.QueryFields, cancellationToken),
                            Filters = filters
                        },
                        parentLink, MaxResults(), InitialRequestMode());
                }
                catch (Exception ex)
                {
                    _logService.LogError("Unable to get query data");
                }
            }
        }

        public new string PageTitle()
        {
            string title = "Query";
            if(!string.IsNullOrWhiteSpace(_action.ActionDisplayName))
            {
                return _action.ActionDisplayName;
            }

            return "Query";
        }

        public DataTable GetData()
        {
            if(_rawData != null)
            {
                return _rawData.Result;
            }
            return null;
        }



        private async Task<List<FieldControlField>> GetFields(List<QueryField> queryFields, CancellationToken cancellationToken)
        {
            List<FieldControlField> fields = new List<FieldControlField>();

            if (queryFields != null)
            {
                foreach (QueryField qf in queryFields)
                {
                    TableInfo fieldTable = await _configurationService.GetTableInfoAsync(qf.TableAlias, cancellationToken).ConfigureAwait(false);
                    FieldInfo fieldInfo = fieldTable.GetFieldInfo(qf.FieldIndex);
                    if(fieldInfo != null)
                    {
                        fields.Add(FieldControlField.GetFieldControl(fieldInfo));
                    }
                    
                }
            }

            return fields;
        }

        private async Task<List<FieldControlSortField>> GetSortFields(List<QueryField> queryFields, CancellationToken cancellationToken)
        {
            List<FieldControlSortField> fields = new List<FieldControlSortField>();

            if (queryFields != null)
            {
                foreach (QueryField qf in queryFields)
                {
                    fields.Add(new FieldControlSortField { FieldIndex = qf.FieldIndex, InfoAreaId = qf.TableAlias, Descending = true });
                }
            }

            return fields;
        }

        private RequestMode InitialRequestMode()
        {
            if (_sessionContext.IsInOfflineMode || !_sessionContext.HasNetworkConnectivity)
            {
                return RequestMode.Offline;
            }

            if (_queryActionTemplate != null)
            {
                return _queryActionTemplate.GetRequestMode();
            }

            return RequestMode.Fastest;
        }

        private int MaxResults()
        {
            if(_queryActionTemplate != null)
            {
                return _queryActionTemplate.MaxResults();
            }

            return 100;
        }
    }
}

