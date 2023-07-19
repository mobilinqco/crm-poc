using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class RepService: IRepService
    {
        private IConfigurationService _configurationService;
        private ICrmDataService _crmDataService;
        private ILogService _logService;
        private ICacheService _cacheService;
        private IFilterProcessor _filterProcessor;

        private SearchAndList _searchAndList;
        private FieldControl _listFieldControl;
        private InfoArea _infoArea;
        private DataResponse _rawData;

        public RepService(IConfigurationService configurationService,
            ICrmDataService crmDataService,
            IFilterProcessor filterProcessor,
            ICacheService cacheService,
            ILogService logService)
        {
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _filterProcessor = filterProcessor;
            _cacheService = cacheService;
            _logService = logService;
        }

        public async Task<List<CrmRep>> GetAllCrmReps(CancellationToken cancellationToken)
        {
            List<CrmRep> reps = (List<CrmRep>)_cacheService.GetItem(CacheItemKeys.CrmReps);

            if (reps == null)
            {
                reps = new List<CrmRep>();
                await PrepareContentAsync(cancellationToken);

                if (_rawData.Result != null)
                {
                    foreach (DataRow row in _rawData.Result.Rows)
                    {
                        if (row != null
                            && row.Table.Columns.Contains("recid")
                            && row.Table.Columns.Contains("F0")
                            && row.Table.Columns.Contains("F2")
                            && row.Table.Columns.Contains("F3")
                            && row.Table.Columns.Contains("F68"))
                        {
                            reps.Add(new CrmRep(row["F0"].ToString(),
                                row["F2"].ToString(),
                                row["F3"].ToString(),
                                row["recid"].ToString(),
                                row["F68"].ToString()));
                        }
                    }
                }

                _cacheService.AddItem(CacheItemKeys.CrmReps, reps);
            }

            return reps;
        }

        public async Task<string> GetRepName(string repId, CancellationToken cancellationToken)
        {
            List<CrmRep> reps = (List<CrmRep>)_cacheService.GetItem(CacheItemKeys.CrmReps);

            if (reps == null)
            {
                reps = await GetAllCrmReps(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.CrmReps, reps);
            }

            CrmRep rep = reps.Find(r => r.Id == CrmRep.FormatToAureaRepId(repId));
            if (rep != null)
            {
                return rep.Name;
            }

            return string.Empty;
        }

        public async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _infoArea = _configurationService.GetInfoArea("ID");

            _searchAndList = await _configurationService.GetSearchAndList("IDSystem", cancellationToken);
            if (_searchAndList != null)
            {
                _listFieldControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".List", cancellationToken);
            }
            else
            {
                _listFieldControl = await _configurationService.GetFieldControl("IDSystem.List", cancellationToken);
            }

            List<Filter> enabledDataFilters = new List<Filter>();

            if (!string.IsNullOrWhiteSpace(_searchAndList.FilterName))
            {
                enabledDataFilters.AddRange(await _filterProcessor.RetrieveFilterDetails(new List<string> { _searchAndList.FilterName }, cancellationToken));
            }

            TableInfo tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);

            if (_listFieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fields = _listFieldControl.Tabs[0].GetQueryFields();
                _rawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = tableInfo,
                        Fields = fields,
                        SortFields = _listFieldControl.SortFields,
                        Filters = enabledDataFilters
                    },
                    null,
                    100000);
            }
        }
    }
}
