using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class ModifyRecordService: ContentServiceBase, IModifyRecordService
    {
        protected IOfflineRequestsService _offlineStoreService;
        protected IFieldGroupDataService _fieldGroupDataService;
        protected ModifyRecordTemplate _template;
        protected string _infoAreaId;
        protected string _recordId;

        public bool _isBusy = false;

        public ModifyRecordService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IOfflineRequestsService offlineStoreService,
            IFieldGroupDataService fieldGroupDataService,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineStoreService;
            _fieldGroupDataService = fieldGroupDataService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
        }

        public async Task ModifyRecord(UserAction userAction, CancellationToken cancellationToken)
        {
            try
            {
                _isBusy = true;
                _action = userAction;

                _template = new ModifyRecordTemplate(userAction.ViewReference);

                await ResolveCopySourceFieldGroupName(_template.CopySourceFieldGroupName(), cancellationToken);
                Filter templateFilter = await ResolveTemplateFilter(_template.TemplateFilter(), cancellationToken);
                _infoAreaId = templateFilter.InfoAreaId;
                _recordId = userAction.RecordId;
                
                if(string.IsNullOrWhiteSpace(_recordId))
                {
                    throw new CrmException("Modify Record Action has no RecordId!", CrmExceptionType.UserAction, CrmExceptionSubType.CrmDataRequestError);
                }

                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(_infoAreaId, cancellationToken);
                Dictionary<string, string> templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(templateFilter, cancellationToken);

                OfflineRequest offlineRequest = await _offlineStoreService.CreateModifyRequest(_template, tableInfo, userAction.RecordId, templateFilterValues, cancellationToken);

                ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, tableInfo, offlineRequest);

                if (!modifyRecordResult.HasSaveErrors())
                {
                    _logService.LogDebug("Record has been modified.");
                    await _offlineStoreService.Delete(offlineRequest, cancellationToken);
                }
                else
                {
                    _logService.LogError($"Error modifying record {modifyRecordResult.ErrorMessage()}");
                    await _offlineStoreService.Update(offlineRequest, cancellationToken);
                    throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
                }

                _isBusy = false;
            }
            catch(Exception e)
            {
                _isBusy = false;
                throw e;
            }
        }

        private async Task<Filter> ResolveTemplateFilter(string templateFilterName, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(templateFilterName))
            {
                var filters = await _filterProcessor.RetrieveFilterDetails(new List<string> { templateFilterName }, cancellationToken, true);
                if (filters.Count > 0)
                {
                    return filters[0];
                }
            }

            return null;
        }

        public bool IsBusy()
        {
            return _isBusy;
        }

        public async Task<UserAction> SavedAction(CancellationToken cancellationToken)
        {
            string savedActionName = _template.SavedAction();
            if (!string.IsNullOrEmpty(savedActionName))
            {
                UserAction ua = await _userActionBuilder.ResolveSavedAction(_configurationService, savedActionName, _recordId, _infoAreaId, cancellationToken);
                if(!string.IsNullOrWhiteSpace(_template.RecordIdForSavedAction()))
                {
                    ua.RecordIdForSavedAction = _recordId;
                }

                if (!string.IsNullOrWhiteSpace(_template.LinkRecordId()))
                {
                    ua.LinkRecordId = _recordId;
                }

                return ua;
            }

            return null;
        }

        protected async Task ResolveCopySourceFieldGroupName(string fieldGroupName, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_action.RecordId))
            {
                _filterProcessor.SetAdditionalFilterParams(await _fieldGroupDataService.GetSourceFieldGroupData(_action.RecordId,
                    fieldGroupName, DetermineRequestMode(_template), cancellationToken));
            }
        }
    }
}
