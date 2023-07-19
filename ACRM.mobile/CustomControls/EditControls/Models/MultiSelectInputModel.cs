using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class MultiSelectInputModel : CatalogControlModel
    {
        public int FieldCount { get => Widgets != null ? Widgets.Count : 0; }
        private List<int> _InitialSelectedIndices;
        private ObservableCollection<int> _selectedIndices;
        private List<int> _lstSelectedIndices;
        public object SelectedIndices
        {
            get =>  Device.RuntimePlatform == Device.UWP ? (object)_lstSelectedIndices : (object)_selectedIndices;
            set
            {
                var count = getCount(value);
                if (count <= FieldCount)
                {
                    if(Device.RuntimePlatform == Device.UWP)
                    {
                        _lstSelectedIndices = (List<int>)value;
                    }
                    else
                    {
                        _selectedIndices = (ObservableCollection<int>)value;
                        _lstSelectedIndices = _selectedIndices.ToList();
                    }
                    
                    RaisePropertyChanged(() => SelectedIndices);
                    SetIndexToWidgets();
                }
                else
                {
                    RaisePropertyChanged(() => SelectedIndices);
                }
            }
            
        }

        private int? getCount(object value)
        {
            if (Device.RuntimePlatform == Device.UWP)
            {
                 var lstSelectedIndices = (List<int>)value;
                return lstSelectedIndices?.Count;
            }
            else
            {
                var selectedIndices = (ObservableCollection<int>)value;
                return selectedIndices?.Count;
            }
        }

        private object setSelectedIndices(List<int> initialSelectedIndices)
        {
            return Device.RuntimePlatform == Device.UWP ? (object)initialSelectedIndices : (object)new ObservableCollection<int>(initialSelectedIndices);
        }

        private void SetIndexToWidgets()
        {
            for (int i = 0; i < FieldCount; i++)
            {
                var selectedItemCount = _lstSelectedIndices.Count;
                var widget = Widgets[i] as CatalogControlModel;
                if (i < selectedItemCount)
                {
                    var index = _lstSelectedIndices[i];
                    widget.SetSelectedIndex(index);
                }
                else
                {
                    widget.SetSelectedIndex(-1);
                }
            }
        }

        public override List<ListDisplayField> Fields
        {
            get
            {
                var fields = new List<ListDisplayField>();
                if(Widgets != null) 
                {
                    foreach(var widgetObj in Widgets)
                    {
                        var widget =  widgetObj as CatalogControlModel;
                        if (widget.Field.EditData.HasDataChanged)
                        {
                            widget.Field.EditData.ChangeOfflineRequest = widget.ChangeOfflineRequest;
                        }
                        fields.Add(widget.Field);
                    }
                }

                return fields;
            }
        }

        public MultiSelectInputModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
            var Cat = field?.Config?.PresentationFieldAttributes?.FieldInfo.Cat;
            Widgets = new ObservableCollection<UIWidget>();
            foreach (var cField in field.Data.ColspanData)
            {
                var cCat = cField?.Config?.PresentationFieldAttributes?.FieldInfo.Cat;
                if (Cat != null && Cat == cCat)
                {
                    var widget = new CatalogControlModel(cField, _cancellationTokenSource);
                    widget.ParentBaseModel = this;
                    _ = widget.InitializeControl();
                    Widgets.Add(widget);
                }
            }
            if (_parentCatalogId > 0 && _isCatalog)
            {
                // Child Catalog which need to set only after parent is set 
                EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.ParentCatalogChanged, _parentCatalogId.ToString(), ParentCatalogChangedEventHandler));
                EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.InitalizeSelectedItem, null, InitalizeSelectedItemEventHandler));

            }
            else
            {
                _ = InitalizeSelectedItemEventHandler(null);
            }
        }

        private Task InitalizeSelectedItemEventHandler(WidgetMessage arg)
        {
            _InitialSelectedIndices = new List<int>();
            if (AllowedValues != null && AllowedValues.Count > 0)
            {
                foreach (CatalogControlModel widget in Widgets)
                {
                    var index = AllowedValues.FindIndex(c => c.RecordId.Equals(widget.Field.EditData?.DefaultSelectedValue?.RecordId));
                    if (index >= 0)
                    {
                        _InitialSelectedIndices.Add(index);
                    }
                }

                SelectedIndices = setSelectedIndices(_InitialSelectedIndices);
            }
            return Task.CompletedTask;
        }

        private Task ParentCatalogChangedEventHandler(WidgetMessage arg)
        {
            if (arg != null && arg.Data is SelectableFieldValue parentItem && _allCatalogValues != null)
            {
                var allowedValues = _allCatalogValues.Where(c => c.ParentCode.ToString().Equals(parentItem.RecordId)).ToList();
                if (allowedValues == null)
                {
                    AllowedValues.Add(new SelectableFieldValue
                    {
                        RecordId = string.Empty,
                        DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                            LocalizationKeys.KeyBasicEmptyCatalog)
                    });
                }
                else
                    AllowedValues = new ObservableCollection<SelectableFieldValue>(allowedValues);

            }
            else if (arg != null && arg.Data == null)
            {
                AllowedValues.Add(new SelectableFieldValue
                {
                    RecordId = string.Empty,
                    DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                        LocalizationKeys.KeyBasicEmptyCatalog)
                });

            }

            SelectedIndices = setSelectedIndices(new List<int>());

            return Task.CompletedTask;
        }

    }
}
