using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class AppSearchMenuService: ContentServiceBase, IAppSearchMenuService
    {
        private UserAction _globalSearch;
        private UserAction _historySearch;
        private UserAction _favoriteSearch;
        private List<UserAction> _relatedInfoAreas;


        public AppSearchMenuService(ISessionContext sessionContext,
           IConfigurationService configurationService,
           ICrmDataService crmDataService,
           ILogService logService,
           IUserActionBuilder userActionBuilder,
           HeaderComponent headerComponent,
           FieldGroupComponent fieldGroupComponent,
           ImageResolverComponent imageResolverComponent,
           IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
               crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _globalSearch = null;
            _historySearch = null;
            _favoriteSearch = null;
            _relatedInfoAreas = new List<UserAction>();
        }

        public UserAction FavoriteSearch()
        {
            return _favoriteSearch;
        }

        public UserAction GlobalSearch()
        {
            return _globalSearch;
        }

        public UserAction HistorySearch()
        {
            return _historySearch;
        }

        public List<UserAction> InfoAreaRelatedActions()
        {
            return _relatedInfoAreas;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            Menu appSearchMenu = await _configurationService.GetMenu("$AppSearchMenu", cancellationToken);
            if(appSearchMenu == null)
            {
                appSearchMenu = await _configurationService.GetMenu("SMARTBOOK", cancellationToken);
            }

            if(appSearchMenu != null && !string.IsNullOrEmpty(appSearchMenu.Items))
            {
                List<string> menuNames = _configurationService.ExtractConfigFromJsonString(appSearchMenu.Items);
                foreach(string menuName in menuNames)
                {
                    try
                    {
                        Menu menu = await _configurationService.GetMenu(menuName, cancellationToken);
                        if (menu?.ViewReference != null)
                        {
                            UserAction ua = _userActionBuilder.UserActionFromMenu(_configurationService, menu);

                            if (menuName.Equals("$HistorySearch") || ua.ViewReference.ViewName.Equals("HistoryListView"))
                            {
                                _historySearch = ua;
                                continue;
                            }

                            if (menu.ViewReference.IdentificationName().Equals("recordlistview") ||
                                menu.ViewReference.IdentificationName().Equals("calendarview") ||
                                menu.ViewReference.IdentificationName().Equals("documentview") ||
                                menu.ViewReference.IdentificationName().Equals("imageview"))
                            {
                                if (menuName.Equals("$GlobalSearch") ||
                                    menuName.Equals("GLOBALSEARCH"))
                                {
                                    _globalSearch = ua;
                                    continue;
                                }
                                if (menuName.Equals("$FavoriteSearch"))
                                {
                                    _favoriteSearch = ua;
                                    continue;
                                }
                                _relatedInfoAreas.Add(ua);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"Unable to process App Search Menu {ex.Message}");
                    }
                }
            }
            OnDataReady();
        }
    }
}
