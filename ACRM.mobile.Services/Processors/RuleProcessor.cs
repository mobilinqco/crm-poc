using System;
using System.Globalization;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services.Processors
{
    public class RuleProcessor: IRuleProcessor
    {
        public RuleProcessor()
        {
        }

        public bool IsExpandRuleTrue(ExpandRule rule, FieldInfo fieldInfo, string inputValue)
        {
            if(string.IsNullOrEmpty(rule.Operator))
            {
                return false;
            }

            if(fieldInfo.IsNumeric || fieldInfo.IsCatalog)
            {
                return IsNumericRuleTrue(rule, inputValue);
            }
            if(fieldInfo.IsBoolean)
            {
                return IsBooleanRuleTrue(rule, inputValue);
            }

            if(fieldInfo.IsDate)
            {
                return IsDateRuleTrue(rule, inputValue);
            }

            return IsStringRuleTrue(rule, inputValue);
        }

        private bool TryConvertToDateTime(string value, out DateTime outValue)
        {
            DateTime val;

            if (DateTime.TryParseExact(value, CrmConstants.DbFieldDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out val))
            {
                outValue = val;
                return true;
            }

            if (DateTime.TryParse(value, out val))
            {
                outValue = val;
                return true;
            }

            outValue = DateTime.MinValue;
            return false;
        }

        private bool IsDateRuleTrue(ExpandRule rule, string inputValue)
        {
            DateTime ruleValue;
            DateTime inValue;

            if (string.IsNullOrEmpty(rule.Operator))
            {
                return false;
            }

            if (rule.Operator.Equals("><"))
            {
                if (TryConvertToDateTime(inputValue, out inValue) && !string.IsNullOrEmpty(rule.Value))
                {
                    string[] ruleValues = rule.Value.Split(',');
                    if (ruleValues.Length == 2)
                    {
                        DateTime lVal, rVal;
                        if (TryConvertToDateTime(ruleValues[0], out lVal) && TryConvertToDateTime(ruleValues[1], out rVal))
                        {
                            if (inValue >= lVal && inValue <= rVal)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            if (TryConvertToDateTime(rule.Value, out ruleValue) && TryConvertToDateTime(inputValue, out inValue))
            {
                int result = ruleValue.CompareTo(inValue);
                return EvaluateResult(rule, result);
            }

            return false;
        }

        private bool IsNumericRuleTrue(ExpandRule rule, string inputValue)
        {
            decimal ruleValue;
            decimal inValue;

            if (string.IsNullOrEmpty(rule.Operator))
            {
                return false;
            }

            if (rule.Operator.Equals("><"))
            {
                if(decimal.TryParse(inputValue, out inValue) && !string.IsNullOrEmpty(rule.Value))
                {
                    string[] ruleValues = rule.Value.Split(',');
                    if(ruleValues.Length == 2)
                    {
                        decimal lVal, rVal;
                        if (decimal.TryParse(ruleValues[0], out lVal) && decimal.TryParse(ruleValues[1], out rVal))
                        {
                            if(inValue >= lVal && inValue <= rVal)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            
            if (decimal.TryParse(rule.Value, out ruleValue) && decimal.TryParse(inputValue, out inValue))
            {
                int result = ruleValue.CompareTo(inValue);
                return EvaluateResult(rule, result);
            }

            return false;
        }

        private static bool EvaluateResult(ExpandRule rule, int result)
        {
            switch (rule.Operator)
            {
                case "<>":
                    return result != 0;
                case ">":
                    return result < 0;
                case ">=":
                    return result <= 0;
                case "<":
                    return result > 0;
                case "<=":
                    return result >= 0;
                default:
                    return result == 0;
            }
        }

        private bool IsBooleanRuleTrue(ExpandRule rule, string inputValue)
        {
            int result = rule.Value.CrmBool().CompareTo(inputValue.CrmBool());
            return EvaluateResult(rule, result);
        }

        private bool IsStringRuleTrue(ExpandRule rule, string inputValue) {
            if (rule.Operator.Equals("><"))
            {
                return false;
            }

            if (string.IsNullOrEmpty(rule.Value))
            {
                if (string.IsNullOrEmpty(inputValue))
                {
                    if(rule.Operator.Equals("=") || rule.Operator.Equals("LIKE"))
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (rule.Operator.Equals("<>") || rule.Operator.Equals("<") || rule.Operator.Equals("<="))
                    {
                        return true;
                    }
                    return false;
                }
            }

            if(rule.Operator.Equals("LIKE"))
            {
                if(string.IsNullOrEmpty(inputValue))
                {
                    return false;
                }

                return inputValue.Like(rule.Value);
            }

            int result = rule.Value.CompareTo(inputValue);
            return EvaluateResult(rule, result);
        }
    }
}
