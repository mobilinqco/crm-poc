using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Doman.ActionTemplates;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using Newtonsoft.Json;

namespace ACRM.mobile.Services.SubComponents
{
    public class ExpandComponent
    {
        private readonly IConfigurationService _configurationService;
        private readonly ImageResolverComponent _imageResolverComponent;
        private readonly ILogService _logService;
        private List<Expand> _allInfoAreaExpands;
        private readonly IRuleProcessor _ruleProcessor;
        private Expand _resolvedExpand = null;
        private List<ExpandRule> _rules = null;
        private int _configSearchMode = 0;
        private readonly bool _configNoColorOnDefault;
        private readonly bool _configNoColorOnVirtualInfoAreaIds;
        private ConcurrentDictionary<string, (string image, string glyph)> _resourceCache = new ConcurrentDictionary<string, (string image, string glyph)>();


        private TableInfo _tableInfo;
        public TableInfo TableInfo
        {
            get
            {
                return _tableInfo;
            }
        }

        private string _expandName;
        public string ExpandName
        {
            get => _expandName;
            set
            {
                _expandName = value;
                InitializeExpand(_expandName);
            }
        }

        public bool IsExpandResolved
        {
            get => _resolvedExpand != null;
        }

        public ExpandComponent(IConfigurationService configurationService,
            IRuleProcessor ruleProcessor,
            ImageResolverComponent imageResolverComponent,
            ILogService logService)
        {
            _configurationService = configurationService;
            _logService = logService;
            _imageResolverComponent = imageResolverComponent;
            _ruleProcessor = ruleProcessor;

            _configSearchMode = _configurationService.GetNumericConfigValue("Search.Mode", 0);
            _configNoColorOnDefault = _configurationService.GetBoolConfigValue("Search.NoColorOnDefault");
            _configNoColorOnVirtualInfoAreaIds = _configurationService.GetBoolConfigValue("Search.NoColorOnVirtualInfoAreaIds");

        }

        public void InitializeContext(string expandName, string infoAreaId, TableInfo tableInfo)
        {
            _tableInfo = tableInfo;
            _allInfoAreaExpands = _configurationService.GetInfoAreaExpands(infoAreaId);
            ExpandName = expandName;
        }
        private void InitializeExpand(string expandName)
        {
            if (!string.IsNullOrEmpty(expandName))
            {
                var expand = _allInfoAreaExpands.Find(e => e.UnitName == expandName);
                if (expand != null)
                {
                    _rules = ExtractRulesJsonString(expand.AltenrateExpands);

                    if (_rules.Count == 0)
                    {
                        _resolvedExpand = expand;
                    }
                    else
                    {
                        _resolvedExpand = null;

                    }
                }
            }
        }

        private string GetExpandKey(DataRow row)
        {
            if (row != null && _rules?.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var rule in _rules)
                {
                    var columnName = "F" + rule.FieldId;
                    var value = GetColumnValue(row, columnName, "");
                    sb.Append(value);
                    sb.Append("$");

                }
                return sb.ToString();
            }

            return string.Empty;
        }


        public Expand GetDefaultInfoAreaExpand(string infoAreaId)
        {
            return _configurationService.GetInfoAreaExpands(infoAreaId).Find(e => e.UnitName == infoAreaId);
        }

        public bool IsExpandDefined(string expandName)
        {
            if (!string.IsNullOrEmpty(expandName))
            {
                var expand = _allInfoAreaExpands.Find(e => e.UnitName == expandName);
                if (expand != null)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetColorString(string expandName, DataRow row)
        {
            Expand expand = ResolveExpand(expandName, row);
            if (expand != null)
            {
                return expand.GetColorString();
            }

            return "#ffffffff";
        }

        public Expand ResolveExpand(string expandName, DataRow row)
        {
            if (!string.IsNullOrEmpty(expandName))
            {
                if (IsExpandResolved)
                {
                    return _resolvedExpand;
                }
                else
                {
                    string key = GetExpandKey(row);
                    var cExpand = _configurationService.GetExpandItem(expandName, key);
                    if (cExpand != null)
                    {
                        return cExpand;
                    }
                    else
                    {
                        var gExpand = ProcessExpand(expandName, row);
                        _configurationService.AddExpandItem(expandName, key, gExpand);
                        return gExpand;
                    }
                }
            }

            return null;
        }

        private Expand ProcessExpand(string expandName, DataRow row)
        {
            var expand = _allInfoAreaExpands.Find(e => e.UnitName == expandName);
            if (expand != null)
            {
                var rules = ExtractRulesJsonString(expand.AltenrateExpands);

                if (rules.Count == 0)
                {
                    return expand;
                }

                bool isAndConditionTrue = true;
                foreach (var rule in rules)
                {
                    FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, rule.FieldId);
                    if (fieldInfo != null)
                    {
                        var columnName = "F" + rule.FieldId;
                        var value = GetColumnValue(row, columnName, "");

                        if (rule.IsAndRule())
                        {
                            if (!_ruleProcessor.IsExpandRuleTrue(rule, fieldInfo, value))
                            {
                                isAndConditionTrue = false;
                            }
                        }
                        else
                        {
                            if (isAndConditionTrue && _ruleProcessor.IsExpandRuleTrue(rule, fieldInfo, value))
                            {
                                return ProcessExpand(rule.Action, row);
                            }
                            isAndConditionTrue = true;
                        }
                    }

                }
            }

            return expand;
        }

        public List<FieldControlField> GetExpandRuleFields(string expandName)
        {
            return GetExpandRuleFieldsRecursive(expandName, new List<string>());
        }

        private List<FieldControlField> GetExpandRuleFieldsRecursive(string expandName, List<string> visited)
        { 
            List<FieldControlField> fields = new List<FieldControlField>();

            if (!string.IsNullOrWhiteSpace(expandName))
            {
                var expand = _allInfoAreaExpands.Find(e => e.UnitName == expandName);
                if (expand != null)
                {
                    List<ExpandRule> rules = ExtractRulesJsonString(expand.AltenrateExpands);

                    if (rules.Count > 0)
                    {
                        foreach (var rule in rules)
                        {
                            FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, rule.FieldId);
                            if (fieldInfo != null)
                            {
                                fields.Add(new FieldControlField { FieldId = fieldInfo.FieldId,
                                    InfoAreaId = _tableInfo.InfoAreaId,
                                    Attributes = new List<FieldAttribute> { new FieldAttribute { AttributeType = (int)FieldAttributeType.Hide, Value = "true", } } });

                                if (!string.IsNullOrWhiteSpace(rule.Action))
                                {
                                    if (!visited.Exists(act => act.Equals(rule.Action)))
                                    {
                                        visited.Add(rule.Action);
                                        var ruleFields = GetExpandRuleFieldsRecursive(rule.Action, visited);
                                        if (ruleFields.Count > 0)
                                        {
                                            fields.AddRange(ruleFields);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return fields;
        }

        public string GetExpandColorString(Expand expand)
        {
            return string.IsNullOrEmpty(expand.ColorKey) ? "#ffffffff" : expand.ColorKey;
        }


        public List<ExpandRule> ExtractRulesJsonString(string value)
        {
            List<ExpandRule> result = new List<ExpandRule>();
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<ExpandRule>>(value);
                }
                catch (Exception ex)
                {
                    _logService.LogError($"ExtractRulesJsonString: Unable to extract expand alternatives data from {value}: {ex.Message}");
                }
            }

            return result;
        }

        private string GetColumnValue(DataRow row, string columnName, string defaultValue = " ")
        {
            if (row != null && row.Table.Columns.Contains(columnName) && !row.IsNull(columnName))
            {
                return row[columnName].ToString();
            }

            return defaultValue;
        }


        public RowDecorators GetRowDecorators(DataRow row, TabDataWithConfig tab)
        {
            int searchPageMode = tab.ActionTemplate.SearchPageMode();
            string dataRowInfoArea = row.GetRawInfoArea();
            RowDecorators rowDecorators = new RowDecorators { LeftMarginColor = "#E4E4E4" };
            Expand expand = null;

            if (searchPageMode < 0)
            {
                searchPageMode = _configSearchMode;
            }

            if (!_configNoColorOnDefault)
            {
                searchPageMode |= (int)SearchPageMode.ShowColorOnDefault;
            }

            if (!_configNoColorOnVirtualInfoAreaIds)
            {
                searchPageMode |= (int)SearchPageMode.ShowColorOnVirtualInfoArea;
            }

            if (!tab.InfoArea.IsSameInfoArea(dataRowInfoArea))
            {
                expand = GetDefaultInfoAreaExpand(dataRowInfoArea);
                if (expand != null && (searchPageMode & (int)SearchPageMode.ShowColorOnVirtualInfoArea) > 0)
                {
                    rowDecorators.LeftMarginColor = GetExpandColorString(expand);
                }
            }

            if (expand == null)
            {
                if (!tab.InfoArea.UnitName.Equals(ExpandName))
                {
                    InitializeContext(tab.InfoArea.UnitName, tab.InfoArea.UnitName, tab.TableInfo);
                }

                expand = ResolveExpand(tab.InfoArea.UnitName, row);

                if (expand != null)
                {
                    if (tab.InfoArea.IsSameInfoArea(expand.InfoAreaId))
                    {
                        if ((searchPageMode & (int)SearchPageMode.ShowColorOnDefault) > 0)
                        {
                            rowDecorators.LeftMarginColor = GetExpandColorString(expand);
                        }
                    }
                    else if ((searchPageMode & (int)SearchPageMode.IgnoreColors) == 0)
                    {
                        rowDecorators.LeftMarginColor = GetExpandColorString(expand);
                    }
                }
                else if ((searchPageMode & (int)SearchPageMode.ShowColorOnDefault) > 0)
                {
                    rowDecorators.LeftMarginColor = tab.InfoArea.PageAccentColor();
                }
            }

            if (expand != null && !string.IsNullOrWhiteSpace(expand.ImageName))
            {
                rowDecorators.Image = GetImage(expand.ImageName);
            }
            else
            {
                rowDecorators.Image = GetImage(dataRowInfoArea);
            }
            rowDecorators.Expand = expand;

            return rowDecorators;
        }

        private (string image, string glyph) GetImage(string imageName)
        {
            if (_resourceCache.ContainsKey(imageName))
            {
                return _resourceCache[imageName];
            }
            else
            {
                var Image = _imageResolverComponent.ImageTuple(imageName, _configurationService);
                _resourceCache.TryAdd(imageName, Image);
                return Image;
            }
        }
    }
}
