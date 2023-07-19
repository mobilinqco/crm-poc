using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services.Extensions
{
    public static class CrmDataRow
    {
        public static string GetEscapedColumnValue(this DataRow row, string columnName, string defaultValue = " ")
        {
            string value = defaultValue;
            if (row != null && row.Table.Columns.Contains(columnName) && !row.IsNull(columnName))
            {
                value = row[columnName].ToString();
            }

            try
            {
                value = Uri.EscapeDataString(value);
            }
            catch
            {
                return "";
            }

            return value;
        }

        public static string GetColumnValue(this DataRow row, string columnName, string defaultValue = " ")
        {
            if (row != null && row.Table.Columns.Contains(columnName) && !row.IsNull(columnName))
            {
                if (row.Table.Columns[columnName].DataType == typeof(double))
                {
                    return ((double)row[columnName]).ToString(CultureInfo.InvariantCulture);
                }
                else if (row.Table.Columns[columnName].DataType == typeof(int))
                {
                    return ((int)row[columnName]).ToString(CultureInfo.InvariantCulture);
                }

                return row[columnName].ToString();
            }

            return defaultValue;
        }

        public static int GetIntColumnValue(this DataRow row, string columnName, int defaultValue = -1)
        {
            if (row != null && row.Table.Columns.Contains(columnName) && !row.IsNull(columnName))
            {
                if (row.Table.Columns[columnName].DataType == typeof(double))
                {
                    return Convert.ToInt32((double)row[columnName]);
                }
                else if (row.Table.Columns[columnName].DataType == typeof(int))
                {
                    return ((int)row[columnName]);
                }

                if (Int32.TryParse(row[columnName].ToString(), out int val))
                {
                    return val;
                }
            }

            return defaultValue;
        }

        public static bool HasColumn(this DataTable table, string columnName)
        {
            if (table != null && table.Columns.Contains(columnName))
            {
                return true;
            }

            return false;
        }

        public static string GetRawInfoArea(this DataRow row)
        {
            if (row != null && row.Table.Columns.Contains("title") && !row.IsNull("title"))
            {
                return row["title"].ToString();
            }

            return string.Empty;
        }

        public static async Task<T> GetColumnValue<T>(this DataRow row, string functionName, List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, T defaultValue ,CancellationToken cancellationToken)
        {
            var fieldControl = fieldDefinitions.Where(a => a.Function.Equals(functionName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            string strFielddata = string.Empty;
            object fieldData = defaultValue;

            if (fieldControl != null)
            {
                strFielddata = await fieldGroupComponent.ExtractFieldValue(fieldControl, row, cancellationToken);


                switch (true)
                {
                    case true when typeof(T) == typeof(int):
                        if (int.TryParse(strFielddata, out int intFielddata))
                        {
                            fieldData = intFielddata;
                        }
                        break;
                    case true when typeof(T) == typeof(long):
                        if (long.TryParse(strFielddata, out long longFielddata))
                        {
                            fieldData = longFielddata;
                        }
                        break;
                    case true when typeof(T) == typeof(decimal):
                        if (decimal.TryParse(strFielddata, out decimal decimalFielddata))
                        {
                            fieldData = decimalFielddata;
                        }
                        break;
                    case true when typeof(T) == typeof(double):
                        if (double.TryParse(strFielddata, NumberStyles.Number, CultureInfo.InvariantCulture, out double doubleFielddata))
                        {
                            fieldData = doubleFielddata;
                        }
                        break;
                    case true when typeof(T) == typeof(bool):
                        if (bool.TryParse(strFielddata, out bool boolFielddata))
                        {
                            fieldData = boolFielddata;
                        }
                        break;
                    default:
                        fieldData = strFielddata;
                        break;
                }
            }
            return (T)Convert.ChangeType(fieldData, typeof(T));

        }
        public static async Task<string> GetColumnRawValue(this DataRow row, string functionName, List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, CancellationToken cancellationToken, string defaultValue = null)
        {
            var fieldControl = fieldDefinitions.Where(a => a.Function.Equals(functionName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            string strFieldData = string.Empty;

            if (fieldControl != null)
            {
                strFieldData = await fieldGroupComponent.ExtractFieldRawValue(fieldControl, row, cancellationToken);
            }

            if (string.IsNullOrEmpty(strFieldData))
            {
                strFieldData = defaultValue;
            }

            return strFieldData;

        }

    }
}
