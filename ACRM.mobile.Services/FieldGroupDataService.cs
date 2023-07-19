using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class FieldGroupDataService: IFieldGroupDataService
    {
        private IConfigurationService _configurationService;
        private ICrmDataService _crmDataService;
        private ILogService _logService;

        public FieldGroupDataService(IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService)
        {
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _logService = logService;
        }

        public async Task<Dictionary<string, string>> GetSourceFieldGroupData(string sourceRecordId, string fieldGroupName, RequestMode requestMode, CancellationToken cancellationToken)
        {
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(fieldGroupName))
            {
                FieldControl fieldControl = await _configurationService.GetFieldControl(fieldGroupName + ".List", cancellationToken);
                if(fieldControl != null && !string.IsNullOrWhiteSpace(fieldControl.InfoAreaId))
                {
                    TableInfo tableInfo = await _configurationService.GetTableInfoAsync(fieldControl.InfoAreaId, cancellationToken).ConfigureAwait(false);
                    if(tableInfo != null)
                    {
                        List<FieldControlField> fields = GetCopyFields(fieldControl);
                        if(fields.Count > 0)
                        {
                            DataResponse rawData = await _crmDataService.GetRecord(cancellationToken,
                                new DataRequestDetails { TableInfo = tableInfo,
                                    Fields = fields,
                                    RecordId = sourceRecordId },
                                requestMode);

                            if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                            {
                                DataRow dr = rawData.Result.Rows[0];
                                foreach(var field in fields)
                                {
                                    if (!string.IsNullOrEmpty(field.Function))
                                    {
                                        if (string.IsNullOrWhiteSpace(field.ExplicitLabel))
                                        {
                                            string fieldName = field.QueryFieldName(!field.InfoAreaId.Equals(tableInfo.InfoAreaId));
                                            if (rawData.Result.Columns.Contains(fieldName))
                                            {
                                                if (!fieldValues.ContainsKey(field.Function))
                                                {
                                                    fieldValues.Add(field.Function, dr[fieldName].ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!fieldValues.ContainsKey(field.Function))
                                            {
                                                fieldValues.Add(field.Function, field.ExplicitLabel);
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }
                    }
                }
                
            }
            return fieldValues;
        }


        private List<FieldControlField> GetCopyFields(FieldControl fieldControl)
        {

            List<FieldControlField> fields = new List<FieldControlField>();

            foreach(var tab in fieldControl.Tabs)
            {
                foreach(var field in tab.GetQueryFields())
                {
                    if (!string.IsNullOrWhiteSpace(field.Function))
                    {
                        fields.Add(field);
                    }
                }
            }

            return fields;
        }
    }
}
