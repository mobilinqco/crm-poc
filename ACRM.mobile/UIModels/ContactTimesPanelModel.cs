using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ContactTimes;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ValueConverters;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class ContactTimesPanelModel : UIPanelWidget
    {
        private readonly IContactTimesContentService _contentService;

        private string _titleLabelText;
        public string TitleLabelText
        {
            get => _titleLabelText;
            set
            {
                _titleLabelText = value;
                RaisePropertyChanged(() => TitleLabelText);

            }
        }

        private ContactTimesModel _contactTimesModel;
        public ContactTimesModel ContactTimesModel
        {
            get => _contactTimesModel;
            set
            {
                _contactTimesModel = value;
                RaisePropertyChanged(() => ContactTimesModel);
            }
        }

        public ContactTimesPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<IContactTimesContentService>();
            if (widgetArgs is PanelData panelData)
            {
                Data = panelData;
            }
        }

        public override async ValueTask<bool> InitializeControl()
        {
            IsLoading = true;

            InitializeProperties();

            if (Data.action != null)
            {
                _contentService.SetSourceAction(Data.action);
                _contentService.SetWeekDayNames(GetWeekDayNames());
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                if (_contentService.HasData)
                {
                    ContactTimesModel = new ContactTimesModel(_contentService.GetContactTimesDataGridDays(), _cancellationTokenSource);
                    await ContactTimesModel.InitializeControl();
                    HasData = true;
                }
                else 
                {
                    HasData = false;
                }
            }

            IsLoading = false;
            return true;
        }

        private List<string> GetWeekDayNames()
        {
            return new List<string>()
            {
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysMonday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysTuesday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysWednesday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysThursday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysFriday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysSaturday),
                _localizationController.GetString(LocalizationKeys.TextGroupNamesOfWeekdays, LocalizationKeys.KeyNamesOfWeekdaysSunday)
            };
        }

        private void InitializeProperties()
        {
            TitleLabelText = Data.Label.ToUpper();
        }
    }
}
