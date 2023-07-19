using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class PopupListPageViewModel : BaseViewModel
    {
        private IPopupItemSelectionHandler PatentHandler;
        public ICommand SelectedItemCommand => new Command<PopupListItem>(async (item) => await SelectedItemCommandHandlerc(item));

        private async Task SelectedItemCommandHandlerc(PopupListItem item)
        {
            if(PatentHandler!=null)
            {
                await _navigationController.PopAllPopupAsync(null);
                await PatentHandler.PopupItemSelected(item);
            }

        }

        public ICommand OnCloseButtonTapped => new Command(async () =>
        {
            await _navigationController.PopAllPopupAsync(null);
        });
        private ObservableCollection<PopupListItem> _uiItems = new ObservableCollection<PopupListItem>();
        public ObservableCollection<PopupListItem> UIItems
        {
            get => _uiItems;
            set
            {
                _uiItems = value;
                RaisePropertyChanged(() => UIItems);
            }
        }
        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            if (navigationData is IPopupItemSelectionHandler popupObj)
            {
                PatentHandler = popupObj;
                var items = await PatentHandler.GetPoupList();
                if (items != null && items.Count > 0)
                {
                    UIItems = new ObservableCollection<PopupListItem>(items);
                }
                else
                {
                    UIItems = new ObservableCollection<PopupListItem>();
                }

            }

            await base.InitializeAsync(navigationData);
            IsLoading = false;
        }
    }
}
