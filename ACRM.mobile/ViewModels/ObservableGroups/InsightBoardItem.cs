using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class InsightBoardItem : ExtendedBindableObject
    {
        private Domain.Configuration.UserInterface.Menu _insightBoardParentMenu;

        private bool _hasCount = false;
        public bool HasCount
        {
            get => _hasCount;
            set
            {
                _hasCount = value;
                RaisePropertyChanged(() => HasCount);
            }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        private long _records = 0;
        public long Records
        {
            get => _records;
            set
            {
                _records = value;
                RaisePropertyChanged(() => Records);
            }
        }

        private bool _isCounterVisible = false;
        public bool IsCounterVisible
        {
            get => _isCounterVisible;
            set
            {
                _isCounterVisible = value;
                RaisePropertyChanged(() => IsCounterVisible);
            }
        }

        private bool _isDisplayImageNameVisible = true;
        public bool IsDisplayImageNameVisible
        {
            get => _isDisplayImageNameVisible;
            set
            {
                _isDisplayImageNameVisible = value;
                RaisePropertyChanged(() => IsDisplayImageNameVisible);
            }
        }

        private bool _isDisplayGlyphImageTextVisible = true;
        public bool IsDisplayGlyphImageTextVisible
        {
            get => _isDisplayGlyphImageTextVisible;
            set
            {
                _isDisplayGlyphImageTextVisible = value;
                RaisePropertyChanged(() => IsDisplayGlyphImageTextVisible);
            }
        }

        public UserAction UserAction { get; private set; }

        public InsightBoardItem(UserAction action, Domain.Configuration.UserInterface.Menu insightBoardParentMenu = null)
        {
            bool hasIcon = true;

            UserAction = action;
            _insightBoardParentMenu = insightBoardParentMenu;

            if (UserAction.ViewReference?.Name == "RecordListView")
            {
                if (ShowCountAndIcon())
                {
                    hasIcon = true;
                    HasCount = true;
                    IsLoading = true;
                }
                else if (DisableInsightBoardCount())
                {
                    hasIcon = true;
                    HasCount = false;
                    IsLoading = false;
                }
                else
                {
                    hasIcon = false;
                    HasCount = true;
                    IsLoading = true;
                }
            }
            else
            {
                hasIcon = true;
                HasCount = false;
                IsLoading = false;
            }

            IsDisplayImageNameVisible = hasIcon && !string.IsNullOrWhiteSpace(UserAction.DisplayImageName);
            IsDisplayGlyphImageTextVisible = hasIcon && !string.IsNullOrWhiteSpace(UserAction.DisplayGlyphImageText);
        }

        internal async Task LoadCount(CancellationToken token)
        {
            ISearchContentService contentService = AppContainer.Resolve<ISearchContentService>();
            var prepareDataTask = Task.Run(() =>
            {
                RequestMode requestMode = UserAction.ViewReference?.GetRequestMode() ?? RequestMode.Best;
                if(_insightBoardParentMenu != null)
                {
                    requestMode = _insightBoardParentMenu.ViewReference?.GetRequestMode() ?? requestMode;
                }

                return contentService.PrepareRecordCountAsync(UserAction, requestMode, token).Result;
            });
            await prepareDataTask.ContinueWith(
                 antecedent =>
                 {
                     Records = antecedent.Result;
                     IsLoading = false;
                     IsCounterVisible = HasCount && !IsLoading;
                 });
        }

        private bool DisableInsightBoardCount()
        {
            string paramValue = string.Empty;
            if (_insightBoardParentMenu != null)
            {
                if (_insightBoardParentMenu.ViewReference != null)
                {
                    paramValue = _insightBoardParentMenu.ViewReference.GetArgumentValue("ForceActionStyle");
                }
            }

            if(string.IsNullOrWhiteSpace(paramValue))
            {
                if(UserAction.ViewReference != null)
                {
                    paramValue = UserAction.ViewReference.GetArgumentValue("DisableInsightBoardCount"); 
                }
            }
            
            if (!string.IsNullOrWhiteSpace(paramValue))
            {
                // TODO: we may need some string formating.
                if (bool.TryParse(paramValue, out bool boolValue))
                {
                    return boolValue;
                }
            }

            return false;
        }

        private bool ShowCountAndIcon()
        {
            string paramValue = string.Empty;
            if (_insightBoardParentMenu != null)
            {
                if (_insightBoardParentMenu.ViewReference != null)
                {
                    paramValue = _insightBoardParentMenu.ViewReference.GetArgumentValue("ShowCountAndIcon");
                }
            }

            if (string.IsNullOrWhiteSpace(paramValue))
            {
                if (UserAction.ViewReference != null)
                {
                    paramValue = UserAction.ViewReference.GetArgumentValue("ShowCountAndIcon");
                }
            }

            if (!string.IsNullOrWhiteSpace(paramValue))
            {
                // TODO: we may need some string formating.
                if (bool.TryParse(paramValue, out bool boolValue))
                {
                    return boolValue;
                }
            }

            return false;
        }
    }
}
