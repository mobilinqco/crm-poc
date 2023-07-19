using System;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels;

namespace ACRM.mobile.CustomControls
{
    public class UserActionResolver: IUserActionResolver
    {
        public UserActionResolver(IConfigurationService configurationService)
        {
        }

        public Type Resolve(UserAction userAction)
        {
            switch (userAction.ActionType)
            {
                case UserActionType.ShowRecord:
                    return typeof(DetailsPageViewModel);
                case UserActionType.RecordSelector:
                    return typeof(RecordSelectorPageViewModel);
                case UserActionType.Menu:
                    if (userAction.ViewReference == null && userAction.ActionUnitName.ToUpper().Contains("SHOWRECORD"))
                    {
                        return typeof(DetailsPageViewModel);
                    }
                    return ResolveViewReference(userAction.ViewReference);
                case UserActionType.Button:
                    return ResolveViewReference(userAction.ViewReference);
                default:
                    return ResolveViewReference(userAction.ViewReference);
            }
        }

        public Type ResolveViewReference(ViewReference viewReference)
        {
            if(viewReference == null)
            {
                return typeof(UserActionNotImplementedPageViewModel);
            }
            
            if (viewReference.Name.ToLower().Equals("requestforchange"))
            {
                return typeof(UserActionNotImplementedPageViewModel);

            }
            if (viewReference.Name.ToLower().Equals("recordview"))
            {
                return typeof(DetailsPageViewModel);

            }
            if (viewReference.Name.ToLower().Equals("geosearch"))
            {
                return typeof(GeoSearchPageViewModel);

            }
            if (viewReference.ViewName.ToLower().Equals("recordlistview"))
            {
                return typeof(SearchAndListPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("documentview"))
            {
                return typeof(DocumentPageViewModel);
            }

            if (viewReference.Name.ToLower().StartsWith("clientreport") || viewReference.Name.ToLower().StartsWith("report"))
            {
                return typeof(ClientReportPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("webcontentview"))
            {
                return typeof(WebContentPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("settingsview"))
            {
                return typeof(SettingsDetailsPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("settingseditview"))
            {
                return typeof(SettingsEditPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("fileuploadaction") || viewReference.ViewName.ToLower().Equals("photouploadaction"))
            {
                return typeof(DocumentUploadPageViewModel);
            }
            
            if (viewReference.ViewName.ToLower().Equals("editview"))
            {
                if(viewReference.Name.ToLower().Equals("editview"))
                {
                    return typeof(NewOrEditPageViewModel);
                }
                return typeof(NewOrEditPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("action:newinbackground"))
            {
                return typeof(NewOrEditPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("organizeraction"))
            {
                string actionAttribute = viewReference.GetArgumentValue("Action");
                if (!string.IsNullOrEmpty(actionAttribute))
                {
                    if(actionAttribute.ToLower().Equals("switchtoedit"))
                    {
                        return typeof(NewOrEditPageViewModel);
                    }
                }
            }

            if (viewReference.ViewName.Equals("CalendarView"))
            {
                return typeof(CalendarPageViewModel);
            }

            if (viewReference.ViewName.ToLower().Equals("serialentry"))
            {
                return typeof(SerialEntryPageViewModel);
            }

            if(viewReference.Name.ToLower().Equals("imageview"))
            {
                return typeof(ImageViewPageViewModel);
            }

            if(viewReference.Name.ToLower().Equals("characteristicsedit"))
            {
                return typeof(CharacteristicsEditPageViewModel);
            }
            if(viewReference.Name.ToLower().Equals("contacttimeseditview"))
            {
                return typeof(ContactTimesEditPageViewModel);
            }
            if(viewReference.Name.ToLower().Equals("questionnaireeditview"))
            {
                return typeof(QuestionnaireEditPageViewModel);
            }

            return typeof(UserActionNotImplementedPageViewModel);
        }
    }
}
