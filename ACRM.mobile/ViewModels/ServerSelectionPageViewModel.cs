using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class ServerSelectionPageViewModel : BaseViewModel
    {
        private readonly ICrmInstanceService _crmInstanceService;
        
        public ICommand CrmInstanceSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(evt =>
        {
            if (evt.ItemData is CrmInstance selectedItem)
            {
                _crmInstanceService.SetLastUseInstanceAsync(selectedItem.Identification);
                _navigationController.PopAllPopupAsync(null);
            }
        });

        public ICommand OnCloseButtonTapped => new Command(() =>
        {
            _navigationController.PopAllPopupAsync(null);
        });

        private IList<CrmInstance> _availableCrmInstances;
        public IList<CrmInstance> AvailableCrmInstances
        {
            get => _availableCrmInstances;
            set
            {
                _availableCrmInstances = value;
                RaisePropertyChanged(() => AvailableCrmInstances);
            }
        }

        private CrmInstance _selectedServer;
        public CrmInstance SelectedServer
        {
            get => _selectedServer;
            set
            {
                _selectedServer = value;
                RaisePropertyChanged(() => SelectedServer);
            }
        }


        public ServerSelectionPageViewModel(ICrmInstanceService crmInstanceService)
        {
            _crmInstanceService = crmInstanceService;
            MessagingCenter.Subscribe<RefreshEvent>(this, "RefreshEvent", (refreshEvent) =>
            {
                if (refreshEvent.IsRefreshNeeded)
                {
                    _ = LoadServers();
                }
            });
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await LoadServers();
            await base.InitializeAsync(navigationData);
        }

        private async Task LoadServers()
        {
            IsBusy = true;
            AvailableCrmInstances = await _crmInstanceService.GetCrmInstancesAsync();
            SelectedServer = _sessionContext.CrmInstance;
            IsBusy = false;
        }
    }
}
