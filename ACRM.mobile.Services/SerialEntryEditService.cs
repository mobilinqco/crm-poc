using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class SerialEntryEditService : NewOrEditService, ISerialEntryEditService
    {
        public SerialEntryEditService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IOfflineRequestsService offlineStoreService,
            IFilterProcessor filterProcessor,
            IFieldGroupDataService fieldGroupDataService,
            IJSProcessor jsProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, offlineStoreService, filterProcessor, fieldGroupDataService, jsProcessor)
        {

        }

        private string _fieldGroupName = string.Empty;
        public string FieldGroupName
        {
            get
            {
                return _fieldGroupName;
            }
            set
            {
                _fieldGroupName = value;
            }
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action == null)
            {
                await ProcessConfiguration(cancellationToken);
            }
            else
            {
                await ProcessTemplate(_action.ViewReference, cancellationToken);
            }

            if (_isNewMode == false)
            {
                await FetchRecord(_action.RecordId, cancellationToken);
            }

        }

        protected override async Task ProcessTemplate(ViewReference vr, CancellationToken cancellationToken)
        {
            SerialEntryTemplate template = new SerialEntryTemplate(vr);
            _template = template;
            string fieldControlName = string.Empty;
            
            var destExpandConfig = template.DestinationConfigName();
            if(destExpandConfig!=null)
            {
                fieldControlName = destExpandConfig;
            }
            if (!string.IsNullOrEmpty(FieldGroupName))
            {
                fieldControlName = FieldGroupName;
            }

            FieldControl fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Edit", cancellationToken);
            if (fieldControl == null)
            {
                fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Details", cancellationToken);
            }

            if (fieldControl != null)
            {
                string infoAreaId = fieldControl.InfoAreaId;

                _infoArea = _configurationService.GetInfoArea(infoAreaId);
                _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
            }

            await ResolveTemplateFilter(template.DestinationTemplateFilter(), cancellationToken);
        }

        protected async Task ProcessConfiguration(CancellationToken cancellationToken)
        {
            string fieldControlName = string.Empty;
            if (!string.IsNullOrEmpty(FieldGroupName))
            {
                fieldControlName = FieldGroupName;
            }
            else
            {
                return;
            }

            FieldControl fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Edit", cancellationToken);
            if (fieldControl == null)
            {
                fieldControl = await _configurationService.GetFieldControl(fieldControlName + ".Details", cancellationToken);
            }

            if (fieldControl != null)
            {
                string infoAreaId = fieldControl.InfoAreaId;

                _infoArea = _configurationService.GetInfoArea(infoAreaId);
                _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
                _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
            }
        }

        public async Task<PanelData> GetPanelAsync(DataRow dataRow, CancellationToken cancellationToken)
        {
            string recordId = string.Empty;
            PanelData result = null;
            Dictionary<string, string> templateFilterValues = new Dictionary<string, string>();

            if (dataRow == null)
            {
                templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_templateFilter, cancellationToken);
            }
            else
            {
                recordId = dataRow.GetColumnValue("recId", "-1");
            }

            if (_fieldGroupComponent.HasTabs())
            {
                FieldControlTab panel = _fieldGroupComponent.FieldControl.Tabs[0];
                var panelType = panel.GetEditPanelType();
                if (panelType != PanelType.NotSupported && !panel.IsHeaderPanel())
                {
                    PanelData pd = new PanelData
                    {
                        Label = panel.Label,
                        Type = PanelType.SerialEntryEditPanel,
                        RecordInfoArea = panel.FieldControl.InfoAreaId,
                        RecordId = recordId.CleanRecordId()
                    };

                    List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
                    ListDisplayRow outRow = await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, dataRow,
                        templateFilterValues, panel.GetEditPanelType(), cancellationToken);
                    if (outRow.Fields.Count > 0)
                    {
                        pd.Fields = outRow.Fields;
                        result = pd;
                    }
                }

            }

            return result;
        }

        public async Task<bool> Delete(string destRecordId, CancellationToken token)
        {
            try
            {
 

                OfflineRequest offlineRequest = await _offlineStoreService.CreateDeleteRequest(_template, _fieldGroupComponent.FieldControl.InfoAreaId, destRecordId);

                ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(token, _tableInfo, offlineRequest);

                if (!modifyRecordResult.HasSaveErrors())
                {
                    _logService.LogDebug("Record has been deleted.");
                    await _offlineStoreService.Delete(offlineRequest, token);

                }
                else
                {
                    _logService.LogError($"Error deleting the record {modifyRecordResult.ErrorMessage()}");
                    await _offlineStoreService.Update(offlineRequest, token);
                    throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
                }



               
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }
    }
}
