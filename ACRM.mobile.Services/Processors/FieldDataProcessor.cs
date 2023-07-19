using System;
using ACRM.mobile.Domain.Application;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services.SubComponents
{
	public class FieldDataProcessor
	{
        private readonly IRepService _repService;
        private readonly CatalogComponent _catalogComponent;
        private readonly IConfigurationService _configurationService;

        public FieldDataProcessor(IConfigurationService configurationService,
            IRepService repService,
            CatalogComponent catalogComponent)
		{
            _configurationService = configurationService;
            _repService = repService;
            _catalogComponent = catalogComponent;
		}

        public async Task<string> ExtractDisplayValue(DataRow row, FieldInfo fieldInfo, PresentationFieldAttributes pfa, string fieldName, CancellationToken cancellationToken)
        {
            string fieldValue = row.GetColumnValue(fieldName);

            if (fieldInfo.IsCatalog)
            {
                fieldValue = await _catalogComponent.GetStringValueForCatalogField(row, fieldName, fieldInfo, cancellationToken).ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(fieldInfo.RepMode))
            {
                if (!fieldInfo.IsParticipant)
                {
                    fieldValue = await _repService.GetRepName(fieldValue, cancellationToken).ConfigureAwait(false);
                }
            }

            if (fieldInfo.IsBoolean)
            {
                fieldValue = ResolveBoolValue(fieldValue, fieldInfo, pfa);
            }

            return fieldValue;
        }

        public string ResolveBoolValue(string fieldValue, FieldInfo fieldInfo, PresentationFieldAttributes pfa)
        {
            bool showFieldNameForTrueValue = _configurationService.GetBoolConfigValue("Format.ShowFieldNameForTrueValue");
            bool showEmptyForFalse = _configurationService.GetBoolConfigValue("Format.EmptyForFalse");
            string extendedOption = pfa.ExtendedOptionForKey("ShowFieldNameForTrueValue");


            if (!string.IsNullOrEmpty(extendedOption))
            {
                if (extendedOption.ToLower().Equals("true"))
                {
                    showFieldNameForTrueValue = true;
                }
                else
                {
                    showFieldNameForTrueValue = false;
                }
            }

            if (fieldValue.ToLower().Equals("true") || fieldValue.ToLower().Equals("1"))
            {
                if (showFieldNameForTrueValue)
                {
                    if (pfa.ExplicitTrueValue != null)
                    {
                        return pfa.ExplicitTrueValue;
                    }
                    else
                    {
                        return pfa.Label();
                    }
                }
            }
            else
            {
                if (showEmptyForFalse)
                {
                    return string.Empty;
                }
                else
                {
                    if (showFieldNameForTrueValue)
                    {
                        if (pfa.ExplicitFalseValue != null)
                        {
                            return pfa.ExplicitFalseValue;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
            }

            return fieldValue;
        }
    }
}

