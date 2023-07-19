using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;
using Org.BouncyCastle.Ocsp;

namespace ACRM.mobile.Services
{
    public class OpenUrlService: IOpenUrlService
    {
        private readonly ITokenProcessor _tokenProcessor;
        private readonly IConfigurationService _configurationService;
        private readonly ICrmDataService _crmDataService;
        private readonly IFilterProcessor _filterProcessor;
        private readonly FieldGroupComponent _fieldGroupComponent;

        private string _url;
        private UserAction _userAction;
        private OpenURLTemplate _openURLTemplate;

        public OpenUrlService(IConfigurationService configurationService,
            ICrmDataService crmDataService,
            FieldGroupComponent fieldGroupComponent,
            IFilterProcessor filterProcessor,
            ITokenProcessor tokenProcessor)
        {
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _fieldGroupComponent = fieldGroupComponent;
            _filterProcessor = filterProcessor;
            _tokenProcessor = tokenProcessor;
            _url = string.Empty;
        }

        public bool PopToPrevious()
        {
            if(_openURLTemplate != null)
            {
                return _openURLTemplate.PopToPrevious();
            }

            return false;
        }

        public async Task PrepareContentAsync(UserAction userAction, string recordId, CancellationToken cancellationToken)
        {
            _userAction = userAction;
            _openURLTemplate = null;

            if(_userAction.ViewReference != null)
            {
                _openURLTemplate = new OpenURLTemplate(_userAction.ViewReference);
                if(!string.IsNullOrWhiteSpace(_openURLTemplate.Url()))
                {
                    string curRecordId = recordId.FormatedRecordId(userAction.InfoAreaUnitName);
                    if(!string.IsNullOrWhiteSpace(userAction.RawRecordId) && !_openURLTemplate.RecomputeRecordId())
                    {
                        curRecordId = _userAction.RawRecordId;
                    }

                    if (!string.IsNullOrWhiteSpace(_openURLTemplate.DotReplaceChar()))
                    {
                        curRecordId = curRecordId.Replace(".", _openURLTemplate.DotReplaceChar());
                    }

                    _url = _tokenProcessor.ProcessURL(_openURLTemplate.Url(), curRecordId);
                }

                if(!string.IsNullOrWhiteSpace(_openURLTemplate.FieldGroup()))
                {
                    (var functions, var rawData) = await RetrieveData(recordId, cancellationToken)
                        .ConfigureAwait(false);

                    if (rawData != null && rawData.Result != null && rawData.Result.Rows != null)
                    {
                        int i = 0;
                        foreach (DataRow dataRow in rawData.Result.Rows)
                        {
                            string recordIdUrlString = $"{{recid{i}}}";
                            string recId = dataRow.GetEscapedColumnValue("recid", "-1");
                            if (!string.IsNullOrWhiteSpace(_openURLTemplate.DotReplaceChar()))
                            {
                                recId = recId.Replace(".", _openURLTemplate.DotReplaceChar());
                            }
                            _url = _url.Replace(recordIdUrlString, recId);

                            // TODO: This is not considering multiple rows. only the record ids are considered for multiple rows.
                            // The current implementation is identical with the CRM.pad implementation but I think that was an implementation
                            // bug on CRM.pad
                            foreach (var function in functions)
                            {
                                string fieldQueryName = await _fieldGroupComponent.QueryFieldName(function.Value, cancellationToken);
                                if (!string.IsNullOrWhiteSpace(fieldQueryName))
                                {
                                    string functionUrlString = $"{{${function.Key}}}";
                                    string value = dataRow.GetEscapedColumnValue(fieldQueryName, functionUrlString);
                                    _url = _url.Replace(functionUrlString, value);
                                }
                            }
                            i++;
                        }
                    }
                }
            }
        }

        private async Task<(Dictionary<string, FieldControlField>, DataResponse)> RetrieveData(string recordId,
            CancellationToken cancellationToken)
        {
            FieldControl fieldControl = await _configurationService
                                    .GetFieldControl(_openURLTemplate.FieldGroup() + ".List", cancellationToken)
                                    .ConfigureAwait(false);

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(_openURLTemplate.FieldGroup(), cancellationToken).ConfigureAwait(false);
            string filterName = string.Empty;

            if (searchAndList != null)
            {
                filterName = searchAndList.FilterName;
            }

            if (fieldControl != null && !string.IsNullOrWhiteSpace(fieldControl.InfoAreaId))
            {
                TableInfo tableInfo = await _configurationService
                    .GetTableInfoAsync(fieldControl.InfoAreaId, cancellationToken)
                    .ConfigureAwait(false);

                _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);

                Dictionary<string, FieldControlField> functions = _fieldGroupComponent.GetFieldsFunctions(fieldControl.Tabs);
                if (functions.Keys.Count > 0)
                {
                    List<FieldControlField> fields = _fieldGroupComponent.GetQueryFields(fieldControl.Tabs);
                    if (fields.Count > 0)
                    {
                        List<Filter> filters = new List<Filter>();
                        if (!string.IsNullOrWhiteSpace(filterName))
                        {
                            filters.AddRange(await _filterProcessor.RetrieveFilterDetails(new List<string> { filterName }, cancellationToken));
                        }

                        DataResponse rawData = await RetrieveData(recordId, fieldControl, tableInfo, fields, filters, cancellationToken);

                        return (functions, rawData);
                    }
                }
            }
            return (null, null);
        }

        private async Task<DataResponse> RetrieveData(string recordId, FieldControl fieldControl, TableInfo tableInfo, List<FieldControlField> fields, List<Filter> filters, CancellationToken cancellationToken)
        {
            string rId = QueryRecordId(recordId);
            if (!string.IsNullOrWhiteSpace(rId))
            {
                return await _crmDataService.GetRecord(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = tableInfo,
                        Fields = fields,
                        RecordId = QueryRecordId(recordId),
                        Filters = filters
                    },
                    RequestMode.Best);
            }
            
            return await _crmDataService.GetData(cancellationToken,
                new DataRequestDetails
                {
                    TableInfo = tableInfo,
                    Fields = fields,
                    SortFields = fieldControl.SortFields,
                    SearchFields = null,
                    Filters = filters
                },
                null, 0, RequestMode.Offline); ;
            
        }

        public string UrlString()
        {
            return _url;
        }

        public bool IsCustomUrl()
        {
            if(!string.IsNullOrWhiteSpace(_url) && _url.ToLower().StartsWith("http"))
            {
                return false;
            }

            return true;
        }

        private string QueryRecordId(string recordId)
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                return _userAction.RecordId;
            }

            return recordId;
        }
    }
}
