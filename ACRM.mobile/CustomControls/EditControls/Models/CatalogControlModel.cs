using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using AsyncAwaitBestPractices;
using Syncfusion.SfAutoComplete.XForms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class CatalogControlModel: BaseEditControlModel
    {

        protected string _initialSelectedRecordId;
        protected int _parentCatalogId;
        protected bool _isCatalog;
        protected List<SelectableFieldValue> _allCatalogValues;
        protected bool _isParentCatalog = false;
        protected string _fieldId;
        private bool _allowFiltering = false;
        public bool AllowFiltering
        {
            get => _allowFiltering;
            set
            {
                _allowFiltering = value;
                RaisePropertyChanged(() => AllowFiltering);
            }
        }

        private bool _isEditableMode = false;
        public bool IsEditableMode
        {
            get => _isEditableMode;
            set
            {
                _isEditableMode = value;
                RaisePropertyChanged(() => IsEditableMode);
            }
        }

        private string _noResultsFoundText;
        public string NoResultsFoundText
        {
            get => _noResultsFoundText;
            set
            {
                _noResultsFoundText = value;
                RaisePropertyChanged(() => NoResultsFoundText);
            }
        }

        public override SelectableFieldValue SelectedValue
        {
            get
            {
                var sValue = Field?.EditData?.SelectedValue;
                if (sValue != null)
                {
                    var Item = AllowedValues.FirstOrDefault(c => c.RecordId.Equals(Field?.EditData?.SelectedValue.RecordId));
                    if (Item != null)
                    {
                        return Item;
                    }
                    
                }
                return null;


            }
            set
            {

                Field.EditData.SelectedValue = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(_fieldId) && _isParentCatalog)
                {
                    ParentBaseModel?.PublishMessage(new WidgetMessage
                    {
                        ControlKey = _fieldId,
                        Data = Field.EditData.SelectedValue,
                        EventType = WidgetEventType.ParentCatalogChanged
                    }, MessageDirections.Both).SafeFireAndForget<Exception>(onException: ex =>
                    {
                        _logService.LogError($"Unable to set child catalogs {ex.Message}");
                    });
                }

                if (_skipValueChangedMessage)
                {
                    _skipValueChangedMessage = false;
                    return;
                }

                if (NotifyValueChanged)
                {
                    string fieldFunction = Field?.Config?.FieldConfig?.Function;
                    if (!string.IsNullOrEmpty(fieldFunction))
                    {
                        new Action(async () => await PublishMessage(new WidgetMessage
                        {
                            ControlKey = fieldFunction,
                            Data = value,
                            EventType = WidgetEventType.ValueChanged
                        }, MessageDirections.ToParent))();
                    }
                }
            }
        }

        internal void SetSelectedIndex(int index)
        {
            if (index < 0)
            {
                SelectedValue = null;
            }
            else if (AllowedValues != null && AllowedValues.Count > index)
            {
                SelectedValue = AllowedValues[index];
            }

        }

        public CatalogControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
            _initialSelectedRecordId = field.EditData.DefaultSelectedValue?.RecordId;
            _fieldId = Field.Config?.PresentationFieldAttributes?.FieldInfo.FieldId.ToString();
            _parentCatalogId = field?.Config?.PresentationFieldAttributes?.FieldInfo?.Ucat ?? 0;
            _isCatalog = field?.Config?.PresentationFieldAttributes?.FieldInfo.IsCatalog ?? false;


            _allCatalogValues = field.Config.AllowedValues;

            if (_isCatalog)
            {
                if (!string.IsNullOrEmpty(_fieldId))
                {
                    EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.InitalizeParentCatalog, _fieldId, InitalizeParentCatalogEventHandler));
                    EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.SetSelectedIndex, _fieldId, SetSelectedIndexEventHandler));
                }
            }

            if (_parentCatalogId > 0 && _isCatalog)
            {
                // Child Catalog which need to set only after parent is set 
                EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.ParentCatalogChanged, _parentCatalogId.ToString(), ParentCatalogChangedEventHandler));
                if (!string.IsNullOrWhiteSpace(_initialSelectedRecordId))
                {
                    EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.InitalizeSelectedItem, null, InitalizeSelectedItemEventHandler));
                }
            }
            else
            {
                AllowedValues = new ObservableCollection<SelectableFieldValue>(field.Config.AllowedValues);
                if (!string.IsNullOrEmpty(field?.Config?.PresentationFieldAttributes?.FieldInfo.RepMode)
                    || (field.Config.AllowedValues.Count > 5 && _parentCatalogId < 1))
                {
                    AllowFiltering = true;
                    IsEditableMode = true;
                    NoResultsFoundText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                        LocalizationKeys.KeyErrorsNoResults);
                }
                _ = InitalizeSelectedItemEventHandler(null);
            }

            if (field.Config.AllowedValues == null || field.Config.AllowedValues.Count == 0)
            {
                AllowedValues.Add(new SelectableFieldValue
                {
                    RecordId = string.Empty,
                    DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                        LocalizationKeys.KeyBasicEmptyCatalog)
                });
            }

        }

        private async Task InitalizeParentCatalogEventHandler(WidgetMessage arg)
        {
            _isParentCatalog = true;
            await InitalizeSelectedItemEventHandler(arg);
        }

        private async Task SetSelectedIndexEventHandler(WidgetMessage arg)
        {
            if (arg != null && arg.Data is int index)
            {
                SetSelectedIndex(index);
            }
        }

        private Task InitalizeSelectedItemEventHandler(WidgetMessage arg)
        {
            if (AllowedValues != null && AllowedValues.Count > 0 && !string.IsNullOrWhiteSpace(_initialSelectedRecordId))
            {
                var Item = AllowedValues.FirstOrDefault(c => c.RecordId.Equals(_initialSelectedRecordId));
                if (Item == null && SelectedValue != null)
                {
                    Item = AllowedValues.FirstOrDefault(c => c.DisplayValue.Equals(SelectedValue.DisplayValue));
                }
                if (Item != null)
                {
                    Field.EditData.DefaultSelectedValue = Item;
                }

                SelectedValue = Item;
            }

            return Task.CompletedTask;
        }

        private Task ParentCatalogChangedEventHandler(WidgetMessage arg)
        {
            if (arg != null && arg.Data is SelectableFieldValue parentItem && _allCatalogValues != null)
            {

                var allowedValues = _allCatalogValues.Where(c => c.ParentCode.ToString().Equals(parentItem.RecordId)).ToList();
                AllowedValues = new ObservableCollection<SelectableFieldValue>(allowedValues);

                if (SelectedValue != null && !string.IsNullOrWhiteSpace(SelectedValue.RecordId))
                {
                    SelectedValue = AllowedValues.FirstOrDefault(c => c.RecordId.Equals(SelectedValue.RecordId));
                }
            }
            else if (arg != null && arg.Data == null)
            {
                AllowedValues.Add(new SelectableFieldValue
                {
                    RecordId = string.Empty,
                    DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                        LocalizationKeys.KeyBasicEmptyCatalog)
                });

                SelectedValue = null;
            }

            return Task.CompletedTask;
        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (_parentCatalogId > 0 && _isCatalog)
            {
                await ParentBaseModel?.PublishMessage(new WidgetMessage
                {
                    ControlKey = null,
                    Data = _parentCatalogId.ToString(),
                    EventType = WidgetEventType.AddParentCatalog
                }, MessageDirections.ToParent);
            }

            return true;
        }
    }
}
