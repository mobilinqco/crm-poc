using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services
{
    public class QuickSearchService : ContentServiceBase,IQuickSearchService
    {
        private Dictionary<string, QuickSearchInfoAreaData> _infoAreaEntries;
        protected ISearchContentService _searchService;
        public QuickSearchService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            ISearchContentService searchService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _searchService = searchService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                var quickSearch = await _configurationService.GetQuickSearchConfig(cancellationToken);
                var actionTemplate = await ActionTemplateUtility.ResolveActionTemplate(_action, cancellationToken, _configurationService);
                _infoAreaEntries = new Dictionary<string, QuickSearchInfoAreaData>();
                if (quickSearch?.Entries?.Count > 0)
                {
                    foreach (var entry in quickSearch?.Entries)
                    {

                        if (_infoAreaEntries.ContainsKey(entry.InfoAreaId))
                        {
                            _infoAreaEntries[entry.InfoAreaId].Entries.Add(entry);
                        }
                        else
                        {
                            _infoAreaEntries.Add(entry.InfoAreaId, new QuickSearchInfoAreaData() { InfoAreaID = entry.InfoAreaId });
                            _infoAreaEntries[entry.InfoAreaId].Entries.Add(entry);
                        }
                    }

                    foreach (var key in _infoAreaEntries.Keys.ToList())
                    {
                        _infoAreaEntries[key].FieldControl = await _configurationService.GetFieldControl(key + ".List", cancellationToken);
                        _infoAreaEntries[key].TableInfo = await _configurationService.GetTableInfoAsync(key, cancellationToken);
                        _infoAreaEntries[key].InfoArea = _configurationService.GetInfoArea(key);
                        _infoAreaEntries[key].ActionTemplate = actionTemplate;
                        _infoAreaEntries[key].SearchControl = getSearchControl(_infoAreaEntries[key]);

                    }
                }
            }

        }

        private FieldControl getSearchControl(QuickSearchInfoAreaData quickSearchInfoAreaData)
        {
            var searchControl = new FieldControl();
            searchControl.ControlName = quickSearchInfoAreaData.InfoAreaID;
            searchControl.InfoAreaId = quickSearchInfoAreaData.InfoAreaID;
            searchControl.Tabs = new List<FieldControlTab>();
            var tab = new FieldControlTab();
            tab.Fields = new List<FieldControlField>();
            foreach (var entry in quickSearchInfoAreaData.Entries)
            {
                tab.Fields.Add(new FieldControlField
                {
                    FieldId = entry.FieldId,
                    InfoAreaId = entry.InfoAreaId,
                });
            }
            searchControl.Tabs.Add(tab);
            return searchControl;
        }

        public async Task<List<ListDisplayRow>> PerformQuickSearch(string globalSearchText, CancellationToken token)
        {
            List<ListDisplayRow> searchResults = new List<ListDisplayRow>();

            if (_infoAreaEntries?.Keys?.Count > 0)
            {
                foreach(var key in _infoAreaEntries?.Keys.ToList())
                {

                    List<ListDisplayRow> results = await _searchService.GetQuickSearchResult(globalSearchText,_infoAreaEntries[key], token);
                    if(results?.Count > 0)
                    {
                        searchResults.AddRange(results);
                    }
                }

            }

            return searchResults;
        }
    }
}
