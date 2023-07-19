using System;
using System.Collections.Generic;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Formatters;

namespace ACRM.mobile.Localization
{
    public class LocalizationController: ILocalizationController
    {
        private IConfigurationUnitOfWork _configurationUnitOfWork;
        private Dictionary<string, Textgroup> _textGroups;

        public LocalizationController()
        {
            _textGroups = new Dictionary<string, Textgroup>();
        }

        public void AttachConfiguration() { 
            _configurationUnitOfWork = AppContainer.Resolve<IConfigurationUnitOfWork>();
            _textGroups = new Dictionary<string, Textgroup>();
        }

        public void ResetConfiguration()
        {
            _configurationUnitOfWork = null;
            _textGroups = new Dictionary<string, Textgroup>();
        }

        private string GetAppDefaultString(string key, string defaultValue = "<Not localized>")
        {
            return AppResources.ResourceManager.GetString(key) ?? defaultValue;
        }

        public string GetString(string textGroupKey, int textIndex, string defaultString = "<Not localized>")
        {
            if (_configurationUnitOfWork == null)
            {
                return GetAppDefaultString($"{textGroupKey}_{textIndex}", defaultString);
            }

            Textgroup tg;
            if (_textGroups.ContainsKey(textGroupKey))
            {
                tg = _textGroups[textGroupKey];
            }
            else 
            {
                tg = _configurationUnitOfWork.GenericRepository<Textgroup>().Get(textGroupKey);
                _textGroups.Add(textGroupKey, tg);
            }
            
            if (tg == null)
            {
                return GetAppDefaultString($"{textGroupKey}_{textIndex}", defaultString);
            }

            return tg.GetText(textIndex) ?? GetAppDefaultString($"{textGroupKey}_{textIndex}", defaultString);

        }

        public string GetString(string textGroupKey, int textIndex)
        {
            return GetString(textGroupKey, textIndex, "<Not localized>");
        }

        public string GetBoolString(bool isTrue)
        {
            if (isTrue)
            {
                return GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicYes, "Yes");
            }

            return GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicNo, "No");
        }

        public string GetFormatedString(string textGroupKey, int textIndex, params string[] values)
        {
            return string.Format(GetString(textGroupKey, textIndex), values);
        }

        public string GetLocalizedValue(ListDisplayField ldf)
        {
            if (ldf.Data.ColspanData == null || ldf.Data.ColspanData.Count == 0)
            {
                string fieldData = ldf.Data.StringData;
                if (fieldData != null)
                {
                    if (ldf.Config.PresentationFieldAttributes.IsNumeric)
                    {
                        fieldData = NumericFormatter.Convert(ldf.Data.StringData, ldf.Config.PresentationFieldAttributes);
                    }
                    else if (ldf.Config.PresentationFieldAttributes.IsBoolean)
                    {
                        if (ldf.Data.StringData != null)
                        {
                            if (ldf.Data.StringData.ToLower().Equals("true") || ldf.Data.StringData.ToLower().Equals("1"))
                            {
                                fieldData = GetBoolString(true);
                            }
                            else if (ldf.Data.StringData.ToLower().Equals("false"))
                            {
                                fieldData = GetBoolString(false);
                            }
                            else if (ldf.Data.StringData.ToLower().Equals("0"))
                            {
                                if (ldf.Config.PresentationFieldAttributes.FieldInfo.ShowZero)
                                {
                                    fieldData = GetBoolString(false);
                                }
                                else
                                {
                                    fieldData = string.Empty;
                                }
                            }
                            else
                            {
                                fieldData = ldf.Data.StringData;
                            }
                        }
                        else
                        {
                            fieldData = string.Empty;
                        }
                    }
                    else if (ldf.Config.PresentationFieldAttributes.IsDate || ldf.Config.PresentationFieldAttributes.IsTime)
                    {
                        fieldData = DateTimeFormatter.FormatedDateFromDbString(ldf.Data.StringData, ldf.Config.PresentationFieldAttributes);
                    }
                }

                return fieldData;
            }
            else
            {
                return GetLocalizedColspan(ldf.Config.PresentationFieldAttributes, ldf.Data.ColspanData);
            }
        }

        public string GetLocalizedColspan(PresentationFieldAttributes pfa, List<ListDisplayField> values)
        {
            var count = values.Count;

            if (count == 0)
            {
                return string.Empty;
            }

            if (count == 1)
            {
                return GetLocalizedValue(values[0]);
            }

            string combineString = pfa.CombineString;
            if (combineString == null)
            {
                if (pfa.LocalizationTextGroup != null)
                {
                    combineString = GetString(pfa.LocalizationTextGroup, int.Parse(pfa.LocalizationTextId));
                }
            }

            if (pfa.CombineWithIndices)
            {
                for (var i = count; i > 0; i--)
                {
                    string val = GetLocalizedValue(values[i - 1]);
                    combineString = this.StringFrom(combineString, i - 1, val);
                }

                return combineString;
            }

            int nextField, lineCount = pfa.LineCount;
            string result = null;
            nextField = 0;
            for (var j = 0; j < lineCount; j++)
            {
                string lineResult = null;
                var fieldCountInLine = this.FieldCountInLine(pfa, j);
                for (var i = 0; i < fieldCountInLine; i++)
                {
                    if (nextField >= count)
                    {
                        continue;
                    }

                    string val = GetLocalizedValue(values[nextField++]);
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        lineResult = !string.IsNullOrWhiteSpace(lineResult)
                                             ? $"{lineResult}{combineString}{val}"
                                             : val;
                    }
                }

                if (!string.IsNullOrWhiteSpace(lineResult))
                {
                    result = !string.IsNullOrWhiteSpace(result)
                                 ? $"{result}{Environment.NewLine}{lineResult}"
                                 : lineResult;
                }
            }

            return result;
        }

        private string StringFrom(string source, int nr, string replaceString)
        {
            var pattern = $"{{{nr + 1}}}";
            if (source.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) > -1)
            {
                return source.Replace(pattern, replaceString);
            }

            pattern = $"%%{nr + 1}";
            return source.Replace(pattern, replaceString);
        }

        public int FieldCountInLine(PresentationFieldAttributes pfa, int lineIndex)
        {
            return pfa.FieldLines == null
                       ? (lineIndex != 0 ? 0 : pfa.FieldCount)
                       : (pfa.FieldLines.Count <= lineIndex ? 0 : pfa.FieldLines[lineIndex]);
        }
    }
}
