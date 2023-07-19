using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Messages;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class InsightBoardModel : UIPanelWidget
    {
        private IConfigurationService _configurationService;
        private IUserActionBuilder _userActionBuilder;
        public ICommand InsightBoardItemNavigationCommand => new Command<InsightBoardItem>(async item => await InsightBoardItemNavigation(item));
        private bool _isBusy;

        
        private List<InsightBoardItem> _insightBoardActions;
        public List<InsightBoardItem> InsightBoardActions
        {
            get => _insightBoardActions;
            set
            {
                _insightBoardActions = value;
                RaisePropertyChanged(() => InsightBoardActions);
            }
        }

        public InsightBoardModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _configurationService = AppContainer.Resolve<IConfigurationService>();
            _userActionBuilder = AppContainer.Resolve<IUserActionBuilder>();

            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }
            else if (widgetArgs is List<UserAction> actions && actions.Count > 0)
            {
                InsightBoardActions = new List<InsightBoardItem>();
                foreach (var action in actions)
                {
                    InsightBoardActions.Add(BuildInsightBoardItem(action));
                }
            }
        }

        private async Task BuildInsightBoardForFormsObjects()
        {
            if (WidgetConfig?.FormItem != null)
            {
                InsightBoardActions = new List<InsightBoardItem>();
                var funparts = WidgetConfig.FormItem.Func.Split(';');
                
                if (funparts !=null && funparts.Length > 0 && !string.IsNullOrWhiteSpace(funparts[0]))
                {
                    var insightBoardMenuName = funparts[0];
                    InsightBoardActions = await BuildInsightBoardActions(insightBoardMenuName);
                }
            }
        }

        private async Task BuildInsightBoardForDetailsObjects()
        {
            if (Data != null)
            {
                InsightBoardActions = new List<InsightBoardItem>();
                var funparts = Data.PanelTypeKey.Split('_');

                if (funparts != null && funparts.Length > 1 && !string.IsNullOrWhiteSpace(funparts[1]))
                {
                    var insightBoardMenuName = funparts[1];
                    InsightBoardActions = await BuildInsightBoardActions(insightBoardMenuName);
                }
            }
        }

        private async Task<List<InsightBoardItem>> BuildInsightBoardActions(string insightBoardMenuName)
        {
            var actions = new List<InsightBoardItem>();
            var appMenu = _configurationService.GetMenu(insightBoardMenuName, _cancellationTokenSource.Token).Result;

            if (WidgetConfig?.FormParams != null)
            {
                RegisterMessageIfNotExist(WidgetEventType.FormParamsChanged, "*", OnFormItemChanged);
            }

            if (appMenu != null && !string.IsNullOrEmpty(appMenu.Items))
            {
                List<string> menuNames = _configurationService.ExtractConfigFromJsonString(appMenu.Items);
                foreach (string menuName in menuNames)
                {
                    try
                    {
                        Domain.Configuration.UserInterface.Menu insightBoardItemMenu = await _configurationService.GetMenu(menuName, _cancellationTokenSource.Token);
                        if (insightBoardItemMenu == null || insightBoardItemMenu.ViewReference == null)
                        {
                            _logService.LogError($"Unable to retrieve insight Board Item Menu {menuName}");
                        }
                        else if (insightBoardItemMenu.ViewReference.ViewName.Equals("InsightBoardItem"))
                        {
                            string subMenuName = insightBoardItemMenu.ViewReference.Arguments.Find(a => a.Name.Equals("ContextMenu"))?.Value;
                            if (!string.IsNullOrWhiteSpace(subMenuName))
                            {
                                Domain.Configuration.UserInterface.Menu menu = await _configurationService.GetMenu(subMenuName, _cancellationTokenSource.Token);
                                if (menu.ViewReference != null)
                                {
                                    menu.DisplayName = insightBoardItemMenu.DisplayName;
                                    menu.ImageName = insightBoardItemMenu.ImageName;

                                    UserAction ua = _userActionBuilder.UserActionFromMenu(_configurationService, menu);
                                    if (Data != null)
                                    {
                                        ua.RecordId = Data.RecordId;
                                        ua.SourceInfoArea = Data.RecordInfoArea;
                                    }

                                    actions.Add(BuildInsightBoardItem(ua, insightBoardItemMenu));
                                }
                            }
                            else
                            {
                                subMenuName = insightBoardItemMenu.ViewReference.Arguments.Find(a => a.Name.Equals("ConfigName"))?.Value;
                                if (!string.IsNullOrWhiteSpace(subMenuName))
                                {
                                    insightBoardItemMenu.ImageName = string.Empty;
                                    UserAction ua = _userActionBuilder.UserActionFromMenu(_configurationService, insightBoardItemMenu);
                                    ua.ViewReference.ViewName = "RecordListView";
                                    ua.ViewReference.Name = "RecordListView";
                                    if (Data != null)
                                    {
                                        ua.RecordId = Data.RecordId;
                                        ua.SourceInfoArea = Data.RecordInfoArea;
                                    }

                                    actions.Add(BuildInsightBoardItem(ua));
                                }
                            }
                        }
                        else
                        {
                            UserAction ua = _userActionBuilder.UserActionFromMenu(_configurationService, insightBoardItemMenu);
                            if (ua != null)
                            {
                                if (Data != null)
                                {
                                    ua.RecordId = Data.RecordId;
                                    ua.SourceInfoArea = Data.RecordInfoArea;
                                }

                                actions.Add(BuildInsightBoardItem(ua));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"Unable to process {WidgetConfig?.FormItem?.Func} Menu {ex.Message}");
                    }
                }
            }

            return actions;
        }

        public async Task InsightBoardItemNavigation(InsightBoardItem item)
        {
            if (!_isBusy && item != null)
            {
                if (item.UserAction.IsNavigableAction())
                {
                    _isBusy = true;
                    await _navigationController.NavigateAsyncForAction(item.UserAction, _cancellationTokenSource.Token);
                    _isBusy = false;
                }
                else
                {
                    await PublishMessage(new WidgetMessage
                    {
                        EventType = WidgetEventType.InsightItemActionForParent,
                        ControlKey = string.Empty,
                        Data = item.UserAction
                    }, MessageDirections.ToParent);
                }
            }
        }
        
        public override async ValueTask<bool> InitializeControl()
        {
            _isBusy = true;
            await BuildInsightBoardForFormsObjects();
            await BuildInsightBoardForDetailsObjects();
            PrepareTileCount().SafeFireAndForget<Exception>(onException: ex =>
            {
                _logService.LogError($"Unable to prepare content {ex.Message}");
            });
            _isBusy = false;
            return true;
        }

        internal async Task PrepareTileCount()
        {
            List<Task> tasks = new List<Task>();
            foreach (var item in InsightBoardActions)
            {
                if (item.HasCount && item.IsLoading)
                {
                    tasks.Add(item.LoadCount(_cancellationTokenSource.Token));
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        private InsightBoardItem BuildInsightBoardItem(UserAction action, Domain.Configuration.UserInterface.Menu menu = null)
        {
            action.AdditionalArguments = GetFormParams();

            return new InsightBoardItem(action, menu);
        }

        private Dictionary<string, string> GetFormParams()
        {
            if (WidgetConfig?.FormItem != null)
            {
                string[] typeParts = WidgetConfig?.FormItem.Func.Split(';');
                var param1 = (typeParts.Length > 2 && typeParts[typeParts.Length - 1].StartsWith("$"))
                    ? typeParts[typeParts.Length - 1]
                    : string.Empty;
                return WidgetConfig.FormParams.ContainsKey(param1) ?
                    WidgetConfig.FormParams[param1] :
                    null;
            }
            return null;
        }

        private async Task OnFormItemChanged(WidgetMessage arg)
        {
            _isBusy = true;

            var FormParams = arg.Data as Dictionary<string, Dictionary<string, string>>;

            foreach (var item in InsightBoardActions)
            {
                if (item.HasCount)
                {
                    item.UserAction.AdditionalArguments = GetFormParams();
                    item.IsLoading = true;
                }
            }

            PrepareTileCount().SafeFireAndForget<Exception>(onException: ex =>
            {
                _logService.LogError($"Unable to prepare content {ex.Message}");
            });

            _isBusy = false;
        }
    }
}
