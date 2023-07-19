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
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class QuotaService : ContentServiceBase, IQuotaService
    {
        protected Menu configMenu;
        List<QuotaArticleData> quotaArticleData;
        List<QuotaData> quotaData;
        protected ISearchContentService _articleSearchService;
        protected ISearchContentService _quotaSearchService;
        protected ILinkResolverService _linkResolverService;
        protected string _itemNumberFunctionName;
        protected string _rowItemNumberFunctionName;
        protected int _quotaLinkId = -1;
        protected string _parentLinkInfoArea;
        protected int _maxQuota = 0;

        bool IQuotaService.HasQuotaConfig => configMenu == null ? false : true;

        public QuotaService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            ISearchContentService quotaSearchService,
            ISearchContentService articleSearchService,
            ILinkResolverService linkResolverService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _quotaSearchService = quotaSearchService;
            _articleSearchService = articleSearchService;
            _linkResolverService = linkResolverService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                var configName = _action.ViewReference.GetArgumentValue("QuotaConfiguration");
                if (!string.IsNullOrWhiteSpace(configName))
                {
                    configMenu = await _configurationService.GetMenu(configName, cancellationToken);
                    _itemNumberFunctionName = configMenu.ViewReference.GetArgumentValue("ItemNumberFunctionName");
                    _rowItemNumberFunctionName = configMenu.ViewReference.GetArgumentValue("RowItemNumberFunctionName");
                    _parentLinkInfoArea = configMenu.ViewReference.GetArgumentValue("ParentLink");
                    
                    var quotaLinkId = configMenu.ViewReference.GetArgumentValue("QuotaLinkId");
                    if (!int.TryParse(quotaLinkId,out _quotaLinkId))
                    {
                        _quotaLinkId = -1;
                    }
                    var maxSamplesPerPeriod = configMenu.ViewReference.GetArgumentValue("MaxSamplesPerPeriod");
                    if (!int.TryParse(maxSamplesPerPeriod, out _maxQuota))
                    {
                        _maxQuota = 0;
                    }

                    await LoadQuotaConfigurations(cancellationToken);
                }

            }
        }

        private async Task LoadQuotaConfigurations(CancellationToken cancellationToken)
        {
            if (configMenu?.ViewReference != null)
            {
                await LoadArticleConfiguration(cancellationToken);
                await LoadQuotaConfiguration(cancellationToken);

            }
        }

        private async Task LoadQuotaConfiguration(CancellationToken cancellationToken)
        {
            var configName = configMenu.ViewReference.GetArgumentValue("QuotaConfigName");
            if (string.IsNullOrEmpty(configName))
            {
                return;
            }

            _quotaSearchService.SearchAndListName = configName;
            UserAction action = _userActionBuilder.UserActionFromMenu(_configurationService, configMenu);
            _quotaSearchService.SetSourceAction(action);
            _quotaSearchService.SetAdditionalFilterParams(_additionalParams);
            await _quotaSearchService.PrepareContentAsync(cancellationToken);
            var quotaLink = await getQuotaLink(cancellationToken);
            var dtable = await _quotaSearchService.GetRecords(0, cancellationToken, quotaLink);
            var (fieldGroupComponent, fieldDefinitions) = _quotaSearchService.GetFieldConfig(0);

            if (dtable != null && dtable?.Rows.Count > 0)
            {
                quotaData = new List<QuotaData>();
                var tasks = dtable.Rows.Cast<DataRow>().Select(async row => await GetQuotaDataRow(row, fieldGroupComponent, fieldDefinitions, cancellationToken));
                quotaData.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
            }
        }

        private async Task<ParentLink> getQuotaLink(CancellationToken cancellationToken)
        {
            ParentLink _parentLink = new ParentLink
            {
                LinkId = -1,
                ParentInfoAreaId = _action.SourceInfoArea,
                RecordId = _action.RecordId,
            };

            if (string.IsNullOrEmpty(_parentLinkInfoArea))
            {
                return _parentLink;
            }

            var recordId = await _linkResolverService.GetLinkedRecord(_parentLink, _parentLinkInfoArea, cancellationToken);

            if (!string.IsNullOrEmpty(recordId))
            {
                return new ParentLink() { LinkId = _quotaLinkId, ParentInfoAreaId = _parentLinkInfoArea, RecordId = recordId };
            }
            else
            {
                return null;
            }

        }

        private async Task<QuotaData> GetQuotaDataRow(DataRow row, FieldGroupComponent fieldGroupComponent, List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            QuotaData item = new QuotaData();
            item.InfoAreaId = fieldGroupComponent.TableInfo.InfoAreaId;
            item.RecordIdentification = row.GetColumnValue("recid", "-1");
            var itemNo = await row.GetColumnValue(_itemNumberFunctionName, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            if (string.IsNullOrEmpty(itemNo))
            {
                itemNo = await row.GetColumnValue(_rowItemNumberFunctionName, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            }
            item.ItemNumber = itemNo;
            item.QuantityIssued = await row.GetColumnValue("Items", fieldDefinitions, fieldGroupComponent, 0, cancellationToken);
            item.EndOfPeriod = await row.GetColumnValue("EndDate", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            item.Year = await row.GetColumnValue("Year", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            item.StartOfPeriod = await row.GetColumnValue("StartDate", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            item.Allocated = await row.GetColumnValue("Limited", fieldDefinitions, fieldGroupComponent, false, cancellationToken);

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

        private async Task LoadArticleConfiguration(CancellationToken cancellationToken)
        {
            var configName = configMenu.ViewReference.GetArgumentValue("ArticleConfigName");
            if (string.IsNullOrEmpty(configName))
            {
                return;
            }

            _articleSearchService.SearchAndListName = configName;
            UserAction action = _userActionBuilder.UserActionFromMenu(_configurationService, configMenu);
            _articleSearchService.SetSourceAction(action);
            await _articleSearchService.PrepareContentAsync(cancellationToken);
            _articleSearchService.SetAdditionalFilterParams(_additionalParams);
            var dtable = await _articleSearchService.GetRecords(0, cancellationToken);
            var (fieldGroupComponent, fieldDefinitions) = _articleSearchService.GetFieldConfig(0);

            if (dtable != null && dtable?.Rows.Count > 0)
            {
                quotaArticleData = new List<QuotaArticleData>();

                var tasks = dtable.Rows.Cast<DataRow>().Select(async row => await GetQuotaArticleDataRow(row, fieldGroupComponent, fieldDefinitions, cancellationToken));
                quotaArticleData.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
            }

        }

        private async Task<QuotaArticleData> GetQuotaArticleDataRow(DataRow row, FieldGroupComponent fieldGroupComponent, List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            QuotaArticleData item = new QuotaArticleData();
            item.InfoAreaId = fieldGroupComponent.TableInfo.InfoAreaId;
            item.RecordIdentification = row.GetColumnValue("recid", "-1");

            var itemNo = await row.GetColumnValue(_itemNumberFunctionName, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            if (string.IsNullOrEmpty(itemNo))
            {
                itemNo = await row.GetColumnValue(_rowItemNumberFunctionName, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            }
            item.ItemNumber = itemNo;

            item.Quota = await row.GetColumnValue("Quota", fieldDefinitions, fieldGroupComponent, 0, cancellationToken);

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

        public async Task<ItemQuota> GetQuotaItem(SerialEntryItem item, CancellationToken cancellationToken)
        {
             QuotaArticleData articleConfiguration = null;
             QuotaData quotaConfiguration = null;
            if (quotaArticleData?.Count > 0)
            {
                articleConfiguration = quotaArticleData.Where(x => x.ItemNumber.Equals(item.ItemNumber) && !x.ItemNumber.Equals(string.Empty)).FirstOrDefault();
            }

            if (quotaData?.Count > 0)
            {
                quotaConfiguration = quotaData.Where(x => x.ItemNumber.Equals(item.ItemNumber) && !x.ItemNumber.Equals(string.Empty)).FirstOrDefault();
            }

            return new ItemQuota(articleConfiguration, quotaConfiguration, _maxQuota,0);

        }

    }
}
