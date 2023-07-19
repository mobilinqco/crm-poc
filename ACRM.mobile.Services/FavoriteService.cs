using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class FavoriteService : ContentServiceBase, IFavoriteService
    {
        protected OrganizerActionTemplate _template;
        protected IOfflineRequestsService _offlineStoreService;
        protected TableInfo _tableInfo;
        protected Filter _templateFilter;

        private List<PanelData> _panels;

        private const string _infoAreaId = "FV";
        private const string _templateFilterName = "FV.new";

        public string AddToFavoritesImageName => "Icon:Star";
        public string RemoveFromFavoritesImageName => "Icon:StarEmpty";

        public bool _isBusy = false;

        private bool _isRecordFavorite = false;

        public FavoriteService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IOfflineRequestsService offlineStoreService,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineStoreService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _isBusy = true;

            ViewReference vr = _action.ViewReference;
            _template = new OrganizerActionTemplate(vr);

            var filters = await _filterProcessor.RetrieveFilterDetails(new List<string> { _templateFilterName }, cancellationToken);
            if (filters.Count > 0)
            {
                _templateFilter = filters[0];
            }

            FieldControl fieldControl = await _configurationService.GetFieldControl(_infoAreaId + ".List", cancellationToken);

            if (fieldControl == null)
            {
                fieldControl = new FieldControl {
                    ControlName = "FV.Default",
                    InfoAreaId = _infoAreaId,
                    Tabs = new List<FieldControlTab>()
                };
            }

            _tableInfo = await _configurationService.GetTableInfoAsync(_infoAreaId, cancellationToken);
            _fieldGroupComponent.InitializeContext(fieldControl, _tableInfo);
            _panels = await PanelsAsync(cancellationToken);

            await UpdateFavoriteRecord(cancellationToken);

            InitFavoriteStatus();

            _isBusy = false;
        }

        public async Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken)
        {
            List<PanelData> result = new List<PanelData>();
            Dictionary<string, string> templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_templateFilter, cancellationToken);

            if (_fieldGroupComponent.HasTabs())
            {
                foreach (FieldControlTab panel in _fieldGroupComponent.FieldControl.Tabs.OrderBy(t => t.OrderId))
                {
                    if (panel.IsSupported() && !panel.IsHeaderPanel())
                    {
                        PanelData pd = new PanelData
                        {
                            Label = panel.Label,
                            Type = panel.GetPanelType(),
                            RecordId = _action.RecordId,
                            RecordInfoArea = _action.SourceInfoArea
                        };

                        List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
                        ListDisplayRow outRow = new ListDisplayRow();
                        outRow.Fields.AddRange((await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, null, templateFilterValues, panel.GetEditPanelType(), cancellationToken)).Fields);
                        
                        if (outRow.Fields.Count > 0)
                        {
                            pd.Fields = outRow.Fields;
                            result.Add(pd);
                        }
                    }
                }
            }

            // Add the link field even if no other fields are avaiable. There are some bastards that have deleted all the FV definitions
            PanelData lpd = new PanelData
            {
                Label = "FV.Link",
                Type = PanelType.EditPanel,
                RecordId = _action.RecordId,
                RecordInfoArea = _action.SourceInfoArea
            };

            ListDisplayRow lOutRow = new ListDisplayRow();
            lOutRow.Fields.Add(GetLinkListDisplayField());
            lpd.Fields = lOutRow.Fields;
            result.Add(lpd);

            return result;
        }

        private ListDisplayField GetLinkListDisplayField()
        {
            // TODO currently hardcoded with some unnecessary attributes
            SelectableFieldValue selectableFieldValue = new SelectableFieldValue
            {
                RecordId = _action.RecordId
            };

            FieldControlField fieldControlField = new FieldControlField
            {
                InfoAreaId = _action.SourceInfoArea,
                LinkId = 126
            };

            FieldInfo fieldInfo = new FieldInfo();

            PresentationFieldAttributes presentationFieldAttributes = new PresentationFieldAttributes(
                fieldControlField, fieldInfo, _configurationService.GetServerTimezone(), EditModes.New);

            CrmFieldConfiguration crmFieldConfiguration = new CrmFieldConfiguration(
                fieldControlField, presentationFieldAttributes);

            ListDisplayField linkListDisplayField = new ListDisplayField();
            linkListDisplayField.EditData.DefaultSelectedValue = selectableFieldValue;
            linkListDisplayField.Config = crmFieldConfiguration;

            return linkListDisplayField;
        }

        private async Task UpdateFavoriteRecord(CancellationToken cancellationToken)
        {
            List<FieldControlField> fields = GetQueryFields(_fieldGroupComponent.FieldControl.Tabs);

            ParentLink parentLink = new ParentLink
            {
                LinkId = 126,
                ParentInfoAreaId = _action.SourceInfoArea,
                RecordId = _action.RecordId
            };

            _rawData = await _crmDataService.GetRecord(cancellationToken,
                new DataRequestDetails { TableInfo = _tableInfo, Fields = fields },
                RequestMode.Offline, parentLink);
        }

        private void InitFavoriteStatus()
        {
            if(_rawData.Result != null)
            {
                _isRecordFavorite = true;
                var (imageName, glyphText) = _imageResolverComponent.ExtractImage(_configurationService, AddToFavoritesImageName);
                _action.DisplayImageName = imageName;
                _action.DisplayGlyphImageText = glyphText;
            }
            else
            {
                _isRecordFavorite = false;
                var (imageName, glyphText) = _imageResolverComponent.ExtractImage(_configurationService, RemoveFromFavoritesImageName);
                _action.DisplayImageName = imageName;
                _action.DisplayGlyphImageText = glyphText;
            }
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

        public async Task<bool> HandleFavoriteStatus(CancellationToken cancellationToken)
        {
            if(_isRecordFavorite)
            {
                return await RemoveRecordFromFavorites(cancellationToken);
            }
            else
            {
                return await AddRecordToFavorites(cancellationToken);
            }
        }

        private async Task<bool> RemoveRecordFromFavorites(CancellationToken cancellationToken)
        {
            if (_rawData.Result != null && _rawData.Result.Rows.Count > 0 && _rawData.Result.Rows[0].Table.Columns.Contains("recid"))
            {
                try
                {
                    _isBusy = true;

                    OfflineRequest offlineRequest = await _offlineStoreService.CreateDeleteRequest(_template, _fieldGroupComponent.FieldControl.InfoAreaId, _rawData.Result.Rows[0]["recid"].ToString());

                    ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

                    if (!modifyRecordResult.HasSaveErrors())
                    {
                        _logService.LogDebug("Record has been deleted.");
                        await _offlineStoreService.Delete(offlineRequest, cancellationToken);
                        _isRecordFavorite = false;
                    }
                    else
                    {
                        _logService.LogError($"Error adding to favourites {modifyRecordResult.ErrorMessage()}");
                        await _offlineStoreService.Update(offlineRequest, cancellationToken);
                        throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
                    }

                    await UpdateFavoriteRecord(cancellationToken);

                    _isBusy = false;
                }
                catch(Exception e)
                {
                    _isBusy = false;
                    throw e;
                }
            }

            return _isRecordFavorite;
        }

        private async Task<bool> AddRecordToFavorites(CancellationToken cancellationToken)
        {
            try
            {
                _isBusy = true;

                Dictionary<string, string> templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_templateFilter, cancellationToken);
                OfflineRequest offlineRequest = await _offlineStoreService.CreateSaveRequest(_template,
                    _fieldGroupComponent.FieldControl,
                    _tableInfo,
                    _panels,
                    templateFilterValues,
                    new List<OfflineRecordLink> { _action?.GetLinkParentRequest() },
                    cancellationToken);

                ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

                if (!modifyRecordResult.HasSaveErrors())
                {
                    _logService.LogDebug("New record has been saved.");
                    await _offlineStoreService.Delete(offlineRequest, cancellationToken);
                    _isRecordFavorite = true;
                }
                else
                {
                    _logService.LogError($"Error adding to favourites {modifyRecordResult.ErrorMessage()}");
                    await _offlineStoreService.Update(offlineRequest, cancellationToken);
                    throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
                }

                await UpdateFavoriteRecord(cancellationToken);

                _isBusy = false;

                return _isRecordFavorite;
            }
            catch(Exception e)
            {
                _isBusy = false;
                throw e;
            }
        }

        public bool IsBusy()
        {
            return _isBusy;
        }

        public bool IsRecordFavorite()
        {
            return _isRecordFavorite;
        }
    }
}
