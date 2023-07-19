using System;
using System.Collections.Generic;
using System.Text;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Extensions
{
    public static class PresentationFieldAttributesExtensions
    {
        public static string GetFormatedString(this PresentationFieldAttributes colspanPfa, List<ListDisplayField> columns)
        {
            StringBuilder builder = new StringBuilder();

            if (colspanPfa != null && columns != null && columns.Count > 0)
            {
                foreach (var col in columns)
                {
                    if (IsValidData(col))
                    {
                        if (!string.IsNullOrWhiteSpace(builder.ToString()))
                        {
                            builder.Append($"{colspanPfa.CombineString}{col.Data.StringData}");

                        }
                        else
                        {
                            builder.Append($"{col.Data.StringData}");
                        }
                    }
                }
            }

            return builder.ToString();
        }

        private static bool IsValidData(ListDisplayField col)
        {
            if(string.IsNullOrWhiteSpace(col.Data.StringData))
            {
                return false;
            }

            if(col.Config.PresentationFieldAttributes.IsNumeric && col.Data.StringData.Equals("0", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
            
        }
    }
}
