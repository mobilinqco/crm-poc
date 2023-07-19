using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using Jint.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ACRM.mobile.Services.Processors
{
    public class FilterProcessor : IFilterProcessor
    {
        private readonly ITokenProcessor _tokenProcessor;
        private readonly ILogService _logService;
        private readonly IJSProcessor _jsProcessor;
        private readonly IConfigurationService _configurationService;
        private Dictionary<string, string> _filterAdditionalParams;
        private readonly ISessionContext _sessionContext;

        //private readonly List<TableInfo> _allTables;

        public FilterProcessor(ITokenProcessor tokenProcessor,
            IConfigurationService configurationService,
            ISessionContext sessionContext,
            ILogService logService,
            IJSProcessor jsProcessor)
        {
            _tokenProcessor = tokenProcessor;
            _logService = logService;
            _sessionContext = sessionContext;
            _configurationService = configurationService;
            _jsProcessor = jsProcessor;
        }

        public List<Filter> ResolveFiltersTokens(List<Filter> filters)
        {
            foreach (var filter in filters)
            {
                ResolveFilterTokens(filter);
            }

            return filters;
        }

        public QueryFilterBase ResolveFilterTokens(QueryFilterBase filter, bool isForTemplateFilter = false)
        {
            if (filter != null
                && filter.RootTable != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(filter.RootTable.Conditions))
                    {
                        filter.RootTable = ResolveConditionsTokens(filter.RootTable, isForTemplateFilter);
                    }

                    if (filter.RootTable.SubTables != null)
                    {
                        filter.RootTable.SubTables = ApplySubTablesConditions(filter.RootTable.SubTables, isForTemplateFilter);
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Filter processing request failed with {ex}");
                }
            }

            return filter;
        }

        public QueryFilterBase ExpandRootTable(QueryFilterBase filter)
        {
            if (filter != null
                && filter.Definition != null)
            {
                try
                {
                    filter.RootTable = JsonConvert.DeserializeObject<QueryTable>(filter.Definition);
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Filter: unable to expand the definition. Request failed with {ex}");
                }
            }

            return filter;
        }

        private List<QueryTable> ApplySubTablesConditions(List<QueryTable> subConditions, bool isForTemplateFilter = false)
        {
            if (subConditions != null)
            {
                List<QueryTable> resolved = new List<QueryTable>();
                foreach (var subCond in subConditions)
                {
                    
                    var resolv = new QueryTable
                    {
                        InfoAreaId = subCond.InfoAreaId,
                        LinkId = subCond.LinkId,
                        ParentRelation = subCond.ParentRelation,
                        Conditions = subCond.Conditions,
                        Alias = subCond.Alias
                    };

                    if (!string.IsNullOrEmpty(subCond.Conditions))
                    {
                        resolv = ResolveConditionsTokens(subCond, isForTemplateFilter);

                    }

                    if (resolv?.ExpandedConditions?.FunctionName?.StartsWith("RemoveInfoAreaIf") == true)
                    {
                        if (RemoveConditionForCondition(resolv?.ExpandedConditions))
                        {
                            return resolved;
                        }

                    }

                    if (resolv?.ExpandedConditions?.Conditions?.Count > 0)
                    {

                        var filteredConditions = new List<NodeCondition>();
                        foreach (var condition in resolv?.ExpandedConditions?.Conditions)
                        {
                            if (condition.FunctionName?.StartsWith("RemoveInfoAreaIf") == true)
                            {
                                if (RemoveConditionForCondition(condition))
                                {
                                    return resolved;
                                }

                            }
                            else
                            {
                                filteredConditions.Add(condition);
                            }

                            
                        }
                        if (filteredConditions.Count < resolv?.ExpandedConditions?.Conditions?.Count && filteredConditions.Count == 1)
                        {
                            resolv.ExpandedConditions = filteredConditions[0];
                            resolv.ExpandedConditions.Conditions = null;
                        }
                        else
                        {

                            resolv.ExpandedConditions.Conditions = filteredConditions;
                        }

                    }

                    if (subCond.SubTables != null)
                    {
                        resolv.SubTables = ApplySubTablesConditions(subCond.SubTables, isForTemplateFilter);
                    }

                    resolved.Add(resolv);
                }

                return resolved;
            }

            return subConditions;
        }

        private bool RemoveConditionForCondition(NodeCondition nodeCondition)
        {
            bool check0 = false;
            bool invert = false;
            bool checkValue = false;
            bool removeTable = false;

            var functionName = nodeCondition.FunctionName;

            if (functionName.Contains("Not"))
            {
                invert = true;
            }

            if (functionName.Contains("Or0"))
            {
                check0 = true;
            }

            if (functionName.Contains("HasValue"))
            {
                checkValue = true;
            }

            var newFieldValues = nodeCondition.FieldValues;
            var firstValue = nodeCondition.FirstValue();


            if (check0 && firstValue.Equals("0"))
            {
                removeTable = !invert;
            }
            else if (checkValue)
            {

                bool found = false;
                for (int i = 1; i < newFieldValues.Count; i++)
                {
                    if (firstValue.Equals(newFieldValues[i]))
                    {
                        found = true;
                        break;
                    }
                }

                removeTable = found ? !invert : invert;
            }
            else if (!checkValue && ( string.IsNullOrEmpty(firstValue) || firstValue.StartsWith("$cur") || firstValue.StartsWith("$par")))
            {
                removeTable = !invert;
            }
            else
            {
                removeTable = invert;
            }

            return removeTable;
            
        }

        private QueryTable ResolveConditionsTokens(QueryTable queryTable, bool isForTemplateFilter = false)
        {
            if (queryTable.ExpandedConditions == null)
            {
                var conditionsObject = JArray.Parse(queryTable.Conditions);
                queryTable.ExpandedConditions = new NodeCondition(conditionsObject, queryTable.InfoAreaId);

            }

            ResolveConditionsTokens(queryTable.ExpandedConditions, isForTemplateFilter);
            return queryTable;
        }

        private void ResolveConditionsTokens(NodeCondition condition, bool isForTemplateFilter = false)
        {
            if (!string.IsNullOrEmpty(condition.Relation))
            {
                switch (condition.Relation.ToUpper())
                {
                    case "AND":
                    case "OR":
                        foreach (var subCondition in condition.Conditions)
                        {
                            ResolveConditionsTokens(subCondition, isForTemplateFilter);
                        }
                        break;
                    case "LEAF":
                        condition.FieldValues = ResolveLeafConditionsTokens(condition, isForTemplateFilter);
                        break;
                    default:
                        break;

                }
            }
        }

        private List<string> ResolveLeafConditionsTokens(NodeCondition condition, bool isForTemplateFilter = false)
        {
            if (condition.FieldValues != null)
            {
                List<string> resolved = new List<string>();
                bool returnOnlyFirst = false;
                foreach (var val in condition.FieldValues)
                {
                    if (_tokenProcessor.IsOnlyFirst(val))
                    {
                        returnOnlyFirst = true;
                        continue;
                    }

                    if (_tokenProcessor.ISParValueToken(val))
                    {
                        resolved.Add(val);
                        continue;
                    }

                    string proccessedVal = val;

                    if (_tokenProcessor.IsToken(val))
                    {
                        proccessedVal = _tokenProcessor.TokenStringValue(val, isForTemplateFilter);
                    }

                    if (returnOnlyFirst)
                    {
                        if (!string.IsNullOrWhiteSpace(proccessedVal))
                        {
                            resolved.Add(proccessedVal);
                            break;
                        }
                    }
                    else
                    {
                        resolved.Add(proccessedVal);
                    }
                }
                return resolved;
            }
            return null;
        }

        public async Task<List<Filter>> RetrieveFilterDetails(List<string> filterNames, CancellationToken cancellationToken, bool isForTemplateFilter = false)
        {
            List<Filter> filters = new List<Filter>();
            if (filterNames != null && filterNames.Count > 0)
            {
                foreach (string filterName in filterNames.Distinct())
                {
                    Filter filter = await RetrieveFilterDetails(filterName, cancellationToken, isForTemplateFilter);

                    if (filter != null)
                    {
                        filters.Add(filter);
                    }
                }
            }

            return filters;
        }

        public async Task<Filter> RetrieveFilterDetails(string filterName, CancellationToken cancellationToken, bool isForTemplateFilter = false)
        {
            Filter outfilter = null;
            if (!string.IsNullOrWhiteSpace(filterName))
            {

                Filter filter = await _configurationService.GetFilter(filterName, cancellationToken);
                if (filter != null)
                {
                    filter = (Filter)ExpandRootTable(filter);
                    outfilter = (Filter)ResolveFilterTokens(filter, isForTemplateFilter);
                }
                else if (filterName.IsListing())
                {
                    outfilter = new Filter();
                    string[] strParts = filterName.Split(':');
                    if (strParts.Length > 1)
                    {
                        outfilter.UnitName = filterName;
                        outfilter.DisplayName = strParts[1];
                        outfilter.InfoAreaId = strParts[0];

                    }

                }
            }

            return outfilter;
        }

        public List<string> RetrieveUserFiltersNames(ActionTemplateBase actionTemplate)
        {
            List<string> filterNames = new List<string>();
            for (int i = 1; i <= 6; i++)
            {
                string filterName = actionTemplate.Filter(i);
                if (!string.IsNullOrEmpty(filterName))
                {
                    filterNames.Add(filterName);
                }
            }

            filterNames.AddRange(actionTemplate.AdditionalFilter());
            return filterNames;
        }

        public List<string> RetrievePositionFilterNames(ActionTemplateBase actionTemplate)
        {
            List<string> filterNames = new List<string>();
            for (int i = 1; i <= 6; i++)
            {
                string filterName = actionTemplate.PositionFilter(i);
                if (!string.IsNullOrEmpty(filterName))
                {
                    filterNames.Add(filterName);
                }
            }

            filterNames.AddRange(actionTemplate.AdditionalFilter());
            return filterNames;
        }

        public List<(string, string)> RetrieveDistanceFilterNames(ActionTemplateBase actionTemplate)
        {
            List<(string, string)> filterNames = new List<(string, string)>();
            for (int i = 1; i <= 6; i++)
            {
                var filterName = actionTemplate.DistanceFilter(i);
                if (!string.IsNullOrEmpty(filterName.Item1) && !string.IsNullOrEmpty(filterName.Item2))
                {
                    filterNames.Add(filterName);
                }
            }
            return filterNames;
        }

        public List<string> RetrieveUserSearchFiltersNames(ActionTemplateBase actionTemplate)
        {
            List<string> filterNames = new List<string>();
            for (int i = 1; i <= 6; i++)
            {
                string filterName = actionTemplate.SearchFilter(i);
                if (!string.IsNullOrEmpty(filterName))
                {
                    filterNames.Add(filterName);
                }
            }

            filterNames.AddRange(actionTemplate.AdditionalFilter());
            return filterNames;
        }

        public List<string> RetrieveEnabledFiltersNames(ActionTemplateBase actionTemplate)
        {
            List<string> filterNames = actionTemplate.EnabledFilter();

            if (!string.IsNullOrEmpty(actionTemplate.FilterName()))
            {
                filterNames.Add(actionTemplate.FilterName());
            }

            return filterNames;
        }

        public async Task<Dictionary<string, string>> FilterToTemplateDictionary(Filter filter, CancellationToken cancellationToken)
        {
            Dictionary<string, string> filterValues = new Dictionary<string, string>();
            List<EditTriggerUnit> triggers = null;
            if (filter != null)
            {
                // Prepare the JavaScript Processor
                await _jsProcessor.InitJSRuntime(cancellationToken);
                triggers = new List<EditTriggerUnit>();
                ProcessTableNode(filter.RootTable, ref triggers);
                foreach (var trigger in triggers)
                {
                    var evaluations = trigger.Evaluations;
                    if (trigger?.Evaluations?.Count > 0)
                    {
                        foreach (var key in trigger.Evaluations.Keys.ToList())
                        {
                            string result = trigger.Evaluations[key];
                            if (result.IsJsExpression() && (trigger?.FixedParameters?.Count > 0 || trigger?.VariableParameters?.Count > 0))
                            {
                                result = _jsProcessor.ExecuteJSEvaluation(result, trigger?.FixedParameters, trigger?.VariableParameters);
                            }

                            if (!filterValues.ContainsKey(key))
                            {
                                filterValues.Add(key, result);
                            }
                        }

                    }

                }

            }

            return filterValues;
        }

        private void ExtractConditions(NodeCondition condition, Dictionary<string, string> filterValues, string infoArea)
        {
            if (!string.IsNullOrEmpty(condition.Relation))
            {
                switch (condition.Relation.ToUpper())
                {
                    case "AND":
                        foreach (var subCondition in condition.Conditions)
                        {
                            ExtractConditions(subCondition, filterValues, infoArea);
                        }
                        break;
                    case "LEAF":
                        {
                            filterValues[$"{infoArea}_{condition.FieldId}"] = condition.FieldValues[0];
                        }
                        break;
                    default:
                        break;

                }
            }
        }

        private void ExtractSubConditions(List<QueryTable> subConditions, Dictionary<string, string> filterValues, string infoArea)
        {
            if (subConditions != null)
            {
                foreach (var subCond in subConditions)
                {
                    string localInfoArea = string.IsNullOrEmpty(subCond.InfoAreaId) ? infoArea : subCond.InfoAreaId;
                    if (subCond.ExpandedConditions != null)
                    {
                        ExtractConditions(subCond.ExpandedConditions, filterValues, localInfoArea);
                    }

                    if (subCond.SubTables != null)
                    {
                        ExtractSubConditions(subCond.SubTables, filterValues, localInfoArea);
                    }

                }
            }
        }

        public void SetAdditionalFilterParams(Dictionary<string, string> filterAdditionalParams)
        {
            if (_filterAdditionalParams == null)
            {
                _filterAdditionalParams = filterAdditionalParams;
            }
            else if (filterAdditionalParams.Keys?.Count > 0)
            {
                foreach (var item in filterAdditionalParams.ToList())
                {
                    if (_filterAdditionalParams.ContainsKey(item.Key))
                    {
                        _filterAdditionalParams[item.Key] = item.Value;
                    }
                    else
                    {
                        _filterAdditionalParams.Add(item.Key, item.Value);
                    }

                }
            }

            if (_filterAdditionalParams != null && _filterAdditionalParams.Keys.Count > 0)
            {
                _tokenProcessor.AddExtraParams(_filterAdditionalParams);
            }
        }

        public async Task<List<EditTriggerUnit>> PrepareEditTriggerUnits(string filterName, CancellationToken cancellationToken)
        {
            List<EditTriggerUnit> editTriggers = null;

            var filter = await RetrieveFilterDetails(filterName, cancellationToken);
            if (filter != null)
            {
                editTriggers = new List<EditTriggerUnit>();
                ProcessTableNode(filter.RootTable, ref editTriggers);
            }

            return editTriggers;
        }

        private void ProcessTableNode(QueryTable qTable, ref List<EditTriggerUnit> editTriggers)
        {
            if (qTable?.ExpandedConditions?.Conditions?.Count > 0)
            {
                var editTrigger = new EditTriggerUnit();
                foreach (var condition in qTable?.ExpandedConditions?.Conditions)
                {
                    ProcessCoditionNode(condition, qTable.InfoAreaId, ref editTrigger);
                }
                editTriggers.Add(editTrigger);
            }
            else if (qTable?.ExpandedConditions !=null)
            {
                var editTrigger = new EditTriggerUnit();
                ProcessCoditionNode(qTable?.ExpandedConditions, qTable.InfoAreaId, ref editTrigger);
                editTriggers.Add(editTrigger);
            }

            if (qTable?.SubTables?.Count > 0)
            {
                foreach (var table in qTable.SubTables)
                {
                    ProcessTableNode(table, ref editTriggers);
                }

            }
        }

        private void ProcessCoditionNode(NodeCondition condition, string infoAreaId, ref EditTriggerUnit editTriggerUnit)
        {
            if (condition != null)
            {
                if (condition.Relation.Equals("LEAF", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (condition.FunctionName.StartsWith("Parameter:Arguments", StringComparison.CurrentCultureIgnoreCase) && condition.FieldValues?.Count > 0)
                    {
                        foreach (var fieldValue in condition.FieldValues)
                        {
                            if (!string.IsNullOrWhiteSpace(fieldValue) && !editTriggerUnit.VariableParameters.Contains(fieldValue))
                            {
                                editTriggerUnit.VariableParameters.Add(fieldValue);
                            }
                        }
                    }
                    else if (condition.FunctionName.StartsWith("Parameter:Fixed", StringComparison.CurrentCultureIgnoreCase) && condition.FieldValues?.Count > 0)
                    {
                        foreach (var fieldValue in condition.FieldValues)
                        {
                            editTriggerUnit.FixedParameters.Add(fieldValue);
                        }
                    }
                    else if (condition.FieldId >= 0 && condition.FieldValues?.Count > 0)
                    {
                        var evaluationKey = $"{infoAreaId}_{condition.FieldId}";
                        var fieldValue = condition.FieldValues[0];
                        if (!string.IsNullOrWhiteSpace(fieldValue) && !editTriggerUnit.Evaluations.ContainsKey(evaluationKey))
                        {
                            editTriggerUnit.Evaluations.Add(evaluationKey, fieldValue);
                        }

                    }
                }
                else if (condition.Relation.Equals("AND", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var subCondition in condition?.Conditions)
                    {
                        ProcessCoditionNode(subCondition, infoAreaId, ref editTriggerUnit);
                    }
                }
            }
        }

        //Start: Client Filter Processing
        public async Task<string> CheckFieldClientFilter(ListDisplayField field, Filter clientFilter, Dictionary<string, string> otherValues, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(field.Config.FieldConfig.InfoAreaId))
            {
                QueryTable table = null;
                if (field.Config.FieldConfig.InfoAreaId.Equals(clientFilter.InfoAreaId))
                {
                    table = clientFilter.RootTable;
                }
                else
                {
                    if (clientFilter.RootTable != null && clientFilter.RootTable.SubTables != null)
                    {
                        foreach (var subTable in clientFilter.RootTable.SubTables)
                        {
                            if (field.Config.FieldConfig.InfoAreaId.Equals(subTable.InfoAreaId))
                            {
                                table = subTable;
                                break;
                            }
                        }
                    }
                }

                if (table != null)
                {
                    _logService.LogDebug($"Verifying the client filter for {field.Config.FieldConfig.FieldIdentification()}");
                    return await CheckFieldContidition(field, table, otherValues, token);
                }
            }

            return null;
        }

        private async Task<string> CheckFieldContidition(ListDisplayField field, QueryTable table, Dictionary<string, string> otherValues, CancellationToken token)
        {
            List<NodeCondition> conditions = table.GetCondition(field.Config.FieldConfig.FieldId, field.Config.FieldConfig.InfoAreaId);
            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    _logService.LogDebug($"Evaluating field {field.Config.FieldConfig.FieldIdentification()} for conditions {condition.GetJsonRepresentation()}");
                    if (!CheckNodeConditions(condition, field.Config.FieldConfig.FieldIdentification(), field.EditData.ValueForFilter(), otherValues))
                    {
                        return await GetConditionsErrorMessage(condition, field.Config.FieldConfig.FieldIdentification(), token);
                    }
                }
            }

            return null;
        }

        private bool CheckNodeConditions(NodeCondition node, string fieldIdentification, string value, Dictionary<string, string> otherValues)
        {
            if (node.Conditions == null || node.Conditions.Count == 0)
            {
                _logService.LogDebug($"Reached a leaf node condition {node.GetJsonRepresentation()}");

                string compareValue = value;
                if (!fieldIdentification.Equals(node.FieldIdentification()))
                {
                    if (otherValues.ContainsKey(node.FieldIdentification()))
                    {
                        compareValue = otherValues[node.FieldIdentification()];
                    }
                    else
                    {
                        compareValue = string.Empty;
                    }
                }

                return CheckLeafNodeValue(node, compareValue);
            }

            foreach (NodeCondition subCondition in node.Conditions)
            {
                bool subConditionResult = CheckNodeConditions(subCondition, fieldIdentification, value, otherValues);
                _logService.LogDebug($"Evaluation of subcondition {subCondition.GetJsonRepresentation()} returned {subConditionResult}");
                if (!subConditionResult)
                {
                    if (!node.IsOrCondition())
                    {
                        _logService.LogDebug($"Failed part of AND condition for {fieldIdentification} with value {value} and conditions{subCondition.GetJsonRepresentation()}");
                        return false;
                    }
                }
                else
                {
                    if (node.IsOrCondition())
                    {
                        _logService.LogDebug($"Successful part of OR condition for {fieldIdentification} with value {value} and conditions{subCondition.GetJsonRepresentation()}");
                        return true;
                    }
                }
            }

            if(node.IsOrCondition())
            {
                _logService.LogDebug($"Failed condition for {fieldIdentification} with value {value} and conditions{node.GetJsonRepresentation()}");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CheckLeafNodeValue(NodeCondition node, string value)
        {
            _logService.LogDebug($"Leaf node condition {node.GetJsonRepresentation()} with value {value}");

            if (!string.IsNullOrWhiteSpace(node.FunctionName) && node.FunctionName.ToLower().Equals("parameter:error"))
            {
                return true;
            }

            if (node.FieldValues.Count == 0)
            {
                return true;
            }

            bool isNumericCompare = false;

            foreach (string fieldValue in node.FieldValues)
            {
                if (!string.IsNullOrWhiteSpace(fieldValue) && fieldValue.StartsWith("$"))
                {
                    if (fieldValue.Equals("$compareNumber"))
                    {
                        isNumericCompare = true;
                    }
                    continue;
                }

                string compareValue = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    compareValue = string.Empty;
                }

                string fieldVal = fieldValue;
                if (string.IsNullOrWhiteSpace(fieldValue))
                {
                    fieldVal = string.Empty;
                }

                if (compareValue.CrmFilterCompare(fieldVal, node.CompareOperator, isNumericCompare))
                {
                    return true;
                }
            }

            _logService.LogDebug($"Failed leaf node condition {node.GetJsonRepresentation()} with value {value}");
            return false;
        }

        private async Task<string> GetConditionsErrorMessage(NodeCondition node, string fieldIdentification, CancellationToken token)
        {
            if (node.Conditions == null || node.Conditions.Count == 0)
            {
                if (node.FunctionName.StartsWith("Parameter:Error", StringComparison.CurrentCultureIgnoreCase) && node.FieldIdentification().Equals(fieldIdentification))
                {
                    if (!string.IsNullOrWhiteSpace(node.FirstValue()))
                    {
                        string[] parts = node.FirstValue().Split('.');
                        if (parts.Count() < 2)
                        {
                            return string.Empty;
                        }

                        FieldControl fieldControl = await _configurationService.GetFieldControl(parts[0] + ".List", token);
                        if (fieldControl != null)
                        {
                            return fieldControl.LabelTextForFunctionName(parts[1]);
                        }
                    }
                }

                return string.Empty;
            }


            foreach (NodeCondition subCondition in node.Conditions)
            {
                string errorMessage = await GetConditionsErrorMessage(subCondition, fieldIdentification, token);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return errorMessage;
                }
            }

            return string.Empty;
        }

        public async Task ExtractTabFilters(TabDataWithConfig tab, CancellationToken cancellationToken)
        {
            tab.EnabledDataFilters = new List<Filter>();
            tab.EnabledUserFilters = new List<Filter>();

            // Only the action template FilterName filter and the filter
            // defined in the Search and List could not be
            // disabled by the user.
            List<string> filterNames = new List<string>();
            if (!string.IsNullOrWhiteSpace(tab.SearchAndList.FilterName))
            {
                filterNames.Add(tab.SearchAndList.FilterName);
            }

            if (!string.IsNullOrEmpty(tab.ActionTemplate.FilterName()))
            {
                filterNames.Add(tab.ActionTemplate.FilterName());
            }

            if (tab.ActionTemplate is CalendarViewTemplate)
            {
                CalendarViewTemplate cvt = tab.ActionTemplate as CalendarViewTemplate;
                if (!string.IsNullOrWhiteSpace(cvt.RepFilter()))
                {
                    filterNames.Add(cvt.RepFilter());
                    SetAdditionalFilterParams(new Dictionary<string, string> { { "Rep", _sessionContext.User.SessionInformation.RepIdStr() } });
                }
            }

            tab.EnabledDataFilters.AddRange(await RetrieveFilterDetails(filterNames, cancellationToken));
            tab.UserFilters = new List<Filter>();

            if (tab.ActionTemplate is SerialEntryTemplate)
            {
                filterNames = RetrieveUserSearchFiltersNames(tab.ActionTemplate);
            }
            else
            {
                filterNames = RetrieveUserFiltersNames(tab.ActionTemplate);
            }

            tab.UserFilters.AddRange(await RetrieveFilterDetails(filterNames, cancellationToken));

            List<Filter> enabledFilters = await RetrieveFilterDetails(tab.ActionTemplate.EnabledFilter(), cancellationToken);
            foreach (Filter filter in enabledFilters)
            {
                Filter userFilter = tab.UserFilters.Find(f => filter.UnitName == f.UnitName);
                if (userFilter != null)
                {
                    tab.EnabledUserFilters.Add(userFilter);
                }
                else
                {
                    tab.UserFilters.Add(filter);
                    tab.EnabledUserFilters.Add(filter);
                }
            }
        }
    }
}
