using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class QuestionnaireMetaDataService : ContentServiceBase, IQuestionnaireMetaDataService
    {

        private readonly string _searchAndListName = "F1Quest";

        private ActionTemplateBase _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private string _questionnaireModelRecordId = "";
        private string _questionnaireLabelFieldName = "";

        public string QuestionnaireLabel { get; private set; } = "";

        public QuestionnaireMetaDataService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {

        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            _actionTemplate = new ActionTemplateBase(_action.ViewReference);

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(_searchAndListName, cancellationToken).ConfigureAwait(false);

            _infoArea = _configurationService.GetInfoArea(searchAndList.InfoAreaId);
            _fieldControl = await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".List", cancellationToken);
            _tableInfo = await _configurationService.GetTableInfoAsync(searchAndList.InfoAreaId, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                GetFieldsInfo(fieldDefinitions, cancellationToken);

                if (fieldDefinitions.Count > 0)
                {
                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    ProcessQuestionnaireLabel();
                }
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private void GetFieldsInfo(List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {
                if (field.Function == "Label")
                {
                    _questionnaireLabelFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }

        private void ProcessQuestionnaireLabel()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count == 1)
            {
                DataRow row = _rawData.Result.Rows[0];

                if (row.Table.Columns.Contains("recid"))
                {
                    _questionnaireModelRecordId = row["recid"].ToString();
                }
                else
                {
                    return;
                }

                if (!string.IsNullOrEmpty(_questionnaireLabelFieldName) && row.Table.Columns.Contains(_questionnaireLabelFieldName))
                {
                    QuestionnaireLabel = row[_questionnaireLabelFieldName].ToString();
                }
            }
        }

        public string GetQuestionnaireModelRecordId()
        {
            return _questionnaireModelRecordId;
        }
    }
}
