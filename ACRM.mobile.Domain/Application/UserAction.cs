using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.Domain.Application
{
    public class UserAction : INotifyPropertyChanged
    {
        public int Id { get; set; }
        // used to define the type of acction
        public UserActionType ActionType { get; set; }
        public UserActionTarget ActionTaget { get; set; }

        // Used to identify the configuration associated with the user action
        public string ActionUnitName { get; set; }
        public int SubActionUnitId { get; set; }
        public string ActionDisplayName { get; set; }
        public string DisplayImageName { get; set; }
        public string DisplayGlyphImageText { get; set; }
        public string ActionColorAccent { get; set; }

        // used to identify the data that needs to be displayed in the resulting view
        public string RecordId { get; set; }
        public string RawRecordId { get; set; }
        public string RecordIdForSavedAction { get; set; }
        public string LinkRecordId { get; set; }
        private string _infoAreaUnitName;
        public string InfoAreaUnitName
        {
            get
            {
                return string.IsNullOrWhiteSpace(_infoAreaUnitName) ? SourceInfoArea : _infoAreaUnitName;
            }
            set
            {
                _infoAreaUnitName = value;
            }
        }
        public string SourceInfoArea { get; set; }
        public string TargetLinkInfoAreaId { get; set; }
        public string ResolvedExpandName { get; set; }
        public bool IsRecordRetrievedOnline { get; set; }

        // Added this because the Documents (D3) are usualy having the default link set to 126/127
        public int ForceLinkId { get; set; } = -1;

        public bool UseForce { get; set; }

        public ViewReference ViewReference { get; set; }

        public RecordSelectorTemplate RecordSelector { get; set; }

        public Dictionary<string, string> AdditionalArguments { get; set; }

        // used to simplify the display. we can remove this in future if we find a better
        // solution for changing the background of a listview selected item.

        private bool _isSelected;

        public UserAction()
        {
        }

        public UserAction(UserAction _action)
        {
            ActionDisplayName = _action.ActionDisplayName;
            ActionTaget = _action.ActionTaget;
            ActionType = _action.ActionType;
            ActionUnitName = _action.ActionUnitName;
            SourceInfoArea = _action.SourceInfoArea;
            InfoAreaUnitName = _action.InfoAreaUnitName;
            SubActionUnitId = _action.SubActionUnitId;
            ViewReference = _action.ViewReference;
            IsSelected = _action.IsSelected;
            RecordId = _action.RecordId;
            DisplayGlyphImageText = _action.DisplayGlyphImageText;
            DisplayImageName = _action.DisplayImageName;
            UseForce = _action.UseForce;
            AdditionalArguments = _action.AdditionalArguments?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            RecordSelector = _action.RecordSelector;
            ResolvedExpandName = _action.ResolvedExpandName;
            IsRecordRetrievedOnline = _action.IsRecordRetrievedOnline;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsRecordListView()
        {
            if (ActionType == UserActionType.RecordLists)
            {
                return true;
            }

            if (ViewReference != null
                && !string.IsNullOrEmpty(ViewReference.ViewName)
                && ViewReference.ViewName == "RecordListView")
            {
                return true;
            }

            return false;
        }

        public bool IsDeleteAction()
        {
            if (ViewReference != null)
            {
                return ViewReference.IsDeleteAction();
            }

            return false;
        }

        public string GetLinkId()
        {
            return ViewReference.GetArgumentValue("LinkId");
        }

        public string GetRightsFilterName()
        {
            return ViewReference?.GetArgumentValue("RightsFilterName");
        }

        public string GetRightsFilter()
        {
            var rightsFilter = ViewReference?.GetArgumentValue("RightsFilter");
            if (string.IsNullOrWhiteSpace(rightsFilter))
            {
                rightsFilter = GetRightsFilterName();

            }
            return rightsFilter;
        }

        public string GetDeleteWarningFilter()
        {
            return ViewReference?.GetArgumentValue("DeleteWarningFilter");
        }

        public OfflineRecordLink GetLinkRequest()
        {
            if (string.IsNullOrWhiteSpace(RecordId))
            {
                return null;
            }

            if (ViewReference != null)
            {
                var strLinkId = ViewReference.GetArgumentValue("LinkId");

                int intLinkId;
                if (!int.TryParse(strLinkId, out intLinkId))
                {
                    intLinkId = 0;
                }

                string infoAreaId = SourceInfoArea;
                if(string.IsNullOrWhiteSpace(infoAreaId))
                {
                    infoAreaId = InfoAreaUnitName;
                }

                return new OfflineRecordLink()
                {
                    InfoAreaId = infoAreaId,
                    LinkId = intLinkId,
                    RecordId = RecordId
                };
            }

            return null;
        }

        public OfflineRecordLink GetLinkParentRequest()
        {
            if (string.IsNullOrWhiteSpace(RecordId))
            {
                return null;
            }

            if (ViewReference != null)
            {
                int intLinkId = 126;
                return new OfflineRecordLink()
                {
                    InfoAreaId = SourceInfoArea,
                    LinkId = intLinkId,
                    RecordId = RecordId
                };
            }

            return null;
        }

        public string IconFileName()
        {
            if (string.IsNullOrWhiteSpace(ActionUnitName))
            {
                return $"image_action_icon_defaultIcon.png";
            }

            string name = ActionUnitName.Replace('/', '_').Replace('\\', '_').ToLower();

            return $"image_action_icon_{name}.png";
        }


        public bool IsToggleFavorite()
        {
            if (ActionUnitName.Equals("ToggleFavorite"))
            {
                return true;
            }

            return false;
        }

        public bool IsNavigableAction()
        {
            if (IsToggleFavorite()
                || IsDeleteAction()
                || (ViewReference != null
                    && (ViewReference.IsModifyRecordAction()
                    || ViewReference.IsSyncRecordAction()
                    || ViewReference.IsOpenUrlAction())))
            {
                return false;
            }

            return true;
        }

        public bool AreSectionsEnabled()
        {
            if (ViewReference != null)
            {
                string val = ViewReference?.GetArgumentValue("Sections");
                if(!string.IsNullOrWhiteSpace(val) && !val.ToLower().Equals("false") && !val.ToLower().Equals("0"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
