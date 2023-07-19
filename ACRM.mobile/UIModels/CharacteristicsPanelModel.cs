using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Characteristics;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class CharacteristicsPanelModel : UIPanelWidget
    {
        private readonly ICharacteristicsContentService _contentService;

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

        private ObservableCollection<CharacteristicGroup> _characteristicGroups = new ObservableCollection<CharacteristicGroup>();
        public ObservableCollection<CharacteristicGroup> CharacteristicGroups
        {
            get => _characteristicGroups;
            set
            {
                _characteristicGroups = value;
                RaisePropertyChanged(() => CharacteristicGroups);
            }
        }

        public CharacteristicsPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<ICharacteristicsContentService>();
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
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                var Result = _contentService.GetCharacteristicGroups();
                foreach (CharacteristicGroup characteristicGroup in Result)
                {
                    _characteristicGroups.Add(characteristicGroup);
                }
                CharacteristicGroups = _characteristicGroups;
            }

            IsLoading = false;
            return true;
        }

        private void InitializeProperties()
        {
            TitleLabelText = Data.Label.ToUpper();
        }
    }
}
