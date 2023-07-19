using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Questionnaire;
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
    public class QuestionnaireAnswersService : ContentServiceBase, IQuestionnaireAnswersService
    {
        private readonly string _searchAndListName = "F3Quest";

        private ActionTemplateBase _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private Dictionary<string, Dictionary<int, List<QuestionnaireAnswer>>> _questionnaireIdQuestionNumberAnswerDict =
            new Dictionary<string, Dictionary<int, List<QuestionnaireAnswer>>>();

        private string _questionnaireIdFieldName = "";
        private string _questionNumberFieldName = "";
        private string _answerNumberFieldName = "";
        private string _labelFieldName = "";
        private string _followUpNumberFieldName = "";

        public QuestionnaireAnswersService(ISessionContext sessionContext,
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

                GetFieldsInfo(fieldDefinitions);

                if (fieldDefinitions.Count > 0)
                {
                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    ProcessQuestionnaireQuestions();
                }
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private void GetFieldsInfo(List<FieldControlField> fieldDefinitions)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {
                FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, field.FieldId);

                if (field.Function == "QuestionnaireID")
                {
                    _questionnaireIdFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "QuestionNumber")
                {
                    _questionNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "AnswerNumber")
                {
                    _answerNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Label")
                {
                    _labelFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "FollowUpNumber")
                {
                    _followUpNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }

        private void ProcessQuestionnaireQuestions()
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

                    string questionnaireId = "";
                    int questionNumber = 0;
                    int answerNumber = 0;
                    string label = "";
                    int followUpNumber = 0;

                    if (!string.IsNullOrEmpty(_questionnaireIdFieldName) && row.Table.Columns.Contains(_questionnaireIdFieldName))
                    {
                        questionnaireId = row[_questionnaireIdFieldName].ToString();
                    }
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
                    if (!string.IsNullOrEmpty(_labelFieldName) && row.Table.Columns.Contains(_labelFieldName))
                    {
                        label = row[_labelFieldName].ToString();
                    }
                    if (!string.IsNullOrEmpty(_followUpNumberFieldName) && row.Table.Columns.Contains(_followUpNumberFieldName))
                    {
                        if (int.TryParse(row[_followUpNumberFieldName].ToString(), out int parsedInt))
                        {
                            followUpNumber = parsedInt;
                        }
                    }

                    if(!_questionnaireIdQuestionNumberAnswerDict.ContainsKey(questionnaireId))
                    {
                        _questionnaireIdQuestionNumberAnswerDict[questionnaireId] = new Dictionary<int, List<QuestionnaireAnswer>>();
                    }

                    if(!_questionnaireIdQuestionNumberAnswerDict[questionnaireId].ContainsKey(questionNumber))
                    {
                        _questionnaireIdQuestionNumberAnswerDict[questionnaireId][questionNumber] = new List<QuestionnaireAnswer>();
                    }

                    _questionnaireIdQuestionNumberAnswerDict[questionnaireId][questionNumber].Add(
                        new QuestionnaireAnswer(recordId, questionnaireId, questionNumber, answerNumber, label, followUpNumber));
                }
            }
        }

        public Dictionary<int, List<QuestionnaireAnswer>> GetQuestionnaireQuestionsAnswers(string questionnaireId)
        {
            if(_questionnaireIdQuestionNumberAnswerDict.ContainsKey(questionnaireId))
            {
                return _questionnaireIdQuestionNumberAnswerDict[questionnaireId];
            }
            return new Dictionary<int, List<QuestionnaireAnswer>>();
        }
    }
}
