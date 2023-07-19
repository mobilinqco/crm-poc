using System;
using System.Collections.Generic;
using System.Linq;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class PricingItem
    {
        public const string FUNCTIONNAME_UNITPRICE = @"UnitPrice";
        public const string FUNCTIONNAME_DISCOUNT = @"Discount";
        public const string FUNCTIONNAME_MINQUANTITY = @"MinQuantity";
        public const string FUNCTIONNAME_MAXQUANTITY = @"MaxQuantity";
        public const string FUNCTIONNAME_FREEGOODS = @"FreeGoods";
        public const string FUNCTIONNAME_MINPRICE = @"MinPrice";
        public const string FUNCTIONNAME_MAXPRICE = @"MaxPrice";
        public const string FUNCTIONNAME_CURRENCY = @"Currency";
        public const string FUNCTIONNAME_ITEMNUMBER = @"ItemNumber";
        public const string FUNCTIONNAME_ITEMNUMBER_ALT = @"CopyItemNumber";

        public PricingItemType ItemType = PricingItemType.PRICELIST;
        public string RecordIdentification { get; set; }
        public string InfoAreaId { get; set; }
        public string ItemNumber { get; set; }
        public Dictionary<string, string> FunctionKeyPairs { get; set; }
        public List<PricingScaleItem> ScaleItems { get; set; }
        public string BundileRecordIdentification { get; set; }


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
                return ScaleItems!=null && ScaleItems.Any(x => x.HasQuantityBoundaries);
            }

        }

        public bool HasPriceBoundaries
        {
            get
            {
                return ScaleItems != null && ScaleItems.Any(x => x.HasPriceBoundaries);
            }

        }

        public decimal UnitPrice
        {
            get
            {
                if(FunctionKeyPairs?.Count>0 && FunctionKeyPairs.ContainsKey(FUNCTIONNAME_UNITPRICE))
                {
                    var strData = FunctionKeyPairs[FUNCTIONNAME_UNITPRICE];
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

        public decimal Discount
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(FUNCTIONNAME_DISCOUNT))
                {
                    var strData = FunctionKeyPairs[FUNCTIONNAME_DISCOUNT];
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

        public int FreeGoods
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(FUNCTIONNAME_FREEGOODS))
                {
                    var strData = FunctionKeyPairs[FUNCTIONNAME_FREEGOODS];
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

        public string Currency
        {
            get
            {
                if (FunctionKeyPairs?.Count > 0 && FunctionKeyPairs.ContainsKey(FUNCTIONNAME_CURRENCY))
                {
                    return FunctionKeyPairs[FUNCTIONNAME_CURRENCY];
                }
                else
                {
                    return string.Empty;
                }
            }

        }

        public PricingItem()
        {
        }
    }
}
