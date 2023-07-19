using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class PricingScaleItem
    {
        public string ParentRecordIdentification { get; set; }
        public string RecordIdentification { get; set; }
        public string InfoAreaId { get; set; }
        public string ItemNumber { get; set; }
        public Dictionary<string, string> FunctionKeyPairs { get; set; }
        public decimal ExchangeRate { get; set; } = 0;


        public PricingScaleItem()
        {
        }

        public bool HasDiscount
        {
            get
            {
                return Discount > 0;
            }

        }

        public bool HasFreeGoods
        {
            get
            {
                return FreeGoods > 0;
            }

        }

        public bool HasUnitPrice
        {
            get
            {
                return UnitPrice > 0;
            }

        }

        public bool HasQuantityBoundaries
        {
            get
            {
                return MinQuantity > 0 || MaxQuantity > 0;
            }

        }

        public bool HasPriceBoundaries
        {
            get
            {
                return MinPrice > 0 || MaxPrice > 0;
            }

        }

        public decimal UnitPrice
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_UNITPRICE))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_UNITPRICE];
                    decimal decData;
                    decimal.TryParse(strData, out decData);
                    if (decData > 0)
                    {
                        if (ExchangeRate == 0)
                        {
                            ExchangeRate = 1;
                        }
                            ExchangeRate = 1;
                        return decData * ExchangeRate;
                    }
                    return decData;
                }
                else
                {
                    return 0;
                }
            }

        }

        public decimal Discount
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_DISCOUNT))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_DISCOUNT];
                    decimal decData;
                    decimal.TryParse(strData, out decData);
                    return decData;
                }
                else
                {
                    return 0;
                }
            }

        }

        public int MinQuantity
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_MINQUANTITY))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_MINQUANTITY];
                    int intData;
                    int.TryParse(strData, out intData);
                    return intData;
                }
                else
                {
                    return 0;
                }
            }

        }

        public int MaxQuantity
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_MAXQUANTITY))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_MAXQUANTITY];
                    int intData;
                    int.TryParse(strData, out intData);
                    return intData;
                }
                else
                {
                    return 0;
                }
            }

        }

        public int FreeGoods
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_FREEGOODS))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_FREEGOODS];
                    int intData;
                    int.TryParse(strData, out intData);
                    return intData;
                }
                else
                {
                    return 0;
                }
            }

        }

        public decimal MinPrice
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_MINPRICE))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_MINPRICE];
                    decimal decData;
                    decimal.TryParse(strData, out decData);
                    return decData;
                }
                else
                {
                    return 0;
                }
            }

        }

        public decimal MaxPrice
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(PricingItem.FUNCTIONNAME_MAXPRICE))
                {
                    var strData = FunctionKeyPairs[PricingItem.FUNCTIONNAME_MAXPRICE];
                    decimal decData;
                    decimal.TryParse(strData, out decData);
                    return decData;
                }
                else
                {
                    return 0;
                }
            }

        }
    }
}
