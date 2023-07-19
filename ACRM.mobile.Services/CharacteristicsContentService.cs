using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Characteristics;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using AsyncAwaitBestPractices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class CharacteristicsContentService : ContentServiceBase, ICharacteristicsContentService
    {
        private readonly IOfflineRequestsService _offlineStoreService;
        private readonly ICharacteristicsGroupService _characteristicsGroupService;
        private readonly ICharacteristicsItemService _characteristicsItemService;

        private ActionTemplateBase _actionTemplate;
        private Expand _expand;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private bool _isEdit = false;

        private string _groupFieldName = "";
        private string _itemFieldName = "";

        private FieldInfo _groupFieldInfo;
        private FieldInfo _itemFieldInfo;
        private Dictionary<int, FieldInfo> _additionalValueFieldInfoIdDict = new Dictionary<int, FieldInfo>();

        // Ordered using catalogs ordering rules
        private List<SelectableFieldValue> _groupSelectableFieldValues = new List<SelectableFieldValue>();
        private List<SelectableFieldValue> _itemSelectableFieldValues = new List<SelectableFieldValue>();

        private HashSet<string> _characteristicGroupSet = new HashSet<string>();
        private Dictionary<string, List<CharacteristicsAdditionalValue>> _characteristicItemValuesDict = new Dictionary<string, List<CharacteristicsAdditionalValue>>();
        private Dictionary<string, string> _characteristicItemRecordIdDict = new Dictionary<string, string>();

        //Group attributes
        private bool _characteristicsGroupAttributesEnabled = false;
        private HashSet<string> _visibleCharacteristicsGroups = new HashSet<string>();
        private Dictionary<string, bool> _characteristicsGroupsSingleSelectionValues = new Dictionary<string, bool>();
        private Dictionary<string, bool> _characteristicsGroupsExpandedValues = new Dictionary<string, bool>();

        //Item Attributes
        private bool _characteristicsItemAttributesEnabled = false;
        private HashSet<string> _visibleCharacteristicsItems = new HashSet<string>();
        private Dictionary<string, bool> _characteristicsItemsShowAdditionalFieldsValues = new Dictionary<string, bool>();

        private List<CharacteristicGroup> _editableCharacteristicGroups = new List<CharacteristicGroup>();
        private List<CharacteristicGroup> _characteristicGroups = new List<CharacteristicGroup>();

        public CharacteristicsContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IOfflineRequestsService offlineRequestsService,
            ICharacteristicsGroupService characteristicsGroupService,
            ICharacteristicsItemService characteristicsItemService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _offlineStoreService = offlineRequestsService;
            _characteristicsGroupService = characteristicsGroupService;
            _characteristicsItemService = characteristicsItemService;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");
            
            ViewReference vr = _action.ViewReference;
            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken);
            }

            if (vr.IsCharacteristicsEditAction())
            {
                _isEdit = true;
                await ProcessCharacteristicsEditActionTemplate(vr, cancellationToken);
            }
            else
            {
                await ProcessRecordViewActionTemplate(vr, cancellationToken);
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private async Task ProcessCharacteristicsEditActionTemplate(ViewReference viewReference, CancellationToken cancellationToken)
        {
            CharacteristicsEditActionTemplate characteristicsEdit = new CharacteristicsEditActionTemplate(viewReference);
            _actionTemplate = characteristicsEdit;

            string expandName = characteristicsEdit.ExpandName();
            if (string.IsNullOrEmpty(expandName))
            {
                expandName = _action.InfoAreaUnitName;
            }

            _expand = _configurationService.GetExpand(expandName);
            _infoArea = _configurationService.GetInfoArea(_expand.InfoAreaId);

            _fieldControl = await _configurationService.GetFieldControl(_expand.FieldGroupName + ".Edit", cancellationToken);
            if (_fieldControl == null)
            {
                _fieldControl = await _configurationService.GetFieldControl(_expand.FieldGroupName + ".Details", cancellationToken);
            }

            _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            await PrepareSearchAndContentServices(characteristicsEdit, cancellationToken);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                if (fieldDefinitions.Count > 0)
                {
                    await GetCharacteristicsCatalogs(fieldDefinitions, cancellationToken);

                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    await GetCharactersiticsCatalogFields(fieldDefinitions, cancellationToken);
                }
            }

            BuildEditableCharacteristicsGroups();
        }

        private async Task PrepareSearchAndContentServices(CharacteristicsEditActionTemplate characteristicsEdit, CancellationToken cancellationToken)
        {
            if(!string.IsNullOrEmpty(characteristicsEdit.GroupSearchAndList()))
            {
                _characteristicsGroupAttributesEnabled = true;
                _characteristicsGroupService.SetSourceAction(_action);
                await _characteristicsGroupService.PrepareContentAsync(cancellationToken);
                _visibleCharacteristicsGroups = _characteristicsGroupService.GetVisibleCharacteristicsGroupCodes();
                _characteristicsGroupsSingleSelectionValues = _characteristicsGroupService.GetCharacteristicsGroupsSingleSelectionValues();
                _characteristicsGroupsExpandedValues = _characteristicsGroupService.GetCharacteristicsGroupsExpandedValues();


            }

            if(!string.IsNullOrEmpty(characteristicsEdit.ItemSearchAndList()))
            {
                _characteristicsItemAttributesEnabled = true;
                _characteristicsItemService.SetSourceAction(_action);
                await _characteristicsItemService.PrepareContentAsync(cancellationToken);
                _visibleCharacteristicsItems = _characteristicsItemService.GetVisibleCharacteristicsItemCodes();
                _characteristicsItemsShowAdditionalFieldsValues = _characteristicsItemService.GetCharacteristicsItemsShowAdditionalFieldsValues();
            }
        }

        private async Task ProcessRecordViewActionTemplate(ViewReference viewReference, CancellationToken cancellationToken)
        {
            RecordViewTemplate recordView = new RecordViewTemplate(viewReference);
            _actionTemplate = recordView;

            string expandName = recordView.ExpandName();
            if (string.IsNullOrEmpty(expandName))
            {
                expandName = _action.InfoAreaUnitName;
            }
            _expand = _configurationService.GetExpand(expandName);
            _infoArea = _configurationService.GetInfoArea(_expand.InfoAreaId);
            _fieldControl = await _configurationService.GetFieldControl(_expand.FieldGroupName + ".List", cancellationToken);
            _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                if (fieldDefinitions.Count > 0)
                {
                    await GetCharacteristicsCatalogs(fieldDefinitions, cancellationToken);

                    ParentLink parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);

                    _rawData = await _crmDataService.GetData(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions },
                        parentLink,
                        requestMode: RequestMode.Best);

                    await GetCharactersiticsCatalogFields(fieldDefinitions, cancellationToken);
                }
            }

            BuildCharacteristicsGroups();
        }

        private async Task GetCharacteristicsCatalogs(List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {
                FieldInfo fieldInfo = _configurationService.GetFieldInfo(_tableInfo, field.FieldId);

                if (fieldInfo.IsCatalog)
                {
                    if (field.Function == "Group")
                    {
                        _groupFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                        _groupFieldInfo = fieldInfo;
                        _groupSelectableFieldValues.AddRange(await _fieldGroupComponent.ExtractAllowedCatalogValues(fieldInfo, cancellationToken));
                    }
                    if (field.Function == "Item")
                    {
                        _itemFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                        _itemFieldInfo = fieldInfo;
                        _itemSelectableFieldValues.AddRange(await _fieldGroupComponent.ExtractAllowedCatalogValues(fieldInfo, cancellationToken));
                    }
                }
                else if(fieldInfo.FieldType.Equals('C'))
                {
                    _additionalValueFieldInfoIdDict.Add(fieldInfo.FieldId, fieldInfo);
                }
            }
        }

        private async Task GetCharactersiticsCatalogFields(List<FieldControlField> fieldDefinitions, CancellationToken cancellationToken)
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
            {
                foreach (DataRow row in _rawData.Result.Rows)
                {
                    string groupCode = row[_groupFieldName].ToString();
                    string itemCode = row[_itemFieldName].ToString();

                    // There should be only one record per itemCode
                    if(_characteristicItemRecordIdDict.ContainsKey(itemCode))
                    {
                        continue;
                    }

                    _characteristicItemRecordIdDict.Add(itemCode, row["recid"].ToString());

                    List<CharacteristicsAdditionalValue> tempAdditionalFieldValues = new List<CharacteristicsAdditionalValue>();

                    ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, cancellationToken);

                    // Get all additionalFieldValues.
                    // The response contains by default all additional fields with empty or populated values
                    if (outRow != null)
                    {
                        foreach (ListDisplayField field in outRow.Fields)
                        {
                            string fieldName = field.Config.FieldConfig.QueryFieldName(!field.Config.FieldConfig.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));

                            int fieldId = field.Config.FieldConfig.FieldId;

                            if (fieldName != _groupFieldName && fieldName != _itemFieldName && _additionalValueFieldInfoIdDict.ContainsKey(fieldId))
                            {
                                if (_isEdit)
                                {
                                    tempAdditionalFieldValues.Add(new CharacteristicsAdditionalValue(fieldId, _additionalValueFieldInfoIdDict[fieldId].Name, field.Data.StringData));
                                }
                                else if (!string.IsNullOrEmpty(field.Data.StringData))
                                {
                                    tempAdditionalFieldValues.Add(new CharacteristicsAdditionalValue(fieldId, _additionalValueFieldInfoIdDict[fieldId].Name, field.Data.StringData));
                                }
                            }
                        }
                    }

                    if(!_characteristicGroupSet.Contains(groupCode))
                    {
                        _characteristicGroupSet.Add(groupCode);
                    }

                    if(!_characteristicItemValuesDict.ContainsKey(itemCode))
                    {
                        _characteristicItemValuesDict.Add(itemCode, tempAdditionalFieldValues);
                    }
                    else
                    {
                        var additionalFieldValues = _characteristicItemValuesDict[itemCode];
                        additionalFieldValues.AddRange(tempAdditionalFieldValues);
                    }
                }
            }
        }

        private void BuildEditableCharacteristicsGroups()
        {
            //Groups and Items are already sorted in the catalog collections.
            //First we construct the item lists in sorted order for each corresponding group.
            //Then we assign the items lists to the groups which are also added to a list in sorted order.
            Dictionary<string, List<CharacteristicItem>> characteristicGroupItemList = new Dictionary<string, List<CharacteristicItem>>();

            foreach (SelectableFieldValue selectableFieldValue in _itemSelectableFieldValues)
            {
                if(!IsEditableCharacteristicsItemVisible(selectableFieldValue.RecordId) || !IsEditableCharacteristicsGroupVisible(selectableFieldValue.ParentCode.ToString()))
                {
                    continue;
                }

                string recordId = null;
                if (_characteristicItemRecordIdDict.ContainsKey(selectableFieldValue.RecordId))
                {
                    recordId = _characteristicItemRecordIdDict[selectableFieldValue.RecordId];
                }

                if (!characteristicGroupItemList.ContainsKey(selectableFieldValue.ParentCode.ToString()))
                {
                    
                    CharacteristicItem characteristicItem = BuildEditableCharacteristicItem(selectableFieldValue, recordId);
                    var list = new List<CharacteristicItem>()
                    { 
                        characteristicItem 
                    };
                    characteristicGroupItemList.Add(selectableFieldValue.ParentCode.ToString(), list);
                }
                else
                {
                    CharacteristicItem characteristicItem = BuildEditableCharacteristicItem(selectableFieldValue, recordId);
                    var list = characteristicGroupItemList[selectableFieldValue.ParentCode.ToString()];
                    list.Add(characteristicItem);
                }
            }

            foreach (SelectableFieldValue selectableFieldValue in _groupSelectableFieldValues)
            {
                if (characteristicGroupItemList.ContainsKey(selectableFieldValue.RecordId) && IsEditableCharacteristicsGroupVisible(selectableFieldValue.RecordId))
                {
                    bool isExpandable = true;
                    if (_characteristicsGroupsExpandedValues.ContainsKey(selectableFieldValue.RecordId))
                    {
                        isExpandable = _characteristicsGroupsExpandedValues[selectableFieldValue.RecordId];
                    }

                    CharacteristicGroup characteristicGroup = new CharacteristicGroup(_groupFieldInfo.FieldId,
                        selectableFieldValue.DisplayValue, isExpandable);
                    characteristicGroup.CharacteristicItems.AddRange(characteristicGroupItemList[selectableFieldValue.RecordId]);
                    _editableCharacteristicGroups.Add(characteristicGroup);
                }
            }
        }

        public bool IsEditableCharacteristicsGroupVisible(string groupCode)
        {
            if (_characteristicsGroupAttributesEnabled && !_visibleCharacteristicsGroups.Contains(groupCode))
            {
                return false;
            }
            return true;
        }

        public bool IsEditableCharacteristicsItemVisible(string itemCode)
        {
            if (_characteristicsItemAttributesEnabled && !_visibleCharacteristicsItems.Contains(itemCode))
            {
                return false;
            }
            return true;
        }

        private CharacteristicItem BuildEditableCharacteristicItem(SelectableFieldValue selectableFieldValue, string recordId)
        {
            bool isSingleSelection = false;
            if (_characteristicsGroupsSingleSelectionValues.ContainsKey(selectableFieldValue.ParentCode.ToString()))
            {
                isSingleSelection = _characteristicsGroupsSingleSelectionValues[selectableFieldValue.ParentCode.ToString()];
            }

            bool showAdditionalFields = true;
            if (_characteristicsItemsShowAdditionalFieldsValues.ContainsKey(selectableFieldValue.RecordId))
            {
                showAdditionalFields = _characteristicsItemsShowAdditionalFieldsValues[selectableFieldValue.RecordId];
            }

            CharacteristicItem characteristicItem = new CharacteristicItem(_groupFieldInfo.FieldId, selectableFieldValue.ParentCode.ToString(),
                _itemFieldInfo.FieldId, selectableFieldValue.RecordId, recordId, selectableFieldValue.DisplayValue, isSingleSelection, showAdditionalFields);
            if (_characteristicItemValuesDict.ContainsKey(selectableFieldValue.RecordId))
            {
                characteristicItem.AdditionalValues.AddRange(_characteristicItemValuesDict[selectableFieldValue.RecordId]);
            }
            else
            {
                foreach (KeyValuePair<int, FieldInfo> entry in _additionalValueFieldInfoIdDict)
                {
                    characteristicItem.AdditionalValues.Add(new CharacteristicsAdditionalValue(entry.Key, entry.Value.Name, ""));
                }
            }

            return characteristicItem;
        }

        private void BuildCharacteristicsGroups()
        {
            //Groups and Items are already sorted in the catalog collections.
            //First we construct the item lists in sorted order for each corresponding group.
            //Then we assign the items lists to the groups which are also added to a list in sorted order.
            Dictionary<string, List<CharacteristicItem>> characteristicGroupItemList = new Dictionary<string, List<CharacteristicItem>>();

            foreach (SelectableFieldValue selectableFieldValue in _itemSelectableFieldValues)
            {

                string recordId = null;
                if (_characteristicItemRecordIdDict.ContainsKey(selectableFieldValue.RecordId))
                {
                    recordId = _characteristicItemRecordIdDict[selectableFieldValue.RecordId];
                }

                if (_characteristicItemValuesDict.ContainsKey(selectableFieldValue.RecordId))
                {
                    if(!characteristicGroupItemList.ContainsKey(selectableFieldValue.ParentCode.ToString()))
                    {
                        CharacteristicItem characteristicItem = BuildCharacteristicItem(selectableFieldValue, recordId, selectableFieldValue.DisplayValue);
                        var list = new List<CharacteristicItem>
                        {
                            characteristicItem
                        };
                        characteristicGroupItemList.Add(selectableFieldValue.ParentCode.ToString(), list);
                    }
                    else
                    {
                        CharacteristicItem characteristicItem = BuildCharacteristicItem(selectableFieldValue, recordId, selectableFieldValue.DisplayValue);
                        var list = characteristicGroupItemList[selectableFieldValue.ParentCode.ToString()];
                        list.Add(characteristicItem);
                    }
                }
            }

            foreach (SelectableFieldValue selectableFieldValue in _groupSelectableFieldValues)
            {
                if (_characteristicGroupSet.Contains(selectableFieldValue.RecordId) && characteristicGroupItemList.ContainsKey(selectableFieldValue.RecordId))
                {
                    bool isExpandable = true;
                    if (_characteristicsGroupsExpandedValues.ContainsKey(selectableFieldValue.RecordId))
                    {
                        isExpandable = _characteristicsGroupsExpandedValues[selectableFieldValue.RecordId];
                    }

                    CharacteristicGroup characteristicGroup = new CharacteristicGroup(_groupFieldInfo.FieldId,
                        selectableFieldValue.DisplayValue, isExpandable);
                    characteristicGroup.CharacteristicItems.AddRange(characteristicGroupItemList[selectableFieldValue.RecordId]);
                    _characteristicGroups.Add(characteristicGroup);
                }
            }
        }

        private CharacteristicItem BuildCharacteristicItem(SelectableFieldValue selectableFieldValue, string recordId, string displayValue)
        {
            bool isSingleSelection = false;
            if (_characteristicsGroupsSingleSelectionValues.ContainsKey(selectableFieldValue.ParentCode.ToString()))
            {
                isSingleSelection = _characteristicsGroupsSingleSelectionValues[selectableFieldValue.ParentCode.ToString()];
            }

            bool showAdditionalFields = true;
            if (_characteristicsItemsShowAdditionalFieldsValues.ContainsKey(selectableFieldValue.RecordId))
            {
                showAdditionalFields = _characteristicsItemsShowAdditionalFieldsValues[selectableFieldValue.RecordId];
            }

            CharacteristicItem characteristicItem = new CharacteristicItem(_groupFieldInfo.FieldId, selectableFieldValue.ParentCode.ToString(),
                _itemFieldInfo.FieldId, selectableFieldValue.RecordId, recordId, selectableFieldValue.DisplayValue, isSingleSelection, showAdditionalFields);
            characteristicItem.AdditionalValues.AddRange(_characteristicItemValuesDict[selectableFieldValue.RecordId]);
            return characteristicItem;
        }

        public List<CharacteristicGroup> GetEditableCharacteristicGroups()
        {
            return _editableCharacteristicGroups;
        }

        public List<CharacteristicGroup> GetCharacteristicGroups()
        {
            return _characteristicGroups;
        }

        public async Task<ModifyRecordResult> Save(Dictionary<int, string> currentFieldValues, Dictionary<int, string> oldFieldValues, string recordId, CancellationToken cancellationToken)
        {
            PanelData panelData = new PanelData() { RecordId = recordId };
            FieldControlTab panel = _fieldGroupComponent.FieldControl.Tabs[0];
            List<FieldControlField> fieldDefinitions = panel.GetQueryFields();
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, null, null, panel.GetEditPanelType(), cancellationToken);

            if (outRow.Fields.Count > 0)
            {
                foreach(ListDisplayField listDisplayField in outRow.Fields)
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

            List<OfflineRecordLink>  recordLinks = new List<OfflineRecordLink>();

            string parentLinkId = _action.GetLinkId();

            if (!int.TryParse(parentLinkId, out int intParentLinkId))
            {
                intParentLinkId = 0;
            }

            var parentLinkInfo = _tableInfo.GetLinkInfo(_action.SourceInfoArea, intParentLinkId);

            if(parentLinkInfo != null)
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

        public async Task<ModifyRecordResult> Delete(string recordId, CancellationToken cancellationToken)
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
