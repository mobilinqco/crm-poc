using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ACRM.mobile.Domain.FormatUtils;

namespace ACRM.mobile.Domain.Application
{
    public class ParticipantData : INotifyPropertyChanged
    {
        public Dictionary<string, string> RequirementIcons = new Dictionary<string, string>
        {
            {"0", "\U000f0028" },
            {"1", "\U000f02d7" },
            {"2", "\U000f02fc" }
        };

        public Dictionary<string, string> AcceptanceIcons = new Dictionary<string, string>
        {
            {"0", "\U000f078c" },
            {"1", "\U000f05e0"},
            {"2", "\U000f0159" },
            {"3", "\U000f1464" }
        };

        public (string image, string glyph) ImageSource
        {
            get; set;
        }
        private bool IsLinkParticepent = false;
        public int RepId { get; private set; }
        public string Key { get; set; }
        public string LinkRecordId { get; private set; }
        public string LinkInfoAreaID { get; private set; }
        public string GroupKey { get; private set; }
        public bool AllowDelete { get; set; } = true;
        public List<PanelData> Panels { get; set; }
        public int DefaultdRequirementIndex { get; set; } = -1;
        public int DefaultdAcceptanceIndex { get; set; } = -1;


        private int _selectedRequirementIndex = 0;
        public int SelectedRequirementIndex
        {
            get
            {
                return _selectedRequirementIndex;
            }
            set
            {
                _selectedRequirementIndex = value;
                OnPropertyChanged();
                SetRequirement(SelectedRequirementIndex);

            }
        }
        private int _selectedAcceptanceIndex = 0;
        public int SelectedAcceptanceIndex
        {
            get
            {
                return _selectedAcceptanceIndex;
            }
            set
            {
                _selectedAcceptanceIndex = value;
                OnPropertyChanged();
                SetAcceptance(SelectedAcceptanceIndex);
            }
        }

        private void SetRequirement(int Index)
        {
            if (_requirements != null && _requirements.Count > 0)
            {
                RequirementText = _requirements[Index].RecordId;
                RequirementDisplayText = _requirements[Index].DisplayText;
            }
        }
        private void SetAcceptance(int Index)
        {
            if (_acceptance != null && _acceptance.Count > 0)
            {
                AcceptanceText = _acceptance[Index].RecordId;
                AcceptanceDisplayText = _acceptance[Index].DisplayText;
            }
        }

        private string _leftMarginColor;
        public string LeftMarginColor
        {
            get
            {
                return _leftMarginColor;
            }
            set
            {
                if (_leftMarginColor != value)
                {
                    _leftMarginColor = value;
                    OnPropertyChanged();
                }
            }
        }
        public string _participantString;

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _acceptanceText;
        public string AcceptanceText
        {
            get
            {
                return _acceptanceText;
            }
            set
            {
                if (_acceptanceText != value)
                {
                    _acceptanceText = value;
                    OnPropertyChanged();
                    if (AcceptanceIcons.ContainsKey(_acceptanceText))
                    {
                        AcceptanceIcon = AcceptanceIcons[_acceptanceText];
                    }
                    else
                    {
                        AcceptanceIcon = AcceptanceIcons["0"];
                    }
                }
            }
        }

        private string _requirementText;
        public string RequirementText
        {
            get
            {
                return _requirementText;
            }
            set
            {
                if (_requirementText != value)
                {
                    _requirementText = value;
                    OnPropertyChanged();
                    if (RequirementIcons.ContainsKey(_requirementText))
                    {
                        RequirementIcon = RequirementIcons[_requirementText];
                    }
                    else
                    {
                        RequirementIcon = RequirementIcons["0"];
                    }
                }
            }
        }

        private List<PopupListItem> _requirements;
        public List<PopupListItem> Requirements
        {
            get => _requirements;
            set
            {
                _requirements = value;
                if (_requirements != null)
                {
                    foreach (var item in _requirements)
                    {
                        if (RequirementIcons.ContainsKey(item.RecordId))
                        {
                            item.ImageSource = (string.Empty, RequirementIcons[item.RecordId]);
                        }
                        else
                        {
                            item.ImageSource = (string.Empty, RequirementIcons["0"]);
                        }
                    }

                    var intex = _requirements.FindIndex(i => i.RecordId.Equals(RequirementText, StringComparison.InvariantCultureIgnoreCase));
                    if (intex > -1)
                    {
                        SelectedRequirementIndex = intex;
                    }

                }

                OnPropertyChanged();
            }
        }
        private List<PopupListItem> _acceptance;
        public List<PopupListItem> Acceptance
        {
            get => _acceptance;
            set
            {
                _acceptance = value;
                if (_acceptance != null)
                {
                    foreach (var item in _acceptance)
                    {
                        if (AcceptanceIcons.ContainsKey(item.RecordId))
                        {
                            item.ImageSource = (string.Empty, AcceptanceIcons[item.RecordId]);
                        }
                        else
                        {
                            item.ImageSource = (string.Empty, AcceptanceIcons["0"]);
                        }
                    }

                    var intex = _acceptance.FindIndex(i => i.RecordId.Equals(AcceptanceText, StringComparison.InvariantCultureIgnoreCase));
                    if (intex > -1)
                    {
                        SelectedAcceptanceIndex = intex;
                    }

                }

                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Gets the requirement display text.
        /// </summary>
        /// <value>
        /// The requirement display text.
        /// </value>
        private string _requirementDisplayText;
        public string RequirementDisplayText
        {
            get
            {
                return _requirementDisplayText;
            }
            set
            {
                if (_requirementDisplayText != value)
                {
                    _requirementDisplayText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _requirementIcon;
        public string RequirementIcon
        {
            get
            {
                return _requirementIcon;
            }
            set
            {
                if (_requirementIcon != value)
                {
                    _requirementIcon = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _acceptanceIcon;
        public string AcceptanceIcon
        {
            get
            {
                return _acceptanceIcon;
            }
            set
            {
                if (_acceptanceIcon != value)
                {
                    _acceptanceIcon = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Gets the acceptance display text.
        /// </summary>
        /// <value>
        /// The acceptance display text.
        /// </value>
        private string _acceptanceDisplayText;

        public string AcceptanceDisplayText
        {
            get
            {
                return _acceptanceDisplayText;
            }
            set
            {
                if (_acceptanceDisplayText != value)
                {
                    _acceptanceDisplayText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RepParticipantString
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(this.Key);
                if (!string.IsNullOrEmpty(this.GroupKey))
                {
                    stringBuilder.Append($",{this.GroupKey}");
                }
                if (!string.IsNullOrEmpty(this.RequirementText) && !this.RequirementText.Equals("0"))
                {
                    stringBuilder.Append($":{this.RequirementText}");
                }
                return stringBuilder.ToString();
            }
        }
           


        public int RepGroupId { get; private set; }
        /// <summary>
        /// Gets the rep identifier string.
        /// </summary>
        /// <value>
        /// The rep identifier string.
        /// </value>
        public string RepIdString { get; private set; }
        public ParticipantData(string participantString)
        {
            string[] arr = participantString.Split(':');

            var requirementText = arr.Length > 1 ? arr[1] : "0";

            arr = arr[0].Split(',');
            this.Key = arr[0];
            this.RequirementText = requirementText;
            this.AcceptanceText = "0";
            this._participantString = participantString;
            this.GroupKey = arr.Length > 1 ? arr[1] : string.Empty;

            this.RepGroupId = this.GroupKey.RepId();
            this.RepId = this.Key.RepId();
            this.RepIdString = $"{this.RepId}".FormattedRepId();
        }

        public ParticipantData(string name, string RecordId, string requirementText, string acceptanceText, string linkRecordId, string linkInfoAreaID)
        {
            IsLinkParticepent = true;
            Name = name;
            Key = RecordId;
            RequirementText = requirementText;
            AcceptanceText = acceptanceText;
            LinkInfoAreaID = linkInfoAreaID;
            LinkRecordId = linkRecordId;
            DefaultdRequirementIndex = SelectedRequirementIndex;
            DefaultdAcceptanceIndex = SelectedAcceptanceIndex;

        }

        public ParticipantData(CrmRep rep)
        {
            if(rep!=null)
            {
                Name = rep.Name;
                Key = rep.Id;
                GroupKey = rep.OrgGroupId;
                RequirementText = "0";
                AcceptanceText = "0";
                this.RepGroupId = this.GroupKey.RepId();
                this.RepId = this.Key.RepId();
                this.RepIdString = $"{this.RepId}".FormattedRepId();
                SetRepLinks(rep.RecordIdentification);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void LoadIntex()
        {
            if (Requirements != null)
            {
                var intex = _requirements.FindIndex(i => i.RecordId.Equals(RequirementText, StringComparison.InvariantCultureIgnoreCase));
                if (intex > -1)
                {
                    SelectedRequirementIndex = intex;
                }
            }
            if (Acceptance != null)
            {
                var intex = _acceptance.FindIndex(i => i.RecordId.Equals(AcceptanceText, StringComparison.InvariantCultureIgnoreCase));
                if (intex > -1)
                {
                    SelectedAcceptanceIndex = intex;
                }
            }
        }

        public void SetRepLinks(string recordIdentification)
        {
            this.LinkRecordId = recordIdentification;
            this.LinkInfoAreaID = "ID";
        }
    }
}
