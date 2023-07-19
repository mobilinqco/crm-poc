using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IFilterProcessor
    {
        QueryFilterBase ResolveFilterTokens(QueryFilterBase filter, bool isForTemplateFilter = false);
        List<Filter> ResolveFiltersTokens(List<Filter> filters);
        Task<List<Filter>> RetrieveFilterDetails(List<string> filterNames, CancellationToken cancellationToken, bool isForTemplateFilter = false);
        Task<Filter> RetrieveFilterDetails(string filterName, CancellationToken cancellationToken, bool isForTemplateFilter = false);
        List<string> RetrieveEnabledFiltersNames(ActionTemplateBase actionTemplate);
        Task<Dictionary<string, string>> FilterToTemplateDictionary(Filter filter, CancellationToken cancellationToken);
        List<string> RetrieveUserFiltersNames(ActionTemplateBase actionTemplate);
        List<string> RetrieveUserSearchFiltersNames(ActionTemplateBase actionTemplate);
        void SetAdditionalFilterParams(Dictionary<string, string> filterAdditionalParems);
        Task<List<EditTriggerUnit>> PrepareEditTriggerUnits(string filterName, CancellationToken cancellationToken);
        List<string> RetrievePositionFilterNames(ActionTemplateBase actionTemplate);
        QueryFilterBase ExpandRootTable(QueryFilterBase filter);
        Task<string> CheckFieldClientFilter(ListDisplayField field, Filter clientFilter, Dictionary<string, string> otherValues, CancellationToken token);
        List<(string, string)> RetrieveDistanceFilterNames(ActionTemplateBase actionTemplate);
        Task ExtractTabFilters(TabDataWithConfig tab, CancellationToken cancellationToken);
    }
}
