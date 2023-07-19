using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace ACRM.mobile.Services.SubComponents
{
    public class TableCaptionComponent
    {
        private readonly IConfigurationService _configurationService;
        private readonly ICrmDataService _crmDataService;
        private readonly ILogService _logService;
        private readonly CatalogComponent _catalogComponent;
        private readonly FieldDataProcessor _fieldDataProcessor;

        private TableInfo _tableInfo;
        private string _tableCaptionName;
        private TableCaption _tableCaption;
        private string _recordId;
        private List<FieldControlField> _captionFields;
        private string _serverTimezone;

        public TableCaptionComponent(IConfigurationService configurationService,
            ICrmDataService crmDataService,
            CatalogComponent catalogComponent,
            FieldDataProcessor fieldDataProcessor,
            ILogService logService)
        {
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _catalogComponent = catalogComponent;
            _fieldDataProcessor = fieldDataProcessor;
            _logService = logService;
        }

        public async Task InitializeContext(string tableCaptionName, TableInfo tableInfo, string recordId, CancellationToken cancellationToken)
        {
            _logService.LogDebug($"Processing TableCaption {tableCaptionName}");

            _recordId = recordId;
            _tableInfo = tableInfo;
            _tableCaptionName = tableCaptionName;
            if (string.IsNullOrEmpty(_tableCaptionName))
            {
                _tableCaptionName = _tableInfo.InfoAreaId;
                _logService.LogDebug($"TableCaption is empty. Using the InfoAreaId: {_tableCaptionName}");
            }

            _tableCaption = await _configurationService.GetTableCaption(_tableCaptionName, cancellationToken);
            if (_tableCaption == null && !_tableInfo.InfoAreaId.Equals(_tableInfo.RootInfoAreaId))
            {
                _tableCaptionName = _tableInfo.RootInfoAreaId;
                _logService.LogDebug($"No TableCaption defined. Trying with RootInfoAreaId: {_tableCaptionName}");
                _tableCaption = await _configurationService.GetTableCaption(_tableCaptionName, cancellationToken);
            }
            _captionFields = new List<FieldControlField>();
            if (_tableCaption != null)
            {
               var fields = ExtractFromJsonString(_tableCaption.Fields);
                foreach (TableCaptionField field in fields)
                {
                    _captionFields.Add(new FieldControlField
                    {
                        FieldId = field.FieldId,
                        InfoAreaId = field.InfoAreaId,
                        LinkId = field.LinkId,
                        OrderId = field.Position
                    });
                }
            }

            _serverTimezone = _configurationService.GetServerTimezone();
        }

        public List<FieldControlField> CaptionFields 
        {
            get
            {
                return _captionFields;
            }
        }

        public async Task<ListDisplayRow> CaptionText(CancellationToken cancellationToken, DataRow dataRow = null)
        {
            if (_tableCaption == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(_tableCaption.Fields))
            {
                return null;
            }

            string captionString = _tableCaption.FormatString;

            if (string.IsNullOrWhiteSpace(captionString) || _captionFields?.Count < 1)
            {
                return null;
            }

            DataRow row = dataRow;

            if (row == null)
            {
                DataResponse rawData = await _crmDataService.GetRecord(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = _tableInfo,
                        Fields = _captionFields,
                        RecordId = _recordId
                    });

                if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                {
                    row = rawData.Result.Rows[0];
                }

            }

            List<string> tableCaptionValues = new List<string>();

            if (row != null)
            {
                List<ListDisplayField> colspanValues = new List<ListDisplayField>();
                PresentationFieldAttributes colspanPfa = null;
                FieldControlField colspanFd = null;
                foreach (FieldControlField fd in _captionFields)
                {
                    string fieldName = fd.QueryFieldName(fd.InfoAreaId != _tableInfo.InfoAreaId);
                    if (row.Table.Columns.Contains(fieldName))
                    {
                        FieldInfo fieldInfo = await _configurationService.GetFieldInfo(_tableInfo, fd, cancellationToken);
                        PresentationFieldAttributes pfa = new PresentationFieldAttributes(fd, fieldInfo, _serverTimezone, EditModes.DetailsOrAll, captionString);

                        if (colspanPfa == null)
                        {
                            colspanPfa = pfa;
                            colspanFd = fd;
                        }

                        string fieldValue = await _fieldDataProcessor.ExtractDisplayValue(row, fieldInfo, pfa, fieldName, cancellationToken);
                        tableCaptionValues.Add(fieldValue);


                        ListDisplayField ldf = new ListDisplayField
                        {
                            Data = new CrmFieldDisplayData { StringData = fieldValue },
                            Config = new CrmFieldConfiguration((FieldControlField)fd.Clone(), (PresentationFieldAttributes)pfa.Clone())
                        };
                        colspanValues.Add(ldf);
                    }
                }

                string specialCaptionString = string.Empty;
                if (_tableCaption.SpecialDefs != null && _tableCaption.SpecialDefs.Count > 0)
                {
                    foreach (TableCaptionSpecialDefs tcsd in _tableCaption.SpecialDefs)
                    {
                        int trueCounter = 0;
                        List<int> emptyFields = ExtractListOfEmptyFieldsFromJsonString(tcsd.EmptyFields);
                        foreach (int fieldId in emptyFields)
                        {
                            if (fieldId >= tableCaptionValues.Count || string.IsNullOrWhiteSpace(tableCaptionValues[fieldId]))
                            {
                                trueCounter++;
                            }
                        }

                        if (trueCounter > 0 && trueCounter == emptyFields.Count)
                        {
                            specialCaptionString = tcsd.FormatString;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(specialCaptionString))
                {
                    colspanPfa.UpdateCombineString(specialCaptionString);
                }

                ListDisplayRow outRow = new ListDisplayRow();
                outRow.Fields.Add(new ListDisplayField
                {
                    Data = new CrmFieldDisplayData { ColspanData = new List<ListDisplayField>(colspanValues) },
                    Config = new CrmFieldConfiguration(colspanFd, colspanPfa)
                });

                return outRow;
            }

            return null;
        }

        private List<TableCaptionField> ExtractFromJsonString(string value)
        {
            List<TableCaptionField> result = new List<TableCaptionField>();
            if (value != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<TableCaptionField>>(value);
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Unable to extract TableCaption fields from {value}: {ex.Message}");
                }
            }

            return result;
        }

        private List<int> ExtractListOfEmptyFieldsFromJsonString(string value)
        {
            List<int> result = new List<int>();
            if (value != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<int>>(value);
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Unable to extract TableCaption Empty Fields List from {value}: {ex.Message}");
                }
            }

            return result;
        }

    }
}
