using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class ParticipantPanelControlModel : UIPanelWidget
    {
        private readonly IParticipantService _PartService;
        public ICommand OpenParticipantCommand => new Command<ParticipantData>(async evt => await OnParticepentClicked(evt));

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private List<ParticipantData> _participants;
        public List<ParticipantData> Participants
        {
            get => _participants;
            set
            {
                _participants = value;
                RaisePropertyChanged(() => Participants);
            }
        }

        public ParticipantPanelControlModel (object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }
            _PartService = AppContainer.Resolve<IParticipantService>();

        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (Data != null)
            {
                Title = Data.Label.ToUpperInvariant();
            }
            await _PartService.PrepareContentAsync(_cancellationTokenSource.Token);
            Participants = await _PartService.BuildParticipants(Data, _cancellationTokenSource.Token);
            HasData = Participants?.Count > 0;

            return true;
        }
        
        private async Task OnParticepentClicked(ParticipantData participant)
        {
            if (participant != null && !string.IsNullOrWhiteSpace(participant.LinkRecordId))
            {
                var userAction = new UserAction
                {
                    RecordId =  participant.LinkRecordId,
                    InfoAreaUnitName = participant.LinkInfoAreaID,
                    ActionUnitName = "SHOWRECORD",
                    ActionType = UserActionType.ShowRecord
                };
                _logService.LogInfo($"OpenParticepent user action for : {participant.Name}");
                await _navigationController.SimpleNavigateWithAction(userAction);
            }
        }

    }
}
