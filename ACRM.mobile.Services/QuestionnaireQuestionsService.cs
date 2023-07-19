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
    public class QuestionnaireQuestionsService : ContentServiceBase, IQuestionnaireQuestionsService
    {
        private readonly string _searchAndListName = "F2Quest";

        private ActionTemplateBase _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private Dictionary<string, List<QuestionnaireQuestion>> _questionnaireIdQuestionDict = new Dictionary<string, List<QuestionnaireQuestion>>();

        private string _questionnaireIdFieldName = "";
        private string _questionNumberFieldName = "";
        private string _labelFieldName = "";
        private string _multipleFieldName = "";
        private string _infoAreaIdFieldName = "";
        private string _fieldIdFieldName = "";
        private string _followUpNumberFieldName = "";
        private string _newSectionFieldName = "";
        private string _mandatoryFieldName = "";
        private string _defaultFieldName = "";
        private string _readFieldName = "";
        private string _saveFieldName = "";
        private string _hideFieldName = "";

        public QuestionnaireQuestionsService(ISessionContext sessionContext,
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

                    ProcessQuestionnaireQuestions();
                }
            }

            _logService.LogDebug("End PrepareContentAsync");
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
                if (field.Function == "QuestionNumber")
                {
                    _questionNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Label")
                {
                    _labelFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Multiple")
                {
                    _multipleFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "InfoAreaId")
                {
                    _infoAreaIdFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "FieldId")
                {
                    _fieldIdFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "FollowUpNumber")
                {
                    _followUpNumberFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "NewSection")
                {
                    _newSectionFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Mandatory")
                {
                    _mandatoryFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Default")
                {
                    _defaultFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Read")
                {
                    _readFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Save")
                {
                    _saveFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Hide")
                {
                    _hideFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }

        private void ProcessQuestionnaireQuestions()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
            {
                foreach(DataRow row in _rawData.Result.Rows)
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
                    string label = "";
                    bool multiple = false;
                    string infoAreaId = "";
                    string fieldId = "";
                    int followUpNumber = 0;
                    bool newSection = false;
                    bool mandatory = false;
                    string def = "";
                    bool read = false;
                    bool save = false;
                    bool hide = false;

                    if (!string.IsNullOrEmpty(_questionnaireIdFieldName) && row.Table.Columns.Contains(_questionnaireIdFieldName))
                    {
                        questionnaireId = row[_questionnaireIdFieldName].ToString();
                    }
                    if (!string.IsNullOrEmpty(_questionNumberFieldName) && row.Table.Columns.Contains(_questionNumberFieldName))
                    {
                        if(int.TryParse(row[_questionNumberFieldName].ToString(), out int parsedInt))
                        {
                            questionNumber = parsedInt;
                        }
                    }
                    if (!string.IsNullOrEmpty(_labelFieldName) && row.Table.Columns.Contains(_labelFieldName))
                    {
                        label = row[_labelFieldName].ToString();
                    }
                    if (!string.IsNullOrEmpty(_multipleFieldName) && row.Table.Columns.Contains(_multipleFieldName))
                    {
                        if (bool.TryParse(row[_multipleFieldName].ToString(), out bool parsedBool))
                        {
                            multiple = parsedBool;
                        }
                    }
                    if (!string.IsNullOrEmpty(_infoAreaIdFieldName) && row.Table.Columns.Contains(_infoAreaIdFieldName))
                    {
                        infoAreaId = row[_infoAreaIdFieldName].ToString();
                    }
                    if (!string.IsNullOrEmpty(_fieldIdFieldName) && row.Table.Columns.Contains(_fieldIdFieldName))
                    {
                        fieldId = row[_fieldIdFieldName].ToString();
                    }
                    if (!string.IsNullOrEmpty(_followUpNumberFieldName) && row.Table.Columns.Contains(_followUpNumberFieldName))
                    {
                        if (int.TryParse(row[_followUpNumberFieldName].ToString(), out int parsedInt))
                        {
                            followUpNumber = parsedInt;
                        }
                    }
                    if (!string.IsNullOrEmpty(_newSectionFieldName) && row.Table.Columns.Contains(_newSectionFieldName))
                    {
                        if (bool.TryParse(row[_newSectionFieldName].ToString(), out bool parsedBool))
                        {
                            newSection = parsedBool;
                        }
                    }
                    if (!string.IsNullOrEmpty(_mandatoryFieldName) && row.Table.Columns.Contains(_mandatoryFieldName))
                    {
                        if (bool.TryParse(row[_mandatoryFieldName].ToString(), out bool parsedBool))
                        {
                            mandatory = parsedBool;
                        }
                    }
                    if (!string.IsNullOrEmpty(_defaultFieldName) && row.Table.Columns.Contains(_defaultFieldName))
                    {
                        def = row[_defaultFieldName].ToString();
                    }
                    if (!string.IsNullOrEmpty(_readFieldName) && row.Table.Columns.Contains(_readFieldName))
                    {
                        if (bool.TryParse(row[_readFieldName].ToString(), out bool parsedBool))
                        {
                            read = parsedBool;
                        }
                    }
                    if (!string.IsNullOrEmpty(_saveFieldName) && row.Table.Columns.Contains(_saveFieldName))
                    {
                        if (bool.TryParse(row[_saveFieldName].ToString(), out bool parsedBool))
                        {
                            save = parsedBool;
                        }
                    }
                    if (!string.IsNullOrEmpty(_hideFieldName) && row.Table.Columns.Contains(_hideFieldName))
                    {
                        if (bool.TryParse(row[_hideFieldName].ToString(), out bool parsedBool))
                        {
                            hide = parsedBool;
                        }
                    }

                    if(!_questionnaireIdQuestionDict.ContainsKey(questionnaireId))
                    {
                        _questionnaireIdQuestionDict[questionnaireId] = new List<QuestionnaireQuestion>();
                    }

                    _questionnaireIdQuestionDict[questionnaireId].Add(new QuestionnaireQuestion(recordId, questionnaireId, questionNumber, label, multiple, infoAreaId,
                        fieldId, followUpNumber, newSection, mandatory, def, read, save, hide));
                }
            }
        }

        public List<QuestionnaireQuestion> GetQuestions(string questionnaireId)
        {
            if(_questionnaireIdQuestionDict.ContainsKey(questionnaireId))
            {
                return _questionnaireIdQuestionDict[questionnaireId];
            }
            return new List<QuestionnaireQuestion>();
        }
    }
}
