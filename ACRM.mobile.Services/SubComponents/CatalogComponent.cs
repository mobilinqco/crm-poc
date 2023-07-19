using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services.SubComponents
{
    public class CatalogComponent
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogService _logService;
        private readonly ISessionContext _sessionContext;

        public CatalogComponent(IConfigurationService configurationService,
            ISessionContext sessionContext,
            ILogService logService)
        {
            _configurationService = configurationService;
            _sessionContext = sessionContext;
            _logService = logService;
        }

        public async Task<List<SelectableFieldValue>> GetCatalogDisplayListAsync(FieldInfo fieldInfo, CancellationToken cancellationToken)
        {
            List<SelectableFieldValue> allowedValues = new List<SelectableFieldValue>();
            List<CatalogValue> catalogValues = await _configurationService.GetCatalogValuesForCatalogField(fieldInfo, cancellationToken).ConfigureAwait(false);
            if (catalogValues != null)
            {
                bool enableExplicitTenantCheck = _configurationService.GetBoolConfigValue("System.ExplicitCatalogTenantCheck");
                bool includeHidden = _configurationService.GetBoolConfigValue("Catalog.HideLockedInFilters", false) ? false : true;

                List<int> tenants = new List<int>();
                if (enableExplicitTenantCheck)
                {
                    tenants = _sessionContext.User.SessionInformation.AllUserTenants();
                }

                foreach (CatalogValue cv in SortedCatalogValues(catalogValues, fieldInfo.IsVariableCatalog))
                {
                    if ((includeHidden || cv.Access == 0) &&
                        (!enableExplicitTenantCheck || AllowedTenant(tenants, cv.Tenant)))
                    {
                        allowedValues.Add(new SelectableFieldValue { RecordId = cv.Code.ToString(), DisplayValue = cv.Text, Id = cv.Id, ParentCode = cv.ParentCode, ExtKey = cv.ExtKey });
                    }
                }
            }
            return allowedValues;
        }

        public async ValueTask<string> GetStringValueForCatalogField(DataRow row, string fieldName, FieldInfo field, CancellationToken cancellationToken)
        {
            string fieldValue = "";

            if (row != null && field.IsCatalog)
            {
                if (row.Table.Columns.Contains(fieldName) && !row.IsNull(fieldName))
                {
                    long longFieldValue = -1;
                    if (row[fieldName].GetType() == typeof(long))
                    {
                        longFieldValue = (long)row[fieldName];
                    }
                    else if (row[fieldName].GetType() == typeof(string))
                    {
                        if (!long.TryParse(row[fieldName].ToString(), out longFieldValue))
                        {
                            return "";
                        }
                    }

                    fieldValue = await GetCatalogValue(field, longFieldValue, cancellationToken);
                }
            }
            return fieldValue;
        }

        public async ValueTask<string> GetStringValueForCatalogField(string fieldStringLongVal, string fieldName, FieldInfo field, CancellationToken cancellationToken)
        {
            string fieldValue = "";

            if (field.IsCatalog)
            {
                long longFieldValue = -1;
                    
                if (!long.TryParse(fieldStringLongVal, out longFieldValue))
                {
                    return "";
                }    

                fieldValue = await GetCatalogValue(field, longFieldValue, cancellationToken);
            }
            return fieldValue;
        }

        private async Task<string> GetCatalogValue(FieldInfo field, long longFieldValue, CancellationToken cancellationToken)
        {
            string fieldValue = string.Empty;

            CatalogValue catalogValue = await _configurationService.GetCatalogValue(field.CatalogId(), unchecked((int)longFieldValue), field.IsVariableCatalog, cancellationToken);
            if (catalogValue != null)
            {
                fieldValue = catalogValue.Text;
            }
            else
            {
                if (longFieldValue > 0)
                {
                    fieldValue = longFieldValue.ToString();
                }
                else
                {
                    fieldValue = "";
                }

            }

            return fieldValue;
        }

        private bool AllowedTenant(List<int> tenants, int fieldTenantNo)
        {
            if (fieldTenantNo > 0)
            {
                foreach (int tenantNo in tenants)
                {
                    if (tenantNo == fieldTenantNo)
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }

        private List<CatalogValue> SortedCatalogValues(List<CatalogValue> catalogValues, bool isVariableCatalog)
        {
            if (catalogValues != null && catalogValues.Count > 0)
            {
                // SortFixCatBySortInfo:
                // true -> text (default)
                // false -> catalog code
                bool sortFixCatBySortInfo = _configurationService.GetBoolConfigValue("System.SortFixCatBySortInfo", true);
                // SortVarCatBySortInfo:
                // true -> catalog code
                // false -> text (default
                bool sortVarCatBySortInfo = _configurationService.GetBoolConfigValue("System.SortVarCatBySortInfo", false);

                if (_configurationService.GetConfigValue("System.DisplayFixCatBySortInfo") != null)
                {
                    sortFixCatBySortInfo = _configurationService.GetBoolConfigValue("System.DisplayFixCatBySortInfo", false);
                }

                if (isVariableCatalog)
                {
                    if (sortVarCatBySortInfo)
                    {
                        return catalogValues.OrderBy(cv => cv.SortInfo)
                            .ThenBy(cv => cv.Code)
                            .ToList();
                    }
                    else
                    {
                        return catalogValues.OrderBy(cv => cv.Text).ToList();
                    }
                }
                else
                { 
                    if(sortFixCatBySortInfo)
                    {
                        return catalogValues.OrderBy(cv => cv.Text).ToList();
                    }
                    else
                    {
                        return catalogValues.OrderBy(cv => cv.SortInfo)
                            .ThenBy(cv => cv.Code)
                            .ToList();
                    }
                }
            }

            return catalogValues;
        }

        internal async Task<string> GetCatalogValue(FieldInfo fieldInfo, string catalogCode, CancellationToken cancellationToken)
        {
            string fieldValue;
            long longFieldValue = -1;
            if (!long.TryParse(catalogCode, out longFieldValue))
            {
                return "";
            }
            CatalogValue catalogValue = await _configurationService.GetCatalogValue(fieldInfo.CatalogId(), unchecked((int)longFieldValue), fieldInfo.IsVariableCatalog, cancellationToken);
            if (catalogValue != null)
            {
                fieldValue = catalogValue.Text;
            }
            else
            {
                if (longFieldValue > 0)
                {
                    fieldValue = longFieldValue.ToString();
                }
                else
                {
                    fieldValue = "";
                }

            }

            return fieldValue;
        }
    }
}
