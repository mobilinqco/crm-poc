using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.FormatUtils
{
    public static class UtilityExtensions
    {
        public static string GetDomain(this string identity)
        {
            int stop = identity.IndexOf("\\");
            return (stop > -1) ? identity.Substring(0, stop) : string.Empty;
        }

        public static string GetLogin(this string identity)
        {
            int stop = identity.IndexOf("\\");
            return (stop > -1) ? identity.Substring(stop + 1, identity.Length - stop - 1) : string.Empty;
        }
        /// <summary>
        /// Reps the identifier.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int RepId(this string source)
        {
            int intValue;
            if (!string.IsNullOrEmpty(source) && source.Length == 9 && source.StartsWith("U"))
            {
                var repString = $"10{source.Substring(1)}";
                return int.TryParse(repString, out intValue) ? intValue : 0;
            }

            return int.TryParse(source, out intValue) ? intValue : 0;
        }

        /// <summary>
        /// Reps the identifier string.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string RepIdString(this string source)
        {
            if (!string.IsNullOrEmpty(source) && source.Length < 9)
            {
                return NineDigitStringFromRep(source.RepId());
            }

            return source;
        }

        /// <summary>
        /// Nines the digit string from rep.
        /// </summary>
        /// <param name="rep">
        /// The rep.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string NineDigitStringFromRep(int rep)
        {
            if (rep == 0)
            {
                return string.Empty;
            }

            return rep >= 1000000000 ? $"U{rep - 1000000000:D8}" : $"{rep:D9}";
        }

        /// <summary>
        /// Formatteds the rep identifier.
        /// </summary>
        /// <param name="repId">The rep identifier.</param>
        /// <returns></returns>
        public static string FormattedRepId(this string repId)
        {
            repId = repId.Trim();
            if (string.IsNullOrEmpty(repId))
            {
                return repId;
            }

            if (repId.Length == 9 && repId.StartsWith("U"))
            {
                return $"10{repId.Substring(1)}";
            }

            switch (repId.Length)
            {
                case 1:
                    return $"00000000{repId}";
                case 2:
                    return $"0000000{repId}";
                case 3:
                    return $"000000{repId}";
                case 4:
                    return $"00000{repId}";
                case 5:
                    return $"0000{repId}";
                case 6:
                    return $"000{repId}";
                case 7:
                    return $"00{repId}";
                case 8:
                    return $"0{repId}";
                default:
                    return repId;
            }
        }

        public static bool HasChanges(this List<PanelData> inputPanels, bool skipHidden = false)
        {
            foreach (PanelData panel in inputPanels)
            {

                if (panel.HasChanges(skipHidden))
                {
                    return true;
                }

            }
            return false;
        }

        public static bool HasEditPanelChild(this List<PanelData> inputPanels)
        {
            foreach (var panels in inputPanels)
            {
                if (panels.Type == PanelType.EditPanelChildren)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasData(this List<PanelData> inputPanels, bool skipHidden = false)
        {
            foreach (PanelData panel in inputPanels)
            {

                if (panel.HasData(skipHidden))
                {
                    return true;
                }

            }
            return false;
        }

        public static string GetFunctionRawValue(this List<PanelData> inputPanels, string function)
        {
            foreach (PanelData panel in inputPanels)
            {

                var fieldIndex = panel.Fields.FindIndex(f => f.Config.FieldConfig.Function.Equals(function, StringComparison.InvariantCultureIgnoreCase));
                if (fieldIndex >= 0)
                {
                    var field = panel.Fields[fieldIndex];
                    var rawValue = panel.Fields[fieldIndex].EditData.SelectedRawValue();

                    if (field.Config.PresentationFieldAttributes.FieldInfo.IsBoolean)
                    {
                        rawValue = rawValue.Equals("1") ? "true" : "false";
                    }
                    else if (field.Config.PresentationFieldAttributes.FieldInfo.IsDate || field.Config.PresentationFieldAttributes.FieldInfo.IsTime)
                    {
                        rawValue = rawValue.Replace("-", "").Replace(":", "");
                    }

                    return rawValue;
                }


            }
            return string.Empty;
        }

        public static string GetFunctionFromFieldId(this List<PanelData> inputPanels, string filedID)
        {
            if (string.IsNullOrWhiteSpace(filedID))
            {
                return string.Empty;
            }

            var strParts = filedID.Split('_');

            int fieldId = 0;
            string infoAreaId = strParts[0];
            if (strParts.Length != 2 || !int.TryParse(strParts[1], out fieldId))
            {
                return string.Empty;
            }

            foreach (PanelData panel in inputPanels)
            {
                var fieldIndex = panel.Fields.FindIndex(f => f.Config.FieldConfig.FieldId == fieldId && f.Config.FieldConfig.InfoAreaId == infoAreaId);
                if (fieldIndex >= 0)
                {
                    var field = panel.Fields[fieldIndex];
                    if (!string.IsNullOrWhiteSpace(field.Config.FieldConfig.Function))
                    {
                        return field.Config.FieldConfig.Function;
                    }
                }

            }
            return string.Empty;
        }

        public static bool HasChanges(this PanelData panel, bool skipHidden = false)
        {
            if (panel != null)
            {
                foreach (ListDisplayField field in panel.Fields)
                {
                    if (skipHidden && field.Config.PresentationFieldAttributes.Hide)
                    {
                        continue;
                    }

                    if (field.Config.PresentationFieldAttributes.Dontsave)
                    {
                        continue;
                    }

                    else if (field.EditData.HasDataChanged)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasData(this PanelData panel, bool skipHidden = false)
        {
            if (panel != null)
            {
                foreach (ListDisplayField field in panel.Fields)
                {
                    if (skipHidden && field.Config.PresentationFieldAttributes.Hide)
                    {
                        continue;
                    }

                    if (field.Config.PresentationFieldAttributes.Dontsave)
                    {
                        continue;
                    }

                    else if (field.EditData.HasData)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string CleanRecordId(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var parts = value.Split('.');
                string recordId;
                if (parts.Length > 1)
                {
                    recordId = parts[1];
                }
                else
                {
                    recordId = parts[0];
                }
                return recordId;
            }
            else
            {
                return value;
            }
        }

        public static bool IsRecordIdFormated(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var parts = value.Split('.');
                if (parts.Length > 1)
                {
                    return true;
                }
                
                return false;
            }
            else
            {
                return false;
            }
        }

        public static string UnformatedRecordId(this string recordId)
        {
            if (recordId.IsRecordIdFormated())
            {
                var parts = recordId.Split('.');
                if (parts.Length > 1)
                {
                    return parts[1];
                }
            }

            return recordId;
        }

        public static string FormatedRecordId(this string recordId, string infoArea)
        {
            if (!string.IsNullOrWhiteSpace(recordId))
            {
                if (recordId.IsRecordIdFormated())
                {
                    return recordId;
                }
                else
                {
                    return $"{infoArea}.{recordId}";
                }
            }
            else
            {
                return recordId;
            }
        }

        public static string UTF8Encoding(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                byte[] bytes = Encoding.Default.GetBytes(value);
                var result = Encoding.UTF8.GetString(bytes);
                return result;
            }
            else
            {
                return value;
            }
        }

        public static string ExtKeyValue(this ListDisplayField field)
        {
            string result = string.Empty;

            if (field != null
                && field.Config.PresentationFieldAttributes.IsSelectableInput()
                && field.Config?.AllowedValues?.Count > 0
                && field.EditData?.DefaultSelectedValue != null)
            {
                var item = field.Config?.AllowedValues.Where(a => a.RecordId == field.EditData?.DefaultSelectedValue.RecordId).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(item.ExtKey))
                {
                    return item.ExtKey;
                }
            }

            return result;

        }

        public static bool IsListing(this string filterName)
        {
            if (string.IsNullOrEmpty(filterName))
            {
                return false;
            }
            return filterName.EndsWith(":Listing", StringComparison.InvariantCultureIgnoreCase);
        }

        public static Dictionary<string, object> GetDictionaryFromJson(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static RequestMode GetRequestMode(this string input, RequestMode defaultMode = RequestMode.Fastest)
        {
            if (!string.IsNullOrEmpty(input))
            {
                RequestMode reqMode;
                if (Enum.TryParse(input, out reqMode))
                {
                    return reqMode;
                }

            }
            return defaultMode;
        }
    }
}
