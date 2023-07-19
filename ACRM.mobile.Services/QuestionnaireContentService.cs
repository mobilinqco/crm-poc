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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class QuestionnaireContentService : ContentServiceBase, IQuestionnaireContentService
    {
        private readonly IOfflineRequestsService _offlineStoreService;
        private readonly IQuestionnaireQuestionsService _questionnaireQuestionsService;
        private readonly IQuestionnaireMetaDataService _questionnaireMetaDataService;
        private readonly IQuestionnaireAnswersService _questionnaireAnswersService;
        private readonly IQuestionnaireAnswerDataService _questionnaireAnswerDataService;
        private readonly IRightsProcessor _rightsProcessor;

        private ActionTemplateBase _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private bool _isEdit = false;
        private bool _isFinalized = false;
        private bool _isQuestionnaireReadOnly = false;

        private Filter _finalizeFilter;

        private string _questionnaireIdFieldName = "";
        private string _questionnaireDateFieldName = "";

        private string _questionnaireRecordId = "";
        private string _questionnaireId = "";
        private string _questionnaireDateString = "";
        public string QuestionnaireLabel { get; private set; } = "";

        private List<QuestionnaireQuestionSection> _questionnaireQuestionSections = new List<QuestionnaireQuestionSection>();

        public QuestionnaireContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IOfflineRequestsService offlineRequestsService,
            IQuestionnaireQuestionsService questionnaireQuestionsService,
            IQuestionnaireMetaDataService questionnaireMetaDataService,
            IQuestionnaireAnswersService questionnaireAnswersService,
            IQuestionnaireAnswerDataService questionnaireAnswerDataService,
            IRightsProcessor rightsProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineRequestsService;
            _questionnaireMetaDataService = questionnaireMetaDataService;
            _questionnaireQuestionsService = questionnaireQuestionsService;
            _questionnaireAnswersService = questionnaireAnswersService;
            _questionnaireAnswerDataService = questionnaireAnswerDataService;
            _rightsProcessor = rightsProcessor;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            ViewReference vr = _action.ViewReference;
            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken);
            }

            if (vr.IsQuestionnaireEditAction())
            {
                _isEdit = true;
                await ProcessQuestionnaireEditActionTemplate(vr, cancellationToken);
            }
            else
            {
                await ProcessQuestionnaireActionTemplate(vr, cancellationToken);
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private async Task ProcessQuestionnaireEditActionTemplate(ViewReference viewReference, CancellationToken cancellationToken)
        {
            QuestionnaireEditTemplate questionnaireEditTemplate = new QuestionnaireEditTemplate(viewReference);
            _actionTemplate = questionnaireEditTemplate;

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(questionnaireEditTemplate.SurveySearchAndListName(), cancellationToken).ConfigureAwait(false);

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

                    await IsQuestionnaireFinalized(questionnaireEditTemplate, fieldDefinitions, parentLink, cancellationToken);

                    if(!_isFinalized)
                    {
                        await InitializeFinalizeFilter(questionnaireEditTemplate, cancellationToken);
                    }

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    ProcessQuestionnaireData();

                    await PrepareSearchAndContentServices(cancellationToken);

                    GetQuestionnaireMetaData();
                    BuildQuestionnaireQuestionSections();
                }
            }
        }

        private async Task ProcessQuestionnaireActionTemplate(ViewReference viewReference, CancellationToken cancellationToken)
        {
            QuestionnaireTemplate questionnaireTemplate = new QuestionnaireTemplate(viewReference);
            _actionTemplate = questionnaireTemplate;

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(questionnaireTemplate.SurveySearchAndListName(), cancellationToken).ConfigureAwait(false);

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

                    _isQuestionnaireReadOnly = questionnaireTemplate.QuestionnaireReadOnly();

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    ProcessQuestionnaireData();

                    await PrepareSearchAndContentServices(cancellationToken);

                    GetQuestionnaireMetaData();
                    BuildQuestionnaireQuestionSections();
                }
            }
        }

        private async Task IsQuestionnaireFinalized(QuestionnaireEditTemplate questionnaireEditTemplate, List<FieldControlField> fieldDefinitions,
            ParentLink rootRecord, CancellationToken cancellationToken)
        {
            var (result, message) = await _rightsProcessor.EvaluateRightsFilter(questionnaireEditTemplate.ConfirmedFilterName(), rootRecord?.RecordId, cancellationToken);
            _isFinalized = result;
        }

        private async Task InitializeFinalizeFilter(QuestionnaireEditTemplate questionnaireEditTemplate, CancellationToken cancellationToken)
        {
            _finalizeFilter = await _configurationService.GetFilter(questionnaireEditTemplate.ConfirmedFilterName(), cancellationToken).ConfigureAwait(false);
        }

        private void GetFieldsInfo(List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {
                FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, field.FieldId);

                if (field.Function == "QuestionnaireID")
                {
                    _questionnaireIdFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Date")
                {
                    _questionnaireDateFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }

        private void ProcessQuestionnaireData()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count == 1)
            {
                DataRow row = _rawData.Result.Rows[0];

                if (row.Table.Columns.Contains("recid"))
                {
                    _questionnaireRecordId = row["recid"].ToString();
                }
                else
                {
                    return;
                }

                if (!string.IsNullOrEmpty(_questionnaireIdFieldName) && row.Table.Columns.Contains(_questionnaireIdFieldName))
                {
                    _questionnaireId = row[_questionnaireIdFieldName].ToString();
                }

                if (!string.IsNullOrEmpty(_questionnaireDateFieldName) && row.Table.Columns.Contains(_questionnaireDateFieldName))
                {
                    _questionnaireDateString = row[_questionnaireDateFieldName].ToString();
                } 
            }
        }

        private async Task PrepareSearchAndContentServices(CancellationToken cancellationToken)
        {
            _questionnaireMetaDataService.SetSourceAction(_action);
            await _questionnaireMetaDataService.PrepareContentAsync(cancellationToken);

            _questionnaireQuestionsService.SetSourceAction(PrepareUserAction("U1", "F1", _questionnaireMetaDataService.GetQuestionnaireModelRecordId()));
            await _questionnaireQuestionsService.PrepareContentAsync(cancellationToken);

            _questionnaireAnswersService.SetSourceAction(PrepareUserAction("U1", "F2", _questionnaireMetaDataService.GetQuestionnaireModelRecordId()));
            await _questionnaireAnswersService.PrepareContentAsync(cancellationToken);

            _questionnaireAnswerDataService.SetSourceAction(PrepareUserAction("U1", "U2", _questionnaireRecordId));
            await _questionnaireAnswerDataService.PrepareContentAsync(cancellationToken);
        }

        private UserAction PrepareUserAction(string infoAreaUnitName, string sourceInfoArea, string recordId)
        {
            return new UserAction
            {
                ViewReference = _action.ViewReference,
                InfoAreaUnitName = infoAreaUnitName,
                SourceInfoArea = sourceInfoArea,
                RecordId = recordId
            };
        }

        private void GetQuestionnaireMetaData()
        {
            QuestionnaireLabel = _questionnaireMetaDataService.QuestionnaireLabel;
        }

        private void BuildQuestionnaireQuestionSections()
        {
            Dictionary<int, List<QuestionnaireAnswer>> questionnaireAnswers = _questionnaireAnswersService.GetQuestionnaireQuestionsAnswers(_questionnaireId);
            List<QuestionnaireQuestion> questionnaireQuestions = _questionnaireQuestionsService.GetQuestions(_questionnaireId);

            QuestionnaireQuestionSection currentQuestionnaireQuestionSection = null;

            foreach(QuestionnaireQuestion questionnaireQuestion in questionnaireQuestions.OrderBy(questionnaireQuestion => questionnaireQuestion.QuestionNumber).ToList())
            {
                if(currentQuestionnaireQuestionSection == null && !questionnaireQuestion.NewSection)
                {
                    break;
                }
                else
                {
                    if(questionnaireQuestion.NewSection)
                    {
                        currentQuestionnaireQuestionSection = new QuestionnaireQuestionSection(questionnaireQuestion);
                        _questionnaireQuestionSections.Add(currentQuestionnaireQuestionSection);
                    }
                    else
                    {
                        List<QuestionnaireAnswerData> questionnaireAnswerDataList =
                            _questionnaireAnswerDataService.GetQuestionnaireQuestionsAnswersData(questionnaireQuestion.QuestionNumber);

                        List<QuestionnaireAnswer> currentQuestionnaireQuestionAnswers = new List<QuestionnaireAnswer>();
                        if (questionnaireAnswers.ContainsKey(questionnaireQuestion.QuestionNumber))
                        {
                            currentQuestionnaireQuestionAnswers.AddRange(questionnaireAnswers[questionnaireQuestion.QuestionNumber]
                                .OrderBy(questionnaireQuestion => questionnaireQuestion.QuestionNumber));
                        }

                        QuestionnaireQuestionData questionnaireQuestionData = new QuestionnaireQuestionData(questionnaireQuestion, 
                            currentQuestionnaireQuestionAnswers, questionnaireAnswerDataList);

                        currentQuestionnaireQuestionSection.AddquestionnaireQuestionData(questionnaireQuestionData);
                    }
                }
            }
        }

        public List<QuestionnaireQuestionSection> GetQuestionnaireQuestionSections()
        {
            return _questionnaireQuestionSections;
        }

        public bool IsQuestionnaireFinalized()
        {
            return _isFinalized;
        }

        public bool IsQuestionnaireReadOnly()
        {
            return _isQuestionnaireReadOnly;
        }

        public async Task<ModifyRecordResult> SaveAnswerData(string _questionNumber, string _answerNumber, string answer,
                    string oldAnswer, string recordId, string questionRecordId, string answerRecordId, CancellationToken cancellationToken)
        {
            return await _questionnaireAnswerDataService.SaveAnswerData(_questionNumber, _answerNumber, answer, oldAnswer, 
                recordId, questionRecordId, answerRecordId, cancellationToken);
        }

        public async Task<ModifyRecordResult> DeleteAnswerData(string recordId, CancellationToken cancellationToken)
        {
            return await _questionnaireAnswerDataService.DeleteAnswerData(recordId, cancellationToken);
        }

        public async Task<ModifyRecordResult> SaveQuestionnaireState(CancellationToken cancellationToken)
        {
            TableInfo tableInfo = await _configurationService.GetTableInfoAsync("U1", cancellationToken).ConfigureAwait(false);
            Dictionary<string, string> templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_finalizeFilter, cancellationToken);

            OfflineRequest offlineRequest = await _offlineStoreService.CreateUpdateRequest(_actionTemplate, tableInfo, 
                _questionnaireRecordId, templateFilterValues, cancellationToken);

            ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

            if (!modifyRecordResult.HasSaveErrors())
            {
                _logService.LogDebug("Records has been updated.");
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
