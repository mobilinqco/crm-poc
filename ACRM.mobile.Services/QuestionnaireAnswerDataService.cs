using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
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
    public class QuestionnaireAnswerDataService : ContentServiceBase, IQuestionnaireAnswerDataService
    {
        private readonly IOfflineRequestsService _offlineStoreService;

        private ActionTemplateBase _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private FieldInfo _questionNumberFieldInfo;
        private FieldInfo _answerNumberFieldInfo;
        private FieldInfo _answerFieldInfo;

        private string _questionNumberFieldName = "";
        private string _answerNumberFieldName = "";
        private string _answerFieldName = "";

        private Dictionary<int, List<QuestionnaireAnswerData>> _questionnaireAnswersDataDict = new Dictionary<int, List<QuestionnaireAnswerData>>();

        public QuestionnaireAnswerDataService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IOfflineRequestsService offlineRequestsService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineRequestsService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            ViewReference vr = _action.ViewReference;
            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken);
            }

            QuestionnaireEditTemplate questionnaireEditTemplate = new QuestionnaireEditTemplate(vr);
            _actionTemplate = questionnaireEditTemplate;

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(questionnaireEditTemplate.SurveyAnswerSearchAndListName(), cancellationToken).ConfigureAwait(false);

            _infoArea = _configurationService.GetInfoArea(searchAndList.InfoAreaId);
            _fieldControl = await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".List", cancellationToken);
            _tableInfo = await _configurationService.GetTableInfoAsync(searchAndList.InfoAreaId, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                GetFieldsInfo(fieldDefinitions);

                if (fieldDefinitions.Count > 0)
                {
                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    ProcessQuestionnaireAnswersData();
                }
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private void GetFieldsInfo(List<FieldControlField> fieldDefinitions)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {
                FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, field.FieldId);

                if (field.Function == "QuestionNumber")
                {
                    _questionNumberFieldInfo = fieldInfo;
                    _questionNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "AnswerNumber")
                {
                    _answerNumberFieldInfo = fieldInfo;
                    _answerNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Answer")
                {
                    _answerFieldInfo = fieldInfo;
                    _answerFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }

        private void ProcessQuestionnaireAnswersData()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
            {
                foreach (DataRow row in _rawData.Result.Rows)
                {
                    string recordId;
                    if (row.Table.Columns.Contains("recid"))
                    {
                        recordId = row["recid"].ToString();
                    }
                    else
                    {
                        continue;
                    }

                    int questionNumber = 0;
                    int answerNumber = 0;
                    string answer = "";

                    if (!string.IsNullOrEmpty(_questionNumberFieldName) && row.Table.Columns.Contains(_questionNumberFieldName))
                    {
                        if (int.TryParse(row[_questionNumberFieldName].ToString(), out int parsedInt))
                        {
                            questionNumber = parsedInt;
                        }
                    }
                    if (!string.IsNullOrEmpty(_answerNumberFieldName) && row.Table.Columns.Contains(_answerNumberFieldName))
                    {
                        if (int.TryParse(row[_answerNumberFieldName].ToString(), out int parsedInt))
                        {
                            answerNumber = parsedInt;
                        }
                    }
                    if (!string.IsNullOrEmpty(_answerFieldName) && row.Table.Columns.Contains(_answerFieldName))
                    {
                        answer = row[_answerFieldName].ToString();
                    }

                    if(!_questionnaireAnswersDataDict.ContainsKey(questionNumber))
                    {
                        _questionnaireAnswersDataDict.Add(questionNumber, new List<QuestionnaireAnswerData>());
                    }

                    List<QuestionnaireAnswerData> currentQuestionAnswerDataList = _questionnaireAnswersDataDict[questionNumber];
                    currentQuestionAnswerDataList.Add(new QuestionnaireAnswerData(recordId, questionNumber, answerNumber, answer));
                }
            }
        }

        public List<QuestionnaireAnswerData> GetQuestionnaireQuestionsAnswersData(int questionNumber)
        {
            if (_questionnaireAnswersDataDict.ContainsKey(questionNumber))
            {
                return _questionnaireAnswersDataDict[questionNumber];
            }
            return null;
        }

        public async Task<ModifyRecordResult> SaveAnswerData(string _questionNumber, string _answerNumber, string answer, 
            string oldAnswer, string recordId, string questionRecordId, string answerRecordId, CancellationToken cancellationToken)
        {
            Dictionary<int, string> currentFieldValues = new Dictionary<int, string>()
            {
                [_questionNumberFieldInfo.FieldId] = _questionNumber,
                [_answerNumberFieldInfo.FieldId] = _answerNumber,
                [_answerFieldInfo.FieldId] = answer
            };

            Dictionary<int, string> oldFieldValues = new Dictionary<int, string>()
            {
                [_questionNumberFieldInfo.FieldId] = _questionNumber,
                [_answerNumberFieldInfo.FieldId] = _answerNumber,
                [_answerFieldInfo.FieldId] = oldAnswer
            };

            return await Save(currentFieldValues, oldFieldValues, recordId, BuildOfflineRecordLinks(questionRecordId, answerRecordId), cancellationToken);
        }

        private List<OfflineRecordLink> BuildOfflineRecordLinks(string questionRecordId, string answerRecordId)
        {
            List<OfflineRecordLink> offlineRecordLinks = new List<OfflineRecordLink>();

            if(questionRecordId != null)
            {
                offlineRecordLinks.Add(new OfflineRecordLink()
                {
                    InfoAreaId = "F2",
                    LinkId = 0,
                    RecordId = questionRecordId
                });
            }

            if(answerRecordId != null)
            {
                offlineRecordLinks.Add(new OfflineRecordLink()
                {
                    InfoAreaId = "F3",
                    LinkId = 0,
                    RecordId = answerRecordId
                });
            }

            return offlineRecordLinks;
        }

        private async Task<ModifyRecordResult> Save(Dictionary<int, string> currentFieldValues, Dictionary<int, string> oldFieldValues, 
            string recordId, List<OfflineRecordLink> offlineRecordLinks, CancellationToken cancellationToken)
        {
            PanelData panelData = new PanelData() { RecordId = recordId };
            FieldControlTab panel = _fieldGroupComponent.FieldControl.Tabs[0];
            List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, null, null, panel.GetEditPanelType(), cancellationToken);

            if (outRow.Fields.Count > 0)
            {
                foreach (ListDisplayField listDisplayField in outRow.Fields)
                {
                    listDisplayField.EditData.ChangeOfflineRequest = new OfflineRecordField()
                    {
                        FieldId = listDisplayField.Config.FieldConfig.FieldId,
                        NewValue = currentFieldValues[listDisplayField.Config.FieldConfig.FieldId],
                        OldValue = oldFieldValues[listDisplayField.Config.FieldConfig.FieldId],
                        Offline = 0
                    };
                    listDisplayField.Data.StringData = oldFieldValues[listDisplayField.Config.FieldConfig.FieldId];
                    listDisplayField.EditData.DefaultStringValue = oldFieldValues[listDisplayField.Config.FieldConfig.FieldId];
                    listDisplayField.EditData.StringValue = currentFieldValues[listDisplayField.Config.FieldConfig.FieldId];
                }
                panelData.Fields = outRow.Fields;
            }

            List<PanelData> inputPanels = new List<PanelData>() { panelData };

            List<OfflineRecordLink> recordLinks = new List<OfflineRecordLink>();

            recordLinks.AddRange(offlineRecordLinks);

            string parentLinkId = _action.GetLinkId();

            if (!int.TryParse(parentLinkId, out int intParentLinkId))
            {
                intParentLinkId = 0;
            }

            var parentLinkInfo = _tableInfo.GetLinkInfo(_action.SourceInfoArea, intParentLinkId);

            if (parentLinkInfo != null)
            {
                var parentLink = new OfflineRecordLink()
                {
                    InfoAreaId = _action.SourceInfoArea,
                    LinkId = parentLinkInfo.LinkId,
                    RecordId = _action.RecordId
                };

                recordLinks.Add(parentLink);
            }
            else
            {
                recordLinks.Add(_action.GetLinkRequest());
            }

            OfflineRequest offlineRequest;

            if (recordId == null)
            {
                offlineRequest = await _offlineStoreService.CreateSaveRequest(_actionTemplate, _fieldGroupComponent.FieldControl, _tableInfo, inputPanels, null, recordLinks, cancellationToken);
            }
            else
            {
                offlineRequest = await _offlineStoreService.CreateUpdateRequest(_actionTemplate, _fieldGroupComponent.FieldControl, inputPanels, new List<string> { recordId }, cancellationToken);
            }


            ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

            if (!modifyRecordResult.HasSaveErrors())
            {
                if (recordId == null)
                {
                    _logService.LogDebug("New record has been saved.");
                }
                else
                {
                    _logService.LogDebug("Records has been updated.");
                }

                await _offlineStoreService.Delete(offlineRequest, cancellationToken);
            }
            else
            {
                await _offlineStoreService.Update(offlineRequest, cancellationToken);
                throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
            }

            return modifyRecordResult;
        }

        public async Task<ModifyRecordResult> DeleteAnswerData(string recordId, CancellationToken cancellationToken)
        {
            OfflineRequest offlineRequest = await _offlineStoreService.CreateDeleteRequest(_actionTemplate, _fieldControl.InfoAreaId, recordId);

            ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

            if (!modifyRecordResult.HasSaveErrors())
            {
                _logService.LogDebug("Record has been deleted.");
                await _offlineStoreService.Delete(offlineRequest, cancellationToken);
            }
            else
            {
                await _offlineStoreService.Update(offlineRequest, cancellationToken);
                throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
            }

            return modifyRecordResult;
        }
    }
}
