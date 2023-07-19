using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application
{
    public class PresentationFieldAttributes : ICloneable
    {
        // Boolean Field attributes. If present in the set then it is considered to be true
        public bool Bold => AttributeIsSet((int)FieldAttributeType.Bold);
        public bool Italic => AttributeIsSet((int)FieldAttributeType.Italic);
        public bool Hyperlink => AttributeIsSet((int)FieldAttributeType.Hyperlink);
        public bool Email => AttributeIsSet((int)FieldAttributeType.Email);
        public bool DontcacheOffline => AttributeIsSet((int)FieldAttributeType.DontCacheOffline);
        public bool Dontsave => AttributeIsSet((int)FieldAttributeType.DontSave);
        public bool Hide => AttributeIsSet((int)FieldAttributeType.Hide);
        public bool Password => AttributeIsSet((int)FieldAttributeType.Password);

        public bool LabelBold => AttributeIsSet((int)FieldAttributeType.LabelBold);
        public bool LabelItalic => AttributeIsSet((int)FieldAttributeType.LabelItalic);
        public bool Phone => AttributeIsSet((int)FieldAttributeType.Phone);
        public bool NoLabel => AttributeIsSet((int)FieldAttributeType.NoLabel);
        public bool MultiLine => AttributeIsSet((int)FieldAttributeType.MultiLine);
        public bool NoMultiLine => AttributeIsSet((int)FieldAttributeType.NoMultiLine);
        public bool Image => AttributeIsSet((int)FieldAttributeType.Image);
        public bool PlaceHolder => AttributeIsSet((int)FieldAttributeType.PlaceHolder);
        public bool Empty => AttributeIsSet((int)FieldAttributeType.Empty);
        public bool HasColspan => IsStringAttributeNotEmpty((int)FieldAttributeType.ColSpan);
        public bool IsNumeric => _fieldInfo.IsNumeric;
        public bool IsBoolean => _fieldInfo.IsBoolean;
        public bool IsDate => _fieldInfo.IsDate;
        public bool IsTime => _fieldInfo.IsTime;

        public int MultiLineHeight
        {
            get
            {
                var mlc = int.Parse(ValueForAttribute((int)FieldAttributeType.MultiLine));
                return mlc > 1 ? mlc : 5;
            }
        }

        public bool Must {
            get
            {
                if(_fieldInfo != null
                    && !string.IsNullOrWhiteSpace(_fieldInfo.TableInfoInfoAreaId)
                    && _fieldInfo.TableInfoInfoAreaId.Equals(_fieldControlInfoAreaId)
                    && (FieldInfo.Rights & 0x40000000) != 0)
                {
                    return true;
                }

                return AttributeIsSet((int)FieldAttributeType.Must);
            }
            
        }

        public bool ReadOnly
        {
            get
            {
                if (_fieldInfo != null
                    && (FieldInfo.Rights & 0x20) != 0)
                {
                    return true;
                }

                return AttributeIsSet((int)FieldAttributeType.ReadOnly);
            }

        }

        public bool Locked
        {
            get
            {
                if (_fieldInfo != null && (FieldInfo.Rights & 0x00000001) == 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool LockedOnNew
        {
            get
            {
                if (Locked || (_fieldInfo != null && (FieldInfo.Rights & 0x00000002) == 0))
                {
                    return true;
                }
                return false;
            }
        }

        public bool LockedOnUpdate
        {
            get
            {
                if (Locked || (_fieldInfo != null && (FieldInfo.Rights & 0x00000004) == 0))
                {
                    return true;
                }
                return false;
            }
        }

        public string FieldStyle => ValueForAttribute((int)FieldAttributeType.FieldStyle);
        public string LableColor => ValueForAttribute((int)FieldAttributeType.LabelColor);
        public string Color => ValueForAttribute((int)FieldAttributeType.Color);

        // Colspan attributes: FieldCount, LineCounts, SeparatorString, LocalizationTextGroup, LocalizationTextId, FieldLines
        public string RawColspanValue => ValueForAttribute((int)FieldAttributeType.ColSpan);
        public int FieldCount { get; private set; }
        public int LineCount { get; private set; }
        public bool NoPlaceHoldersInCombineString { get; private set; }
        public bool CombineWithIndices { get; private set; }
        public string CombineString { get; private set; }
        public string LocalizationTextGroup { get; private set; }
        public string LocalizationTextId { get; private set; }
        public List<int> FieldLines { get; private set; }
        public string ServerTimezone { get; private set; } // Used for Date and Time to convert to proper values
        public string RelatedTimeValue { get; set; }

        // Label Attributes
        private string ExplicitLabel;
        private string FieldInfoLabel;

        private readonly List<FieldAttribute> _fieldAttributes;
        private Dictionary<string, FieldAttribute> _attributes = new Dictionary<string, FieldAttribute>();
        private readonly FieldInfo _fieldInfo;
        public FieldInfo FieldInfo {
            get => _fieldInfo;
        }
        private readonly string _fieldControlInfoAreaId;

        private EditModes _editMode = EditModes.DetailsOrAll;

        public PresentationFieldAttributes(FieldControlField fieldControlField, FieldInfo fieldInfo, string serverTimezone, EditModes editMode, string combineString = null)
        {
            _editMode = editMode;
            _fieldControlInfoAreaId = fieldControlField.TabConfig?.FieldControl?.InfoAreaId;
            _fieldAttributes = fieldControlField.Attributes;
            if (_fieldAttributes != null)
            {
                foreach (FieldAttribute fd in _fieldAttributes)
                {
                    string key = $"{fd.AttributeType}_{fd.EditMode}";
                    if (!_attributes.ContainsKey(key))
                    {
                        _attributes.Add(key, fd);
                    }
                }
            }

            ExplicitLabel = fieldControlField.ExplicitLabel;
            FieldInfoLabel = fieldInfo.Name;
            _fieldInfo = fieldInfo;
            ServerTimezone = serverTimezone;
            UpdateCombineString(combineString);
        }

        public void UpdateCombineString(string combineString)
        {
            // the combineString can be used as constructor parameter in case
            // we want to use the localizedcolspan for table captions

            if (combineString != null)
            {
                CombineString = combineString;
                CombineWithIndices = true;
            }
        }

        private bool IsStringAttributeNotEmpty(int attrid)
        {
            string key = AttributeSetKey(attrid);
            if (!string.IsNullOrWhiteSpace(key))
            {
                return !string.IsNullOrEmpty(_attributes[key].Value);
            }
            return false;
        }


        private string AttributeSetKey(int attrid)
        {
            string key = $"{attrid}_{(int)_editMode}";
            if (_attributes.ContainsKey($"{attrid}_{(int)_editMode}"))
            {
                return $"{attrid}_{(int)_editMode}";
            }
            else
            {
                if (_editMode == EditModes.Update)
                {
                    if (_attributes.ContainsKey($"{attrid}_{(int)EditModes.DetailsOrAll}"))
                    {
                        return $"{attrid}_{(int)EditModes.DetailsOrAll}";
                    }
                }
                else
                {
                    if (_attributes.ContainsKey($"{attrid}_{(int)EditModes.DetailsOrAll}"))
                    {
                        return $"{attrid}_{(int)EditModes.DetailsOrAll}";
                    }

                    if(_attributes.ContainsKey($"{attrid}_{(int)EditModes.MiniDetails}"))
                    {
                        return $"{attrid}_{(int)EditModes.MiniDetails}";
                    }

                    if(_attributes.ContainsKey($"{attrid}_{(int)EditModes.List}"))
                    {
                        return $"{attrid}_{(int)EditModes.List}";
                    }
                }
            }

            return string.Empty;
        }

        private bool AttributeIsSet(int attrid)
        {
            if (!string.IsNullOrWhiteSpace(AttributeSetKey(attrid)))
            {
                return true;
            }
            
            return false;
        }

        public void SetFormatedLabel(List<string> values)
        {
            if (!string.IsNullOrEmpty(ExplicitLabel) && values != null && values.Count > 0)
            {
                for(int i =0; i < values.Count; i++)
                {
                    ExplicitLabel = ExplicitLabel.Replace($"{{{i + 1}}}", values[i]);
                    ExplicitLabel = ExplicitLabel.Replace($"%{i + 1}", values[i]);
                }
            }
        }

        private string ValueForAttribute(int attrid)
        {
            string key = AttributeSetKey(attrid);
            if (!string.IsNullOrWhiteSpace(key))
            {
                FieldAttribute fd = _attributes[key];
                return fd.Value;
            }

            return string.Empty;
        }

        public void ParseColspan()
        {
            string value = ValueForAttribute((int)FieldAttributeType.ColSpan);
            char separator = ':';
            var range = value.IndexOf(separator);
            CombineWithIndices = false;
            string fieldCountPart = value;

            if (range > -1)
            {
                var strLineParts = value.Split(separator);

                if (strLineParts.Length > 3)
                {
                    if (strLineParts[1] == "Text")
                    {
                        this.NoPlaceHoldersInCombineString = true;
                    }
                    else if (strLineParts[1] == "FormatText")
                    {
                        this.CombineWithIndices = true;
                    }

                    LocalizationTextGroup = strLineParts[2];
                    LocalizationTextId = strLineParts[3];
                }
                else
                {
                    CombineString = strLineParts[1].Replace('b', ' ').Replace('c', separator);
                       
                }
                fieldCountPart = strLineParts[0];
            }
            else
            {
                CombineString = " ";
            }

            range = fieldCountPart.IndexOf(";", StringComparison.Ordinal);

            if (range > -1)
            {
                var strLineFieldCounts = fieldCountPart.Split(';');
                LineCount = strLineFieldCounts.Length;
                var lfc = new List<int>(LineCount);
                int fieldCount = 0;

                for (var j = 0; j < LineCount; j++)
                {
                    var intValue = int.Parse(strLineFieldCounts[j]);
                    fieldCount += intValue;
                    lfc.Add(intValue);
                }

                FieldLines = lfc;
                FieldCount = fieldCount;
            }
            else
            {
                FieldCount = int.Parse(fieldCountPart);
                LineCount = 1;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string Label()
        {
            if (!string.IsNullOrEmpty(ExplicitLabel))
            {
                return ExplicitLabel;
            }

            return FieldInfoLabel;
        }

        public RenderHooks RenderHooks()
        {
            return new RenderHooks(ValueForAttribute((int)FieldAttributeType.ColSpan));
        }

        public bool HasSelectFunction()
        {
            return !string.IsNullOrEmpty(SelectFunction());
        }

        public string SelectFunction()
        {
            return ValueForAttribute((int)FieldAttributeType.SelectFunction);
        }

        public bool IsSelectableInput()
        {
            return _fieldInfo.IsCatalog || !string.IsNullOrWhiteSpace(_fieldInfo.RepMode);
        }

        public string ExtendedOptionForKey(string key)
        {
            string strValue = ValueForAttribute((int)FieldAttributeType.ExtendedOptions);
            if(!string.IsNullOrEmpty(strValue))
            {
                try
                {
                    Dictionary<string, string> extendedOptionValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(strValue);
                    if(extendedOptionValues.ContainsKey(key))
                    {
                        return extendedOptionValues[key];
                    }
                }
                catch (Exception error)
                {

                }
            }
            return string.Empty;
        }

        public string ExplicitTrueValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ExplicitLabel))
                {
                    return null;
                }

                var parts = ExplicitLabel.Split(';');
                return parts.Length > 1 ? parts[1] : null;
            }
        }

        public string ExplicitFalseValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ExplicitLabel))
                {
                    return null;
                }

                var parts = ExplicitLabel.Split(';');
                return parts.Length > 2 ? parts[2] : null;
            }
        }

        public bool IsSectionField()
        {
            string sectionField = ExtendedOptionForKey("SectionField");
            if(!string.IsNullOrWhiteSpace(sectionField))
            {
                if(!sectionField.ToLower().Equals("false"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
