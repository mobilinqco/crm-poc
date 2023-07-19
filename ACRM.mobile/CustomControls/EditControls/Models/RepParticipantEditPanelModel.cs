using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class RepParticipantEditPanelModel : EditPanelControlModel
    {
        public ICommand RemoveParticipantCommand => new Command<ParticipantData>(async (participant) => await RemoveSelectedParticipantData(participant));
        public ICommand AddParticipantCommand => new Command<SelectableFieldValue>(async (rep) => await AddSelectedRep(rep));

        private async Task AddSelectedRep(SelectableFieldValue selectedRep)
        {
            if(selectedRep!=null)
            {
                NotifyDirtyState();
                ParticipantData participant = await _PartService.GetParticipant(selectedRep, _cancellationTokenSource.Token);
                if(participant!=null && !Participants.Any(p => p.Key.Equals(participant.Key)))
                {
                    Participants.Add(participant);
                }
            }

        }

        private async Task RemoveSelectedParticipantData(ParticipantData participant)
        {
            if (participant != null)
            {
                NotifyDirtyState();
                Participants.Remove(participant);
            }
        }

        private List<SelectableFieldValue> _repList;
        public List<SelectableFieldValue> RepList
        {
            get => _repList;
            set
            {
                _repList = value;
                RaisePropertyChanged(() => RepList);
            }
        }

        private SelectableFieldValue _selectedValue;
        public SelectableFieldValue SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                RaisePropertyChanged(() => SelectedValue);
            }
        }

        private readonly IParticipantService _PartService;
        private ObservableCollection<ParticipantData> _participants;
        public ObservableCollection<ParticipantData> Participants
        {
            get => _participants;
            set
            {
                _participants = value;
                RaisePropertyChanged(() => Participants);
            }
        }

        private string _initalParticipantsString = "";

        public RepParticipantEditPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(widgetArgs, parentCancellationTokenSource)
        {
            NotifyValueChanged = false;
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }
            _PartService = AppContainer.Resolve<IParticipantService>();
        }

        public async override ValueTask<bool> InitializeControl()
        {
            var result = await base.InitializeControl();
            await _PartService.PrepareContentAsync(_cancellationTokenSource.Token);
            var listParticipants = await _PartService.BuildRepParticipants(Data, _cancellationTokenSource.Token);
            Participants = new ObservableCollection<ParticipantData>(listParticipants);
            _initalParticipantsString = ProcessedParisipentData();
            RepList = await _PartService.GetRepList(_cancellationTokenSource.Token);
            return result;
        }

        public override PanelData ProcessedData
        {
            get
            {
                if (Data == null || Data.Fields.Count == 0)
                {
                    return Data;
                }
                else
                {
                    var field = Data.Fields[0];
                    field.EditData.StringValue = ProcessedParisipentData();
                    field.EditData.DefaultStringValue = _initalParticipantsString;
                    field.EditData.ChangeOfflineRequest = new OfflineRecordField()
                    {
                        FieldId = field.Config.FieldConfig.FieldId,
                        NewValue = field.EditData.StringValue,
                        OldValue = field.EditData.DefaultStringValue,
                        Offline = 0
                    };
                    var processedData = new PanelData(Data);
                    processedData.Fields = new List<ListDisplayField> { field };
                    return processedData;
                }

            }
        }

        private string ProcessedParisipentData()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Participants)
            {
                stringBuilder.Append(item.RepParticipantString);
                stringBuilder.Append(';');
            }

            return stringBuilder.ToString();
        }

    }
}
