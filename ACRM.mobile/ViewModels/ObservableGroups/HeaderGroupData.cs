using System.Collections.Generic;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.ViewModels.Base;
using SkiaSharp;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class HeaderGroupData: ExtendedBindableObject
    {
        public bool IsHeaderVisible { get; set; } = true;
        public Color InfoAreaColor { get; set; } = Color.LightGray;
        public List<UserAction> RelatedInfoAreas { get; set; }
        public List<UserAction> HeaderActions { get; set; }
        public UserAction SelectedRelatedInfoArea { get; set; }
        public bool AreActionsViewVisible { get; set; } = true;
        public bool IsOnlineRecord { get; set; } = false;

        public bool IsHeaderTableCaptionVisible { get; set; } = false;        
        public bool IsOrganizerHeaderVisible { get; set; } = false;
        public string OrganizerHeaderSubText { get; set; }
        public string HeaderTableCaptionText { get; set; }
        public ImageSource InfoAreaHeaderImageSource { get; set; } = ImageSource.FromFile("aurea.png");
        public bool IsInfoAreaHeaderImageVisible { get; set; } = false;

        public List<HeaderActionButton> HeaderActionButtons { get; set; }

        public void SetHeaderActionButtons(List<UserAction> userActions)
        {
            HeaderActions = userActions;

            List<HeaderActionButton> headerActionButtons = new List<HeaderActionButton>();

            foreach(UserAction userAction in userActions)
            {
                headerActionButtons.Add(new HeaderActionButton(userAction));
            }

            HeaderActionButtons = headerActionButtons;
        }
    }
}
