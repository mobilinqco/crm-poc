using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.SerialEntry;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class PricingService : ContentServiceBase, IPricingService
    {
        protected Menu configMenu;
        List<PricingItem> priceListData;
        List<PricingItem> standardConditionData;
        List<PricingItem> specialOfferConditionData;
        List<PricingItem> companyConditionData;
        List<PricingItem> bundleData;

        string[] applyFunnctions;
        protected ISearchContentService _priceListSearchService;
        string bundleInfoAreaId;

        public PricingService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            ISearchContentService priceListSearchService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _priceListSearchService = priceListSearchService;
        }

        public async override Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                var configName = _action.ViewReference.GetArgumentValue("PricingConfiguration");
                if (!string.IsNullOrWhiteSpace(configName))
                {
                    configMenu = await _configurationService.GetMenu(configName, cancellationToken);
                    await LoadPricingConfigurations(cancellationToken);
                    var functionNameApplyOrderName = configMenu.ViewReference.GetArgumentValue("FunctionNameApplyOrder");

                    if (!string.IsNullOrEmpty(functionNameApplyOrderName))
                    {
                        applyFunnctions = functionNameApplyOrderName.Split(',');
                    }
                }
                
            }
        }

        private async Task LoadPricingConfigurations(CancellationToken cancellationToken)
        {
            if (configMenu?.ViewReference != null)
            {
                bundleData = await PreparePricingData("BundleConfigName", "BundleScaleConfigName", PricingItemType.BUNDLE_PRICING, cancellationToken);
                bundleInfoAreaId = LoadBundleInfoArea();
                priceListData = await PreparePricingData("PriceListConfigName", string.Empty, PricingItemType.PRICELIST, cancellationToken);
                standardConditionData = await PreparePricingData("ConditionConfigName", "ConditionScaleConfigName", PricingItemType.STANDARD_CONDITION, cancellationToken);
                specialOfferConditionData = await PreparePricingData("ActionConfigName", "ActionScaleConfigName", PricingItemType.SPECIAL_OFFER_CONDITION, cancellationToken);
                companyConditionData = await PreparePricingData("CompanyConfigName", "CompanyScaleConfigName", PricingItemType.ACCOUNT_CONDITION, cancellationToken);
            }
        }

        private string LoadBundleInfoArea()
        {
            return bundleData?.Count > 0 ? bundleData[0].InfoAreaId : string.Empty;
        }

        private async Task<List<PricingItem>> PreparePricingData(string configName, string scaleConfigName, PricingItemType itemType, CancellationToken cancellationToken)
        {
            var pricingItems = await getPricingRawData(configName, itemType, cancellationToken);

            if (!string.IsNullOrWhiteSpace(scaleConfigName) && pricingItems?.Count > 0)
            {
                List<PricingScaleItem> pricingScale = await getPricingScaleRawData(scaleConfigName, pricingItems[0].InfoAreaId, cancellationToken);

                if (pricingScale != null && pricingScale.Count > 0)
                {
                    foreach (var item in pricingItems)
                    {
                        var scaleItems = GetScaleItems(item, pricingScale);
                        if (scaleItems?.Count > 0)
                        {
                            item.ScaleItems = new List<PricingScaleItem>();
                            item.ScaleItems.AddRange(scaleItems);
                        }
                    }

                }
            }

            return pricingItems;
        }

        private List<PricingScaleItem> GetScaleItems(PricingItem item, List<PricingScaleItem> pricingScale)
        {
            if(pricingScale?.Count>0 && !string.IsNullOrEmpty(item.RecordIdentification))
            {
                var findScales = pricingScale.Where(x => x.ParentRecordIdentification.Equals(item.RecordIdentification)).ToList();
                return findScales;
            }

            return null;
        }

        private async Task<List<PricingScaleItem>> getPricingScaleRawData(string scaleConfigName,string parentInfoAreaId, CancellationToken cancellationToken)
        {
            List<PricingScaleItem> items = new List<PricingScaleItem>();
            var configName = configMenu.ViewReference.GetArgumentValue(scaleConfigName);
            if (string.IsNullOrEmpty(configName))
            {
                return items;
            }
            _priceListSearchService.SearchAndListName = configName;
            UserAction action = _userActionBuilder.UserActionFromMenu(_configurationService, configMenu);
            _priceListSearchService.SetSourceAction(action);
            _priceListSearchService.SetAdditionalFilterParams(_additionalParams);
            await _priceListSearchService.PrepareContentAsync(cancellationToken);
            
            var dtable = await _priceListSearchService.GetRecords(0, cancellationToken);
            var (fieldGroupComponent, fieldDefinitions) = _priceListSearchService.GetFieldConfig(0);

            if (dtable != null && dtable?.Rows.Count > 0)
            {
                var tasks = dtable.Rows.Cast<DataRow>().Select(async row => await GetPricingScaleRow(row, fieldGroupComponent, fieldDefinitions, parentInfoAreaId,cancellationToken));
                items.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
            }

            return items;
        }

        private async Task<PricingScaleItem> GetPricingScaleRow(DataRow row, FieldGroupComponent fieldGroupComponent, List<FieldControlField> fieldDefinitions,string parentInfoAreaId, CancellationToken cancellationToken)
        {
            PricingScaleItem item = new PricingScaleItem();
            item.InfoAreaId = fieldGroupComponent.TableInfo.InfoAreaId;
            item.RecordIdentification = row.GetColumnValue("recid", "-1");
            item.ParentRecordIdentification = row.GetColumnValue($"{parentInfoAreaId}_0_recId", "-1");

            var itemNo = await row.GetColumnValue(PricingItem.FUNCTIONNAME_ITEMNUMBER, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            if (string.IsNullOrEmpty(itemNo))
            {
                itemNo = await row.GetColumnValue(PricingItem.FUNCTIONNAME_ITEMNUMBER_ALT, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            }
            item.ItemNumber = itemNo;

            item.FunctionKeyPairs = new Dictionary<string, string>();
            foreach (var field in fieldDefinitions)
            {
                var key = field.Function;
                var value = await fieldGroupComponent.ExtractFieldRawValue(field, row, cancellationToken);
                if (!string.IsNullOrEmpty(key) && !item.FunctionKeyPairs.ContainsKey(key))
                {
                    item.FunctionKeyPairs.Add(key, value);
                }
            }
            return item;
        }

        private async Task<List<PricingItem>> getPricingRawData(string configNameParam, PricingItemType itemType, CancellationToken cancellationToken)
        {
            List<PricingItem> items = new List<PricingItem>();
            var configName = configMenu.ViewReference.GetArgumentValue(configNameParam);
            if (string.IsNullOrEmpty(configName))
            {
                return items;
            }
            _priceListSearchService.SearchAndListName = configName;
            UserAction action = _userActionBuilder.UserActionFromMenu(_configurationService, configMenu);
            _priceListSearchService.SetSourceAction(action);
            await _priceListSearchService.PrepareContentAsync(cancellationToken);
            _priceListSearchService.SetAdditionalFilterParams(_additionalParams);
            var dtable = await _priceListSearchService.GetRecords(0, cancellationToken);
            var (fieldGroupComponent, fieldDefinitions) = _priceListSearchService.GetFieldConfig(0);

            if (dtable != null && dtable?.Rows.Count > 0)
            {
                var tasks = dtable.Rows.Cast<DataRow>().Select(async row => await GetPricingRow(row, fieldGroupComponent, fieldDefinitions, itemType, cancellationToken));
                items.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
            }

            return items;
        }

        private async Task<PricingItem> GetPricingRow(DataRow row, FieldGroupComponent fieldGroupComponent, List<FieldControlField> fieldDefinitions, PricingItemType itemType, CancellationToken cancellationToken)
        {
            PricingItem item = new PricingItem();
            item.ItemType = itemType;
            item.InfoAreaId = fieldGroupComponent.TableInfo.InfoAreaId;
            item.RecordIdentification = row.GetColumnValue("recid", "-1");
            if(itemType == PricingItemType.SPECIAL_OFFER_CONDITION)
            {
                if (!string.IsNullOrEmpty(bundleInfoAreaId))
                {
                    var bundilRecId = row.GetColumnValue($"{bundleInfoAreaId}_0_recId", "-1");
                    if (!bundilRecId.Equals("-1"))
                    {
                        item.BundileRecordIdentification = bundilRecId;
                    }
                }
                

            }


            var itemNo = await row.GetColumnValue(PricingItem.FUNCTIONNAME_ITEMNUMBER, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            if (string.IsNullOrEmpty(itemNo))
            {
                itemNo = await row.GetColumnValue(PricingItem.FUNCTIONNAME_ITEMNUMBER_ALT, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            }
            item.ItemNumber = itemNo;

            item.FunctionKeyPairs = new Dictionary<string, string>();
            foreach (var field in fieldDefinitions)
            {
                var key = field.Function;
                var value = await fieldGroupComponent.ExtractFieldRawValue(field, row, cancellationToken);
                if (!string.IsNullOrEmpty(key) && !item.FunctionKeyPairs.ContainsKey(key))
                {
                    item.FunctionKeyPairs.Add(key, value);
                }
            }
            return item;
        }


        public async Task EvaluatePricing(SerialEntryItem item, int quntity, CancellationToken cancellationToken)
        {
           
            if (await EvaluatePricingData(item, specialOfferConditionData, quntity, cancellationToken))
            {
                return;
            }

            if (await EvaluatePricingData(item, companyConditionData, quntity, cancellationToken))
            {
                return;
            }
            
            if (await EvaluatePricingData(item, standardConditionData, quntity, cancellationToken))
            {
                return;
            }

            if (await EvaluatePricingData(item, priceListData, quntity, cancellationToken))
            {
                return;
            }

        }

        private async Task<bool> EvaluatePricingData(SerialEntryItem item, List<PricingItem> pricingItems, int quntity, CancellationToken cancellationToken)
        {
            try
            {



                if (pricingItems?.Count > 0 && item != null)
                {
                    PricingItem matchItem = await findMatchPriceItem(item, pricingItems);
                    if (matchItem != null)
                    {
                        PricingScaleItem scaleMatch;
                        scaleMatch = await GetScaleForQuantity(matchItem, item, quntity, cancellationToken);

                        if (scaleMatch != null)
                        {
                            scaleMatch = await GetScaleForPricing(matchItem, item, item.UnitPrice, cancellationToken);
                        }

                        if (scaleMatch != null)
                        {
                            item.UnitPrice = scaleMatch.UnitPrice > 0 ? scaleMatch.UnitPrice : item.UnitPrice;
                            item.Currency = !string.IsNullOrEmpty(matchItem.Currency) ? matchItem.Currency : item.Currency;
                            item.Discount = scaleMatch.Discount > 0 ? scaleMatch.Discount : item.Discount;
                            item.FreeGoods = scaleMatch.FreeGoods > 0 ? scaleMatch.FreeGoods : item.FreeGoods;
                        }
                        else
                        {
                            item.UnitPrice = matchItem.UnitPrice > 0 ? matchItem.UnitPrice : item.UnitPrice;
                            item.Currency = !string.IsNullOrEmpty(matchItem.Currency) ? matchItem.Currency : item.Currency;
                            item.Discount = matchItem.Discount > 0 ? matchItem.Discount : item.Discount;
                            item.FreeGoods = matchItem.FreeGoods > 0 ? matchItem.FreeGoods : item.FreeGoods;
                        }

                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"EvaluatePricingData failed with error {ex.Message}");
            }

            return false;
        }

        private async Task<PricingScaleItem> GetScaleForPricing(PricingItem matchItem, SerialEntryItem item, decimal price, CancellationToken cancellationToken)
        {
            List<PricingScaleItem> ScaleItems = matchItem?.ScaleItems;

            if (ScaleItems == null || ScaleItems.Count == 0)
            {
                if (matchItem?.ItemType == PricingItemType.SPECIAL_OFFER_CONDITION
                && !string.IsNullOrWhiteSpace(matchItem.BundileRecordIdentification)
                && bundleData?.Count > 0)
                {
                    var bundleScale = bundleData.Where(x => x.RecordIdentification == matchItem.BundileRecordIdentification).FirstOrDefault();
                    ScaleItems = bundleScale?.ScaleItems;
                }
            }

            if (ScaleItems?.Count > 0)
            {
                var scaleItem = ScaleItems.Where(x => x.MinPrice <= price && x.MaxPrice >= price).FirstOrDefault();
                return scaleItem;
            }
            return null;
        }

        private async Task<PricingScaleItem> GetScaleForQuantity(PricingItem matchItem, SerialEntryItem item, decimal quantity,CancellationToken cancellationToken)
        {

            List<PricingScaleItem> ScaleItems = matchItem?.ScaleItems;

            if (ScaleItems == null || ScaleItems.Count == 0)
            {
                if (matchItem?.ItemType == PricingItemType.SPECIAL_OFFER_CONDITION
                && !string.IsNullOrWhiteSpace(matchItem.BundileRecordIdentification)
                && bundleData?.Count > 0)
                {
                    var bundleScale = bundleData.Where(x => x.RecordIdentification == matchItem.BundileRecordIdentification).FirstOrDefault();
                    ScaleItems = bundleScale?.ScaleItems;
                }
            }

            if(ScaleItems?.Count>0)
            {
                var scaleItem = ScaleItems.Where(x => x.MinQuantity <= quantity && x.MaxQuantity >= quantity).FirstOrDefault();
                return scaleItem;
            }
            return null;
        }

        private async Task<PricingItem> findMatchPriceItem(SerialEntryItem item, List<PricingItem> pricingItems)
        {
            PricingItem matchItem =  pricingItems.Where(x => x.ItemNumber.Equals(item.ItemNumber) && !x.ItemNumber.Equals(string.Empty)).FirstOrDefault();

            if (matchItem == null && applyFunnctions?.Length > 0 && item.FunctionKeyPairs != null)
            {
                foreach (var fun in applyFunnctions)
                {
                    if (item.FunctionKeyPairs.ContainsKey(fun))
                    {
                        var itemValue = item.FunctionKeyPairs[fun];
                        return pricingItems.Where(x => x.FunctionKeyPairs.ContainsKey(fun) && x.FunctionKeyPairs[fun] == itemValue).FirstOrDefault();

                     }
                }
            }

            return matchItem;

        }
    }
}
