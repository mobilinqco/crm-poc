using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class SerialEntryItem : INotifyPropertyChanged
    {
        public Guid RowIdentification { get; set; }
        public string RecordIdentification { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public bool ShowRowLineEndPrice { get; set; }
        public List<PanelData> Panels { get; set; }
        public List<PanelData> ChildPanels { get; set; }
        public Dictionary<string, string> SearchKeyPairs { get; set; }
        public Dictionary<string, string> FunctionKeyPairs { get; set; }
        public ItemQuota Quota { get; set; }

        private string _destRecordId;
        public string DestRecordId
        {
            get => _destRecordId;
            set
            {
                if (_destRecordId != value)
                {
                    _destRecordId = value;
                    OnPropertyChanged();
                    State = !string.IsNullOrWhiteSpace(_destRecordId) ? SerialEntryItemState.WithDestinationEntry : SerialEntryItemState.NoDestinationEntry;
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _subTitle1;
        public string SubTitle1
        {
            get => _subTitle1;
            set
            {
                if (_subTitle1 != value)
                {
                    _subTitle1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _subTitle2;
        public string SubTitle2
        {
            get => _subTitle2;
            set
            {
                if (_subTitle2 != value)
                {
                    _subTitle2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currency;
        public string Currency
        {
            get => _currency;
            set
            {
                if (_currency != value)
                {
                    _currency = value;
                    OnPropertyChanged();
                    setCountText();
                }
            }
        }

        private string _currencyCode;
        public string CurrencyCode
        {
            get => _currencyCode;
            set
            {
                if (_currencyCode != value)
                {
                    _currencyCode = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _fileImagePath;
        public string FileImagePath
        {
            get => _fileImagePath;
            set
            {
                if (_fileImagePath != value)
                {
                    _fileImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _packageCount = 1;
        public int PackageCount
        {
            get => _packageCount;
            set
            {
                if (_packageCount != value)
                {
                    _packageCount = value;
                    OnPropertyChanged();
                    
                }
            }
        }

        private decimal _unitPrice = 0;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                }
                EndPrice = value * Quantity;
            }
        }

        private decimal _discount = 0;
        public decimal Discount
        {
            get
            {
                return _discount;
            }
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged();
                }
                NetPrice = EndPrice - value;
            }
        }

        private decimal _freeGoods = 0;
        public decimal FreeGoods
        {
            get
            {
                return _freeGoods;
            }
            set
            {
                if (_freeGoods != value)
                {
                    _freeGoods = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _endPrice = -1;
        public decimal EndPrice
        {
            get
            {
                if (_endPrice < 0)
                {
                    return 0;
                }
                else
                {
                    return _endPrice;
                }
            }
            set
            {
                if (_endPrice != value)
                {
                    _endPrice = value;
                    OnPropertyChanged();
                }
                NetPrice = EndPrice - Discount;
            }
        }

        private decimal _netprice = 0;
        public decimal NetPrice
        {
            get
            {
                return _netprice > 0 ? _netprice : 0;
            }
            set
            {
                if (_netprice != value)
                {
                    _netprice = value;
                    OnPropertyChanged();
                }
                setCountText();
            }
        }


        private decimal _quantity = 0;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
                ValidateQuantity(_quantity);
                EndPrice = UnitPrice * value;
            }
        }

        private string _countText;
        public string CountText
        {
            get => _countText;
            private set
            {
                if (_countText != value)
                {
                    _countText = value;
                    OnPropertyChanged();
                }
            }
        }

        private SerialEntryItemState _state = SerialEntryItemState.NoDestinationEntry;
        public SerialEntryItemState State 
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                    if(value == SerialEntryItemState.SaveInprogress)
                    {
                        IsBusy = true;
                    }
                    else
                    {
                        IsBusy = false;
                    }
                }
            }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        private string _quantityMessage;
        public string QuantityMessage
        {
            get => _quantityMessage;
            set
            {
                _quantityMessage = value;
                ShowMessage = string.IsNullOrEmpty(_quantityMessage) ? false : true;
                OnPropertyChanged();
            }
        }

        private bool _showMessage;
        public bool ShowMessage
        {
            get => _showMessage;
            set
            {
                _showMessage = value;
                OnPropertyChanged();
            }
        }

        protected void setCountText()
        {
            if (Quantity > 0)
            {
                if (EndPrice > 0 && ShowRowLineEndPrice)
                {
                    if (string.IsNullOrEmpty(Currency))
                    {
                        CountText = $"{Quantity} - {EndPrice}";
                    }
                    else
                    {
                        CountText = $"{Quantity} - {Currency} {EndPrice}";
                    }

                }
                else
                {
                    if (UnitPrice > 0)
                    {
                        if (string.IsNullOrEmpty(Currency))
                        {
                            CountText = $"{Quantity} x {UnitPrice}";
                        }
                        else
                        {
                            CountText = $"{Quantity} x {Currency} {UnitPrice}";
                        }
                    }
                    else
                    {
                        CountText = $"{Quantity}";
                    }
                }


            }
            else if (UnitPrice > 0)
            {
                CountText = $"{Currency} {UnitPrice}";
            }
            else
            {
                CountText = Currency;
            }

        }

        public int maxQuantity
        {
            get
            {
                if(Quota != null)
                {
                    if (Quota.unlimitedQuota)
                    {
                        return 0;
                    }

                    return Quota.remainingQuota;
                }
                return 0;
            }
        }
    

        public SerialEntryItem()
        {
            ResetRowIdentification();
        }

        public void ResetRowIdentification()
        {
            RowIdentification = Guid.NewGuid();
        }

        public SerialEntryItem Copy()
        {
            var item = (SerialEntryItem)this.MemberwiseClone();
            item.ResetRowIdentification();
            item.DestRecordId = string.Empty;
            return item;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool Match(Dictionary<string, string> filterKeys)
        {
            if (filterKeys?.Keys.Count > 0)
            {
                if (SearchKeyPairs?.Keys.Count > 0)
                {
                    bool result = true;
                    foreach (var key in filterKeys.Keys)
                    {
                        if (!string.IsNullOrWhiteSpace(filterKeys[key]))
                        {
                            if (!SearchKeyPairs.ContainsKey(key) || !SearchKeyPairs[key].Equals(filterKeys[key], StringComparison.InvariantCultureIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                    return result;

                }
                else
                {
                    return false;
                }
            }
            {
                return true;
            }

        }

        protected void ValidateQuantity(decimal quantity)
        {
            HasError = false;
            QuantityMessage = string.Empty;
            if (PackageCount > 1)
            {
                var dif = quantity % PackageCount;
                if (dif > 0)
                {
                    HasError = true;
                    QuantityMessage = $"Quantity per packing unit: {PackageCount}";
                    return;
                }
            }

            if (this.maxQuantity > 0 && this.maxQuantity < quantity)
            {
                HasError = true;
                QuantityMessage = $"Quantity exceeds maximum allowed quota : {this.maxQuantity}";
                return;
            }

        }
    }
}
