using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.SerialEntry;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class SerialEntryUiItem: SerialEntryItem
    {
        private ISerialEntryListing _ISerialEntryListing;
        public ICommand MinusButtonCommand => new Command(async () => await OnMinusButtonClicked());
        public ICommand PlusButtonCommand => new Command(async () => await OnPlusButtonClicked());
        public ICommand DeleteButtonCommand => new Command(async () => await OnDeleteButtonClicked());
        public ICommand DuplicateButtonCommand => new Command(async () => await OnDuplicateButtonClicked());
        public ICommand SelectedItemCommand => new Command(async (item) => await SelectedItemCommandHandlerc());

        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<UIWidget> _widgets;
        public ObservableCollection<UIWidget> Widgets
        {
            get => _widgets;
            set
            {
                if (_widgets != value)
                {
                    _widgets = value;
                    _childWidgets.RegisterWidgetEvents(WidgetEventType.ValueChanged, "Quantity", QuantityChangedEventHandler);
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<UIWidget> _childWidgets;
        public ObservableCollection<UIWidget> ChildWidgets
        {
            get => _childWidgets;
            set
            {
                if (_childWidgets != value)
                {
                    _childWidgets = value;
                    _childWidgets.RegisterWidgetEvents(WidgetEventType.ValueChanged, "Quantity", QuantityChangedEventHandler);
                    OnPropertyChanged();
                }
            }
        }

        public ImageSource FileImageSource
        {
            get
            {
                if (!string.IsNullOrEmpty(FileImagePath))
                {
                    return ImageSource.FromFile(FileImagePath);
                }
                else
                {
                    ResourceDictionary StaticResources = Application.Current.Resources;
                    return new FontImageSource
                    {
                        FontFamily = (OnPlatform<string>)StaticResources["HeaderButtonImagesFont"],
                        Glyph = "\uE060",
                        Color = Color.LightSkyBlue
                    };
                }


            }

        }

        private CancellationTokenSource _cancellationTokenSource;

        public SerialEntryUiItem(ISerialEntryListing iSerialEntryListing, SerialEntryItem item, CancellationTokenSource parentCancellationTokenSource)
        {
            _cancellationTokenSource = parentCancellationTokenSource;
            _ISerialEntryListing = iSerialEntryListing;
            foreach (var prop in item.GetType().GetProperties())
            {
                var methodInfo = this.GetType().GetProperty(prop.Name).GetSetMethod();
                if (methodInfo != null && methodInfo.IsPublic)
                {
                    this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(item, null), null);
                }
            }

        }

        public async Task LoadWidgetsData()
        {
            await LoadParentWidgets();
            await LoadChildWidgets();
            ValidateQuantity(Quantity);
        }

        private async Task LoadParentWidgets()
        {
            if (Panels != null && Panels.Count > 0)
            {
                Widgets = await Panels.BuildWidgetsAsyc(null, _cancellationTokenSource);
            }
        }

        private async Task OnDeleteButtonClicked()
        {
            Quantity = 0;
            if (_ISerialEntryListing != null && !string.IsNullOrWhiteSpace(DestRecordId))
            {
               await _ISerialEntryListing.ItemDeleted(this);
            }
        }
        private async Task OnDuplicateButtonClicked()
        {
            if (_ISerialEntryListing != null)
            {
                await UpdateDataState();
                SerialEntryItem item = this.Copy();
                item.Panels = Widgets?.GetPanelDatas();
                item.ChildPanels = ChildWidgets?.GetPanelDatas();
                await _ISerialEntryListing.ItemModified(item);
            }
        }
        private async Task SelectedItemCommandHandlerc()
        {
            if (_ISerialEntryListing != null)
            {
               
                await _ISerialEntryListing.ItemSelected(this);
            }
        }
        private async Task OnMinusButtonClicked()
        {
            if (Quantity >0)
            {
                Quantity = Quantity - PackageCount;
                await loadChildRecords();
                await UpdateDataState();
                if (_ISerialEntryListing != null)
                {
                    await _ISerialEntryListing.ItemModified(this);
                }
            }

        }

        private async Task OnPlusButtonClicked()
        {
            Quantity = Quantity + PackageCount;
            await loadChildRecords();
            await UpdateDataState();
            if (_ISerialEntryListing != null)
            {
                await _ISerialEntryListing.ItemModified(this);
            }
        }

        private async Task loadChildRecords()
        {
            if (_ISerialEntryListing != null)
            {
                await _ISerialEntryListing.LoadChildPanels(this);
                LoadChildWidgets();
            }
            await CopyToChildren();
        }

        private async Task UpdateDataState()
        {
            await _ISerialEntryListing?.EvaluatePricing(this);
            await Widgets.UpdateField("CopyItemNumber", ItemNumber);
            await Widgets.UpdateField("CopyItemName", ItemName);
            await Widgets.UpdateField("Quantity", Quantity);
            await Widgets.UpdateField("UnitPrice", UnitPrice);
            await Widgets.UpdateField("EndPrice", EndPrice);
            await Widgets.UpdateField("Discount", Discount);
            await Widgets.UpdateField("NetPrice", NetPrice);
            await Widgets.UpdateField("Currency", CurrencyCode);
            await Widgets.UpdateField("FreeGoods", FreeGoods);
            this.ReloadPanels();
        }
        public void ReloadPanels()
        {
            Panels = Widgets?.GetPanelDatas();
            ChildPanels = ChildWidgets?.GetPanelDatas();
        }

        private decimal GetSumValueDecimal(string function, List<PanelData> panels)
        {
            decimal result = 0;
            if (panels != null && panels.Count > 0)
            {
                foreach (var panel in panels)
                {
                    var fieldIndex = panel.Fields.FindIndex(f => f.Config.FieldConfig.Function.Equals(function, StringComparison.InvariantCultureIgnoreCase));
                    if (fieldIndex >= 0)
                    {
                       var value = panel.Fields[fieldIndex].EditData.StringValue;
                       if(decimal.TryParse(value,out decimal dresult))
                        {
                            result = result + dresult;
                        }
                    }
                }
            }
            return result;
        }
        private long GetSumValueLong(string function, List<PanelData> panels)
        {
            long result = 0;
            if (panels != null && panels.Count > 0)
            {
                foreach (var panel in panels)
                {
                    var fieldIndex = panel.Fields.FindIndex(f => f.Config.FieldConfig.Function.Equals(function, StringComparison.InvariantCultureIgnoreCase));
                    if (fieldIndex >= 0)
                    {
                        var value = panel.Fields[fieldIndex].EditData.StringValue;
                        if (long.TryParse(value, out long lresult))
                        {
                            result = result + lresult;
                        }
                    }
                }
            }
            return result;
        }

        internal async Task SyncData()
        {
            if (IsChildModified())
            {
                Quantity = GetSumValueLong("Quantity", ChildPanels);
                Discount = GetSumValueDecimal("Discount", ChildPanels);
                FreeGoods = GetSumValueDecimal("FreeGoods", ChildPanels);
                await UpdateDataState();
                if (_ISerialEntryListing != null)
                {
                    await _ISerialEntryListing.ItemModified(this);
                }
            }
            else if (IsRecordModified())
            {
                this.ReloadPanels();
                if (_ISerialEntryListing != null)
                {
                    await _ISerialEntryListing.ItemModified(this);
                }

            }


        }

        private bool IsChildModified()
        {
            var panels = ChildPanels;
            if (panels != null && panels.Count > 0)
            {
                return panels.HasChanges(true);
            }
            return false;
        }

        internal bool IsRecordModified()
        {
            var panels = Panels;
            if (panels != null && panels.Count > 0)
            {
                return panels.HasChanges(true);
            }
            return false;
        }

        internal async Task LoadChildWidgets()
        {
            if (ChildPanels != null && ChildPanels.Count > 0)
            {
                ChildWidgets = await ChildPanels.BuildWidgetsAsyc(null, _cancellationTokenSource);
            }
        }

        internal async Task CopyToChildren()
        {
            if (ChildPanels != null && ChildWidgets !=null && ChildWidgets.Count>0 && ChildPanels.Count > 0)
            {
                var childSumQuantity = GetSumValueLong("Quantity", ChildPanels);
                if (childSumQuantity < Quantity)
                {
                    string ItemQuantityString = ChildPanels[0].GetValue("Quantity");
                    decimal decItemQuantity;
                    decimal.TryParse(ItemQuantityString, out decItemQuantity);
                    if (!string.IsNullOrEmpty(ItemQuantityString))
                    {
                        var newQuantity = decItemQuantity + (Quantity - childSumQuantity);
                        await ChildWidgets[0].UpdateField("Quantity", newQuantity);
                    }

                }
            }

        }

        private async Task QuantityChangedEventHandler(WidgetMessage arg)
        {
            if (arg != null)
            {
                string strData = arg.Data.ToString();
                decimal quantity;
                if (!string.IsNullOrWhiteSpace(strData) && decimal.TryParse(strData, out quantity) && quantity > 0)
                {
                    // Any action when quantity changed.
                }

            }

        }
    }
}
