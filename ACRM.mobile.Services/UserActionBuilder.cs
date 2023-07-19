using System;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Domain.Application.ActionTemplates;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ACRM.mobile.Services
{
    public class UserActionBuilder: IUserActionBuilder
    {
        private readonly ImageResolverComponent _imageResolverComponent;

        public UserActionBuilder(ImageResolverComponent imageResolverComponent)
        {
            _imageResolverComponent = imageResolverComponent;
        }

        public UserAction UserActionFromButton(IConfigurationService configurationService, Button button, string recordId = null, string recordInfoArea = null, string rawRecordId = null, bool isRecordRetrievedOnline = false)
        {
            string resourceName = button.ImageName;
            var (imageName, glyphText) = _imageResolverComponent.ExtractImage(configurationService, resourceName);
            ActionTemplateBase actionTemplate = ResolveActionTemplate(button.ViewReference);
            string actionAccentColor = GetActionInfoAreaColor(configurationService, actionTemplate);
            string label = string.IsNullOrWhiteSpace(button.Label) ? button.UnitName : button.Label;

            return new UserAction
            {
                ActionDisplayName = label,
                ActionUnitName = button.UnitName,
                ActionColorAccent = actionAccentColor,
                ActionType = ResolveActionType(button.ViewReference),
                ViewReference = button.ViewReference,
                RecordId = recordId,
                RawRecordId = rawRecordId,
                IsRecordRetrievedOnline = isRecordRetrievedOnline,
                SourceInfoArea = recordInfoArea,
                DisplayImageName = imageName,
                DisplayGlyphImageText = glyphText
            };
        }

        public UserAction UserActionFromMenu(IConfigurationService configurationService, Menu menu, string recordId = null, string infoAreaId = null)
        {
            string resourceName = menu.ImageName;
            var (imageName, glyphText) = _imageResolverComponent.ExtractImage(configurationService, resourceName);
            ActionTemplateBase actionTemplate = ResolveActionTemplate(menu.ViewReference);
            string actionAccentColor = GetActionInfoAreaColor(configurationService, actionTemplate);

            string displayName = menu.DisplayName.TrimEnd('\r', '\n');

            return new UserAction
            {
                ActionDisplayName = displayName,
                ActionUnitName = menu.UnitName,
                InfoAreaUnitName = infoAreaId,
                ActionColorAccent = actionAccentColor,
                ActionType = menu.ViewReference == null ? UserActionType.Menu : ResolveActionType(menu.ViewReference),
                ViewReference = menu.ViewReference,
                RecordId = recordId,
                DisplayImageName = imageName,
                DisplayGlyphImageText = glyphText
            };
        }

        private string GetActionInfoAreaColor(IConfigurationService configurationService, ActionTemplateBase actionTemplate)
        {
            string actionAccentColor = "black";
            if (actionTemplate != null && !String.IsNullOrEmpty(actionTemplate.InfoArea()))
            {
                InfoArea infoArea = configurationService.GetInfoArea(actionTemplate.InfoArea());
                if (infoArea != null && infoArea.ColorKey != null)
                {
                    actionAccentColor = infoArea.ColorKey;
                }
            }

            return actionAccentColor;
        }

        public ActionTemplateBase ResolveActionTemplate(ViewReference viewReference)
        {
            if(viewReference == null)
            {
                return null;
            }

            switch(viewReference.IdentificationName())
            {
                case "recordlistview":
                    return new RecordListViewTemplate(viewReference);
                case "calendarview":
                    return new CalendarViewTemplate(viewReference);
                case "documentview":
                    return new DocumentViewTemplate(viewReference);
                case "organizeraction":
                    return new OrganizerActionTemplate(viewReference);
                case "editview":
                    return new EditViewTemplate(viewReference);
                case "openurl":
                    return new OpenURLTemplate(viewReference);
                case "recordview":
                    return new RecordViewTemplate(viewReference);
                case "imageview":
                    return new ImageViewTemplate(viewReference);
                default:
                    return null;
            }
        }

        public UserActionType ResolveActionType(ViewReference viewReference)
        {
            if (viewReference == null)
            {
                return UserActionType.NotImplemented;
            }

            switch (viewReference.IdentificationName())
            {
                case "recordview":
                    return UserActionType.ShowRecord;
                case "recordlistview":
                    return UserActionType.RecordLists;
                case "calendarview":
                    return UserActionType.Calendar;
                case "documentview":
                    return UserActionType.DocumentView;
                case "organizeraction":
                    return UserActionType.NotImplemented;
                case "editview":
                    return UserActionType.NewOrEdit;
                case "webcontentview":
                case "webview":
                    return UserActionType.WebContentView;
                case "openurl":
                    return UserActionType.OpenURL;
                case "imageview":
                    return UserActionType.ImageView;
                case "confirmwebcontentview":
                    return UserActionType.ClientReport;
                case "questionnaireeditview":
                    return UserActionType.QuestionnaireEdit;
                case "recordswitch":
                case "switchonrecord":
                    return UserActionType.RecordSwitch;
                case "modifyrecord":
                    return UserActionType.ModifyRecord;
                default:
                    return UserActionType.NotImplemented; 
            }
        }

        private bool HasParentLink(UserAction userAction)
        {
            bool isNoLink = userAction?.ViewReference?.IsNoLink() ?? false;

            if(isNoLink)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(userAction?.RecordId))
            {
                return true;
            }

            return false;
        }

        public ParentLink GetParentLink(UserAction userAction, ActionTemplateBase actionTemplate)
        {
            if (userAction != null
                && actionTemplate != null
                && HasParentLink(userAction))
            {
                string parentLink = actionTemplate.ParentLink();
                if (string.IsNullOrEmpty(parentLink))
                {
                    parentLink = userAction.SourceInfoArea;
                }

                return new ParentLink {
                    LinkId = userAction.ForceLinkId > -1 ? userAction.ForceLinkId : actionTemplate.LinkId(),
                    ParentInfoAreaId = parentLink,
                    RecordId = userAction.RecordId
                };
            }

            return null;
        }


        public ParentLink GetRootRecord(UserAction userAction)
        {
            if (userAction != null)
            {
                return new ParentLink
                {
                    LinkId = -1,
                    ParentInfoAreaId = userAction.SourceInfoArea,
                    RecordId = userAction.RecordId
                };
            }

            return null;
        }


        public async Task<UserAction> ResolveSavedAction(IConfigurationService configurationService, string savedActionName, string recordId, string infoAreaId, CancellationToken cancellationToken)
        {
            switch (savedActionName.ToLower())
            {
                case "return":
                    return new UserAction
                    {
                        RecordId = recordId,
                        InfoAreaUnitName = infoAreaId,
                        ActionUnitName = "Back",
                        ActionType = UserActionType.NavigateBack
                    };
                case "home":
                    return new UserAction
                    {
                        RecordId = recordId,
                        InfoAreaUnitName = infoAreaId,
                        ActionUnitName = "Home",
                        ActionType = UserActionType.NavigateHome
                    };
                default:
                    if (savedActionName.ToLower().StartsWith("button:"))
                    {
                        Button button = await configurationService.GetButton(savedActionName.Substring(savedActionName.IndexOf(":") + 1), cancellationToken);
                        if (button != null)
                        {
                            return UserActionFromButton(configurationService, button, recordId, infoAreaId);
                        }
                    }
                    else if (savedActionName.ToLower().StartsWith("menu:"))
                    {
                        Menu menu = await configurationService.GetMenu(savedActionName.Substring(savedActionName.IndexOf(":") + 1), cancellationToken);
                        if (menu != null)
                        {
                            return UserActionFromMenu(configurationService, menu, recordId, infoAreaId);
                        }
                    }
                    else
                    {
                        Menu menu = await configurationService.GetMenu(savedActionName, cancellationToken);
                        if (menu != null)
                        {
                            return UserActionFromMenu(configurationService, menu, recordId, infoAreaId);
                        }
                        else
                        {
                            Button button = await configurationService.GetButton(savedActionName, cancellationToken);
                            if (button != null)
                            {
                                return UserActionFromButton(configurationService, button, recordId, infoAreaId);
                            }
                        }
                    }
                    break;
            }
            return null;
        }
    }
}
