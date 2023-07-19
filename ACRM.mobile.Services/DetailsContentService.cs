using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System.Linq;
using ACRM.mobile.Domain.Application.ActionTemplates;
using System.Threading;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Domain.FormatUtils;
using System.Globalization;

namespace ACRM.mobile.Services
{
    public class DetailsContentService : ContentServiceBase, IDetailsContentService
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IDataSyncService _dataSyncService;
        private readonly IOfflineRequestsService _offlineRequestsService;
        private readonly ExpandComponent _expandComponent;

        private RecordViewTemplate _recordView;
        private Expand _expand;
        private readonly TableCaptionComponent _tableCaptionComponent;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;


        private ListDisplayRow _organizerHeaderSubText;
        public ListDisplayRow OrganizerHeaderSubText() => _organizerHeaderSubText;

        private bool _isSyncRecordInProgress = false;
        private bool _isOnlineRecord = false;
        public bool IsOnlineRecord() => _isOnlineRecord;

        private ListDisplayRow _headerTableCaptionText;
        public ListDisplayRow HeaderTableCaptionText() => _headerTableCaptionText;

        List<PanelData> _panels;
        public List<PanelData> Panels() => _panels;

        private bool _didFavoriteServiceInit = false;
        

        public DetailsContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            TableCaptionComponent tableCaptionComponent,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            ExpandComponent expandComponent,
            IFavoriteService favoriteService,
            IFilterProcessor filterProcessor,
            IOfflineRequestsService offlineRequestsService,
            IDataSyncService dataSyncService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _tableCaptionComponent = tableCaptionComponent;
            _favoriteService = favoriteService;
            _offlineRequestsService = offlineRequestsService;
            _dataSyncService = dataSyncService;
            _expandComponent = expandComponent;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            ViewReference vr = _action.ViewReference;
            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken);
            }

            _recordView = new RecordViewTemplate(vr);
            
            _infoArea = _configurationService.GetInfoArea(_action.InfoAreaUnitName);
            _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
            
            _expand = await ResolveExpand(_action, _recordView, _tableInfo, cancellationToken);

            _fieldControl = await _configurationService.GetFieldControl(_expand.FieldGroupName + ".Details", cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fields = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);
                if (fields.Count > 0)
                {
                    _rawData = await _crmDataService.GetRecord(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fields, RecordId = _action.RecordId },
                        DetermineRequestMode());
                }

            }
            await LoadHeadersAsync(cancellationToken);
            OnDataReady();
        }

        private async Task<Expand> ResolveExpand(UserAction action, RecordViewTemplate recordView, TableInfo tableInfo, CancellationToken cancellationToken)
        {
            string expandName = recordView.ConfigName();

            if(string.IsNullOrWhiteSpace(expandName))
            {
                expandName = string.IsNullOrWhiteSpace(recordView.ExpandName()) ? action.InfoAreaUnitName : recordView.ExpandName();
            }

            if (!string.IsNullOrWhiteSpace(expandName))
            {
                _expandComponent.InitializeContext(expandName, action.InfoAreaUnitName, tableInfo);
                List<FieldControlField> fields = new List<FieldControlField>();

                List<FieldControlField> expandFields = _expandComponent.GetExpandRuleFields(expandName);
                foreach (var field in expandFields)
                {
                    if (!fields.Exists(f => f.FieldId == field.FieldId && f.InfoAreaId == field.InfoAreaId))
                    {
                        fields.Add(field);
                    }
                }

                if (fields.Count > 0)
                {
                    DataResponse rawData = await _crmDataService.GetRecord(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fields, RecordId = _action.RecordId },
                        DetermineRequestMode());
                    if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                    {
                        return _expandComponent.ResolveExpand(expandName, rawData.Result.Rows[0]);
                    }
                }

                return _configurationService.GetExpand(expandName);
            }
            else
            {
                expandName = action.ResolvedExpandName;
                if (string.IsNullOrWhiteSpace(expandName))
                {
                    return _configurationService.GetExpand(expandName);
                }
            }

            return null;
            
        }

        public async Task LoadHeadersAsync(CancellationToken cancellationToken)
        {
            string headerName = _expand.HeaderGroupName + ".Expand";
            Header header = await _configurationService.GetHeader(headerName, cancellationToken);
            _headerComponent.InitializeContext(header, _action);

            _organizerHeaderSubText = await OrganizerHeaderSubTextAsync(cancellationToken).ConfigureAwait(false);
            _headerTableCaptionText = await HeaderTableCaptionTextAsync(cancellationToken).ConfigureAwait(false);
            _headerButtons = await HeaderButtons(cancellationToken).ConfigureAwait(false);

            if (_rawData != null)
            {
                _isOnlineRecord = _rawData.IsRetrievedOnline;
            }

            foreach (UserAction userAction in _headerButtons)
            {
                userAction.ResolvedExpandName = _expand.UnitName;
                if (_didFavoriteServiceInit) 
                {
                    break;
                }
                if (userAction.ActionUnitName.Equals("ToggleFavorite"))
                {
                    await PrepareFavoriteServiceAsync(userAction, cancellationToken);
                }
            }
        }

        private RequestMode DetermineRequestMode()
        {
            if(_sessionContext.IsInOfflineMode)
            {
                return RequestMode.Offline;
            }

            if(_action.IsRecordRetrievedOnline)
            {
                return RequestMode.Best;
            }

            if(_recordView != null)
            {
                return _recordView.GetRequestMode();
            }

            return RequestMode.Best;
        }

        private async Task PrepareFavoriteServiceAsync(UserAction userAction, CancellationToken cancellationToken)
        {
            _favoriteService.SetSourceAction(userAction);
            await _favoriteService.PrepareContentAsync(cancellationToken);
            _didFavoriteServiceInit = true;
        }

        public async Task<List<PanelData>> LoadPanelsContentAsync(CancellationToken cancellationToken)
        {
            _panels = await PanelsAsync(cancellationToken).ConfigureAwait(false);
            return _panels;
        }

        public string HeaderImageName()
        {
            FieldControlField field = _fieldGroupComponent.HeaderImageField();

            if (field != null && _rawData.Result != null
                && _rawData.Result.Rows.Count > 0)
            {
                DataRow dr = _rawData.Result.Rows[0];
                string fieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                if (_rawData.Result.Columns.Contains(fieldName) &&
                    !string.IsNullOrEmpty(dr[fieldName].ToString()))
                {
                    return dr[fieldName].ToString();
                }
            }
            return string.Empty;
        }

        public async Task<ListDisplayRow> OrganizerHeaderSubTextAsync(CancellationToken cancellationToken)
        {
            FieldControlTab ohsl = _fieldGroupComponent.OrganizerHeaderSubLabel();
            if (ohsl != null)
            {
                if (_rawData.Result != null && _rawData.Result.Rows.Count > 0)
                {
                    DataRow row = _rawData.Result.Rows[0];
                    List<FieldControlField> fieldDefinitions = ohsl.GetQueryFields();
                    return await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, cancellationToken);
                }
            }
            return null;
        }

        private async Task<ListDisplayRow> HeaderTableCaptionTextAsync(CancellationToken cancellationToken)
        {
            await _tableCaptionComponent.InitializeContext(_expand.TableCaptionName, _fieldGroupComponent.TableInfo, _action.RecordId, cancellationToken);

            return await _tableCaptionComponent.CaptionText(cancellationToken);
        }

        private async Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken)
        {
            List<PanelData> result = new List<PanelData>();
            bool _hasPanels = false;
            if (_fieldGroupComponent.HasTabs())
            {
                bool hideEmptyFields = _configurationService.GetBoolConfigValue("View.HideEmptyFields", false);
                bool show0Float = _configurationService.GetBoolConfigValue("Format.Show0InListsForFloat", false);
                bool show0 = _configurationService.GetBoolConfigValue("Format.Show0InLists", false);

                foreach (FieldControlTab panel in _fieldGroupComponent.FieldControl.Tabs.OrderBy(t => t.OrderId))
                {
                    _hasPanels = true;
                    if (panel.IsSupported() && !panel.IsHeaderPanel())
                    {
                        PanelData pd = new PanelData
                        {
                            Label = panel.Label,
                            Type = panel.GetPanelType(),
                            RecordId = _action.RecordId,
                            RecordInfoArea = _action.SourceInfoArea,
                            PanelTypeKey = panel.Type,
                            action = _action
                        };

                        if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
                        {
                            List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
                            DataRow row = _rawData.Result.Rows[0];
                            ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, cancellationToken);
                            pd.Fields = outRow.Fields;
                            if (pd.Type != PanelType.Grid && fieldDefinitions.Count > 0 )
                            {
                                List<ListDisplayField> fields = new List<ListDisplayField>();
                                int emptyNumbersCounter = 0;
                                foreach (ListDisplayField field in pd.Fields)
                                {
                                    if (!field.Config.PresentationFieldAttributes.Hide)
                                    {
                                        bool isEmpty = false;
                                        if (!string.IsNullOrWhiteSpace(field.Data.StringData))
                                        {
                                            if (field.Config.PresentationFieldAttributes.IsNumeric)
                                            {
                                                if (double.TryParse(field.Data.StringData, NumberStyles.Number, CultureInfo.InvariantCulture, out double value))
                                                {
                                                    if (Math.Abs(value) < double.Epsilon)
                                                    {
                                                        emptyNumbersCounter++;

                                                        if(field.Config.PresentationFieldAttributes.FieldInfo.IsReal)
                                                        {
                                                            if(show0Float)
                                                            {
                                                                field.Data.StringData = "0";
                                                                isEmpty = false;
                                                            }
                                                            else
                                                            {
                                                                isEmpty = true;
                                                            }
                                                        }

                                                        if (field.Config.PresentationFieldAttributes.FieldInfo.IsNumeric)
                                                        {
                                                            if (show0)
                                                            {
                                                                field.Data.StringData = "0";
                                                                isEmpty = false;
                                                            }
                                                            else
                                                            {
                                                                isEmpty = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (field.Config.PresentationFieldAttributes.IsBoolean)
                                            {
                                                if(field.Data.StringData.Equals("0") && !field.Config.PresentationFieldAttributes.FieldInfo.ShowZero)
                                                {
                                                    isEmpty = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isEmpty = true;
                                        }

                                        if (!isEmpty || !hideEmptyFields)
                                        {
                                            fields.Add(field);
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
                        else
                        {
                            result.Add(pd);
                        }
                    }
                }
            }

            if(_hasPanels && result.Count == 0 )
            {
                PanelData pd = new PanelData
                {
                    Label = _action.ActionType.ToString(),
                    Type = PanelType.NotSupported,
                    RecordId = _action.RecordId,
                    RecordInfoArea = _action.SourceInfoArea,
                    PanelTypeKey = _action?.ViewReference?.Name
                };

                result.Add(pd);
            }

            return result;
        }

        public new async Task<List<UserAction>> HeaderButtons(CancellationToken cancellationToken)
        {
            return await _headerComponent.HeaderButtons(cancellationToken, _action.RecordId, _action.InfoAreaUnitName, _action.RawRecordId, _action.IsRecordRetrievedOnline);
        }

        public new List<UserAction> HeaderRelatedInfoAreas()
        {
            return _headerComponent.HeaderRelatedInfoAreas(_action);
        }

        public async Task<PanelData> PreparePanelDataAsync(PanelData inputArgs, CancellationToken token)
        {
            var typeParts = inputArgs.PanelTypeKey.Split('_');
            int LinkId;
            if (typeParts.Length > 1)
            {
                string fieldGroupName;
                var configParts = ((string)typeParts[1]).Split('#');
                if (configParts.Length > 1)
                {
                    fieldGroupName = configParts[0];
                    LinkId = Convert.ToInt32(configParts[1]);
                }
                else
                {
                    fieldGroupName = (string)typeParts[1];
                    LinkId = -1;
                }

                int parentTabIndex;
                if (typeParts.Length > 2)
                {
                    parentTabIndex = Convert.ToInt32(typeParts[2]);
                    if (parentTabIndex >= 1)
                    {
                        parentTabIndex--;
                    }
                }
                else
                {
                    parentTabIndex = 0;
                }
                _fieldControl = await _configurationService.GetFieldControl(fieldGroupName + ".Details", token);

                if (_fieldControl.Tabs.Count <= parentTabIndex || parentTabIndex < 0)
                {
                    parentTabIndex = 0;
                }

                _fieldControl = _fieldControl.FieldControlWithSingleTab(parentTabIndex);
                _tableInfo = await _configurationService.GetTableInfoAsync(_fieldControl.InfoAreaId, token);
                _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);
                if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
                {
                    List<FieldControlField> fields = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);
                    ParentLink parentLink = new ParentLink
                    {
                        LinkId = LinkId,
                        ParentInfoAreaId = inputArgs.RecordInfoArea,
                        RecordId = inputArgs.RecordId
                    };
                    if (fields.Count > 0)
                    {
                        _rawData = await _crmDataService.GetRecord(token,
                            new DataRequestDetails { TableInfo = _tableInfo, Fields = fields, RecordId = inputArgs.FormatedRecordId },
                            DetermineRequestMode(), parentLink);
                    }

                }

                if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
                {

                    List<FieldControlField> fieldDefinitions = _fieldControl.Tabs[0].GetQueryFields();
                    DataRow row = _rawData.Result.Rows[0];
                    ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, token);
                    inputArgs.Fields = outRow.Fields;
                    inputArgs.Type = PanelType.List;
                    
                    if (inputArgs.Type != PanelType.Grid)
                    {
                        List<ListDisplayField> fields = new List<ListDisplayField>();
                        int emptyNumbersCounter = 0;
                        foreach (ListDisplayField field in inputArgs.Fields)
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
                            inputArgs.Fields = fields;

                        }
                    }
                    
                }

            }
            return inputArgs;
        }

        public bool IsRecordFavorite()
        {
            return _favoriteService.IsRecordFavorite();
        }

        public bool IsFavoriteServiceBusy()
        {
            return _favoriteService.IsBusy();
        }

        public Task<bool> HandleFavoriteStatus(CancellationToken cancellationToken)
        {
            return _favoriteService.HandleFavoriteStatus(cancellationToken);
        }

        public (string imageName, string glyphText) GetAddToFavoritesImageName()
        {
            return ExtractImage(_favoriteService.AddToFavoritesImageName);
        }

        public (string imageName, string glyphText) GetRemoveFromFavoritesImageName()
        {
            return ExtractImage(_favoriteService.RemoveFromFavoritesImageName);
        }

        

        public bool IsSyncRecordServiceBusy()
        {
            return _isSyncRecordInProgress;
        }

        public async Task SyncRecord(UserAction userAction, CancellationToken cancellationToken)
        {
            _isSyncRecordInProgress = true;
            string recordId = userAction.RecordId;
            string sourceInfoArea = userAction.SourceInfoArea;
            recordId = recordId.FormatedRecordId(sourceInfoArea);
            await _dataSyncService.SyncRecord(recordId, _configurationService, cancellationToken);
            _isSyncRecordInProgress = false;
        }

        public (string imageName, string glyphText) ExtractImage(string resourceName)
        {
            return _imageResolverComponent.ExtractImage(_configurationService, resourceName);
        }

        public async Task<ModifyRecordResult> DeleteRecord(UserAction userAction, CancellationToken cancellationToken)
        {
            OrganizerActionTemplate actionTemplate = new OrganizerActionTemplate(userAction.ViewReference);
            OfflineRequest offlineRequest = await _offlineRequestsService.CreateDeleteRequest(actionTemplate, _fieldControl.InfoAreaId, _action.RecordId);

            ModifyRecordResult modifyRecordResult = await _crmDataService.ModifyRecord(cancellationToken, _tableInfo, offlineRequest);

            if (!modifyRecordResult.HasSaveErrors())
            {
                _logService.LogDebug("Record has been deleted.");
                await _offlineRequestsService.Delete(offlineRequest, cancellationToken);
            }
            else
            {
                await _offlineRequestsService.Update(offlineRequest, cancellationToken);
                throw new CrmException(modifyRecordResult.UserErrorMessage(), CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataRequestError);
            }

            return modifyRecordResult;
        }

        public bool DisplayEmptyPanels()
        {
            return _configurationService.GetBoolConfigValue("View.DetailsShowEmptyPanels");
        }
    }
}
