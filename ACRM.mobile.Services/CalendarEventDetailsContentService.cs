using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class CalendarEventDetailsContentService : ContentServiceBase, ICalendarEventDetailsContentService
    {
        private CalendarViewTemplate _calendarViewTemplate;
        List<PanelData> _panels;
        public List<PanelData> Panels() => _panels;

        public CalendarEventDetailsContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            FieldControl fieldControl = await _configurationService.GetFieldControl(_calendarViewTemplate.CalendarPopOverConfig() + ".Details", cancellationToken);

            _infoArea = _configurationService.GetInfoArea(fieldControl.InfoAreaId);

            TableInfo tableInfo = await _configurationService.GetTableInfoAsync(fieldControl.InfoAreaId, cancellationToken);

            _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);

            if (fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fields = GetQueryFields(fieldControl.Tabs);
                _rawData = await _crmDataService.GetRecord(cancellationToken,
                    new DataRequestDetails { TableInfo = tableInfo, Fields = fields, RecordId = _action.RecordId },
                    RequestMode.Best);
            }

            _panels = await PanelsAsync(cancellationToken).ConfigureAwait(false);
            OnDataReady();
        }

        private List<FieldControlField> GetQueryFields(List<FieldControlTab> controlTabs)
        {
            List<FieldControlField> fields = new List<FieldControlField>();

            foreach (FieldControlTab tab in controlTabs)
            {
                fields.AddRange(tab.Fields);
            }

            return fields;
        }

        private async Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken)
        {
            List<PanelData> result = new List<PanelData>();
            if (_rawData?.Result != null
                && _rawData.Result.Rows.Count > 0
                && _fieldGroupComponent.HasTabs())
            {
                foreach (FieldControlTab panel in _fieldGroupComponent.FieldControl.Tabs.OrderBy(t => t.OrderId))
                {
                    if (panel.IsSupported() && !panel.IsHeaderPanel())
                    {
                        PanelData pd = new PanelData
                        {
                            Label = panel.Label,
                            Type = panel.GetPanelType(),
                            RecordId = _action.RecordId
                        };

                        List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
                        DataRow row = _rawData.Result.Rows[0];
                        ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, cancellationToken);
                        pd.Fields = outRow.Fields;

                        if (pd.Type != PanelType.Grid)
                        {
                            List<ListDisplayField> fields = new List<ListDisplayField>();
                            int emptyNumbersCounter = 0;
                            foreach (ListDisplayField field in pd.Fields)
                            {
                                if (!field.Config.PresentationFieldAttributes.Hide)
                                {
                                    fields.Add(field);

                                    if (!string.IsNullOrEmpty(field.Data.StringData))
                                    {
                                        if (field.Config.PresentationFieldAttributes.IsNumeric)
                                        {
                                            if (double.TryParse(field.Data.StringData, NumberStyles.Number, CultureInfo.InvariantCulture, out double value))
                                            {
                                                if (Math.Abs(value) < double.Epsilon)
                                                {
                                                    emptyNumbersCounter++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (fields.Count > 0 && fields.Count != emptyNumbersCounter)
                            {
                                pd.Fields = fields;
                                result.Add(pd);
                            }
                        }
                        else
                        {
                            result.Add(pd);
                        }
                    }
                }
            }

            return result;
        }

        public void SetCalendarViewTemplate(CalendarViewTemplate calendarViewTemplate)
        {
            _calendarViewTemplate = calendarViewTemplate;
        }
    }
}
