using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Calendar;
using ACRM.mobile.ViewModels.Base;
using Syncfusion.DataSource;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class RepSelectionPageViewModel: BaseViewModel
    {
        public ICommand CloseCommand => new Command(async () => await Close());
        public ICommand TextChangedCommand => new Command(() => OnTextChanged());
        public ICommand ItemTappedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>((args) => OnItemTapped(args));
        public ICommand ConfirmCommand => new Command(async () => await OnConfirm());

        private HashSet<string> _selectedCrmRepIds = new HashSet<string>();

        private string _closeText;
        public string CloseText
        {
            get => _closeText;
            set
            {
                _closeText = value;
                RaisePropertyChanged(() => CloseText);
            }
        }

        private string _titleText;
        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                RaisePropertyChanged(() => TitleText);
            }
        }

        private string _searchTextBoxPlaceholderText;
        public string SearchTextBoxPlaceholderText
        {
            get => _searchTextBoxPlaceholderText;
            set
            {
                _searchTextBoxPlaceholderText = value;
                RaisePropertyChanged(() => SearchTextBoxPlaceholderText);
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(() => SearchText);
            }
        }

        private ObservableCollection<BindableCrmRep> _bindableCrmReps = new ObservableCollection<BindableCrmRep>();
        public ObservableCollection<BindableCrmRep> BindableCrmReps
        {
            get => _bindableCrmReps;
            set
            {
                _bindableCrmReps = value;
                RaisePropertyChanged(() => BindableCrmReps);
            }
        }

        private DataSource _filterDataSource = new DataSource();
        public DataSource FilterDataSource
        {
            get => _filterDataSource;
            set
            {
                _filterDataSource = value;
                RaisePropertyChanged(() => FilterDataSource);
            }
        }

        private string _confirmText;
        public string ConfirmText
        {
            get => _confirmText;
            set
            {
                _confirmText = value;
                RaisePropertyChanged(() => ConfirmText);
            }
        }

        public RepSelectionPageViewModel()
        {
            InitProperties();
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            TitleText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicEmployee);
            SearchTextBoxPlaceholderText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicName);
            ConfirmText = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesSignatureConfirmButtonTitle); // TODO using correct localization

            FilterDataSource.Source = BindableCrmReps;
            FilterDataSource.Filter = FilterBindableCrmReps;
        }

        private bool FilterBindableCrmReps(object sourceObject)
        {
            if (sourceObject is BindableCrmRep bindableCrmRep)
            {
                if (SearchText == null || SearchText == "")
                {
                    return true;
                }
                else
                {
                    return bindableCrmRep.Name.ToLower().Contains(SearchText.ToLower());
                }
            }
            return false;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is List<BindableCrmRep> bindableCrmReps)
            {
                foreach (BindableCrmRep bindableCrmRep in bindableCrmReps)
                {
                    _bindableCrmReps.Add(bindableCrmRep);

                    if(bindableCrmRep.IsSelected)
                    {
                        _selectedCrmRepIds.Add(bindableCrmRep.Name);
                    }
                }
                BindableCrmReps = _bindableCrmReps;
            }
        }

        private async Task Close()
        {
            await _navigationController.PopPopupAsync();
        }

        private void OnTextChanged()
        {
            FilterDataSource.Refresh();
        }

        private void OnItemTapped(Syncfusion.ListView.XForms.ItemTappedEventArgs itemTappedEventArgs)
        {
            if (itemTappedEventArgs.ItemData is BindableCrmRep bindableCrmRep)
            {
                bindableCrmRep.IsSelected = !bindableCrmRep.IsSelected;

                if(bindableCrmRep.IsSelected)
                {
                    _selectedCrmRepIds.Add(bindableCrmRep.Name);
                }
                else
                {
                    _selectedCrmRepIds.Remove(bindableCrmRep.Name);
                }
            }
        }

        private async Task OnConfirm()
        {
            MessagingCenter.Send<BaseViewModel, HashSet<string>>(this, InAppMessages.SelectedRepIdSet, _selectedCrmRepIds);

            await _navigationController.PopPopupAsync();
        }
    }
}
