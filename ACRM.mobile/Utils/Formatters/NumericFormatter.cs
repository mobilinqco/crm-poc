using System;
using System.Globalization;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Utils.Formatters
{
    public class NumericFormatter
    {
        public static string Convert(string rawValue,
            PresentationFieldAttributes pfa,
            bool isReportField = false)
        {
            FieldInfo fieldInfo = pfa.FieldInfo;

            var convertedValue = string.Empty;

            if (!string.IsNullOrWhiteSpace(fieldInfo.RepMode))
            {
                return rawValue;
            }

            if (double.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double numericValue))
            {
                if (IsPercentageField(fieldInfo, pfa))
                {
                    numericValue *= 100;
                }

                char fieldType = fieldInfo.FieldType;
                if (pfa.RenderHooks().FieldType() != ' ')
                {
                    fieldType = pfa.RenderHooks().FieldType();
                }

                switch (fieldType)
                {
                    case 'L':
                    case 'S':
                        {
                            if (Math.Abs(numericValue) < double.Epsilon)
                            {
                                if (fieldInfo.ShowZero)
                                {
                                    convertedValue = "0";
                                }
                                else
                                {
                                    convertedValue = string.Empty;
                                }
                            }
                            else
                            {
                                convertedValue = numericValue.ToString();
                            }

                            break;
                        }

                    case 'F':
                        {
                            string convertFormat = "N";
                            int renderHookDecimalDigits = pfa.RenderHooks().DecimalDigits();

                            if (pfa.RenderHooks().DecimalDigits() > -1)
                            {
                                convertFormat = $"N{renderHookDecimalDigits}";
                            }
                            else
                            {
                                if (fieldInfo.OneDecimalDigit)
                                {
                                    convertFormat = "N1";
                                }
                                else if (fieldInfo.NoDecimalDigits)
                                {
                                    convertFormat = "N0";
                                }
                                else if (fieldInfo.ThreeDecimalDigits)
                                {
                                    convertFormat = "N3";
                                }
                                else if (fieldInfo.FourDecimalDigits)
                                {
                                    convertFormat = "N4";
                                }
                            }

                            if (!fieldInfo.ShowZero && numericValue == 0)
                            {
                                convertedValue = string.Empty;
                            }
                            else
                            {
                                convertedValue = numericValue.ToString(convertFormat, CultureInfo.CurrentUICulture);
                            }

                            if (!isReportField && fieldInfo.IsAmount)
                            {
                                convertedValue = numericValue.ToString("N,-", CultureInfo.CurrentUICulture);
                            }

                            break;
                        }
                }

                if (!pfa.RenderHooks().GroupingSeparator())
                {
                    convertedValue = convertedValue.Replace(CultureInfo.CurrentUICulture.NumberFormat.NumberGroupSeparator, string.Empty);
                }

                if (IsPercentageField(fieldInfo, pfa) && !string.IsNullOrWhiteSpace(convertedValue))
                {
                    if (!isReportField)
                    {
                        convertedValue = $"{convertedValue}%";
                    }
                }
            }

            return convertedValue;
        }

        private static bool IsPercentageField(FieldInfo fieldInfo, PresentationFieldAttributes pfa)
        {
            return fieldInfo.IsPercent || pfa.RenderHooks().PercentField();
        }
    }
}
