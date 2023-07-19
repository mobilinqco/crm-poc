using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class LinkParticipantEditPanelModel : EditPanelControlModel, IPopupItemSelectionHandler
    {
        public enum Pagepoup
        {
            Requirements,
            Acceptance,
            InfoAreaSelect

        }
        private List<ListDisplayField> SearchFields;
        private readonly IParticipantService _PartService;
        private ObservableCollection<ParticipantData> _participants;
        public ParticipantData SelectedParticipantData { get; set; }
        public Pagepoup PopUpKey { get; set; }
        public ObservableCollection<ParticipantData> Participants
        {
            get => _participants;
            set
            {
                _participants = value;
                RaisePropertyChanged(() => Participants);
            }
        }
        public ICommand AcceptanceTappedCommand => new Command<ParticipantData>(async (item) =>
        {
            if (item != null)
            {
                SelectedParticipantData = item;
                PopUpKey = Pagepoup.Acceptance;
                if (item?.Acceptance != null)
                {
                    await _navigationController.NavigateToAsync<PopupListPageViewModel>(parameter: this);
                }
            }
        });
        public ICommand RequirementTappedCommand => new Command<ParticipantData>(async (item) =>
        {
            if (item != null)
            {
                SelectedParticipantData = item;
                PopUpKey = Pagepoup.Requirements;
                if (item?.Requirements != null)
                {
                    await _navigationController.NavigateToAsync<PopupListPageViewModel>(parameter: this);
                }
            }
        });

        public ICommand AddParticipantCommand => new Command(async () => await AddParticipant());
        public ICommand RemoveParticipantCommand => new Command<ParticipantData>(async (participant) => await RemoveSelectedParticipantData(participant));

        private async Task RemoveSelectedParticipantData(ParticipantData participant)
        {
            if (participant != null)
            {
                NotifyDirtyState();    
                Participants.Remove(participant);
                if (!string.IsNullOrEmpty(participant.Key))
                {
                    await _PartService.ChildEditServic.Delete(participant.Key, _cancellationTokenSource.Token);
                }
            }
        }

        private async Task AddParticipant()
        {
            if (SearchFields.Count > 1)
            {
                NotifyDirtyState();
                PopUpKey = Pagepoup.InfoAreaSelect;
                await _navigationController.NavigateToAsync<PopupListPageViewModel>(parameter: this);
            }
        }

        public LinkParticipantEditPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
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
            var listParticipants = await _PartService.BuildLinkParticipants(Data, _cancellationTokenSource.Token);
            Participants = new ObservableCollection<ParticipantData>(listParticipants);
            LoadSearchItems();
            EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.SaveChild, string.Empty, SaveLinkParticipant));
            EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.LinkParticipantSelected, "LinkParticipant", SetSelectedLinkParticipant));
            return result;
        }

        private async Task SetSelectedLinkParticipant(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is ListDisplayRow recordRow)
            {
                var name = getFirstNoneEmptyData(recordRow);
                var panel = await _PartService.ChildEditServic.GetPanelAsync(null, _cancellationTokenSource.Token);
                var Participant = new ParticipantData(name, "", "0", "0", recordRow.RecordId, recordRow?.RowDecorators.Expand?.InfoAreaId);
                Participant.Acceptance = _PartService.Acceptance;
                Participant.Requirements = _PartService.Requirements;
                if (panel != null)
                {
                    Participant.Panels = new List<PanelData>();
                    Participant.Panels.Add(panel);
                }
                Participants.Add(Participant);
            }
        }

        private async Task SaveLinkParticipant(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string parentRecordId)
            {
                var ParentLink = new OfflineRecordLink
                {
                    LinkId = 0,
                    InfoAreaId = Data.RecordInfoArea,
                    RecordId = parentRecordId
                };

                foreach (var Part in Participants)
                {
                    var widgets = await Part.Panels.BuildWidgetsAsyc(null, _cancellationTokenSource);
                    var Links = new List<OfflineRecordLink> { new OfflineRecordLink
                    {
                    LinkId = 0,
                    InfoAreaId = Part.LinkInfoAreaID,
                    RecordId = Part.LinkRecordId
                      }, ParentLink };
                    widgets.SetSelectedIndex(_PartService.RequirementFieldInfo, Part.SelectedRequirementIndex);
                    widgets.SetSelectedIndex(_PartService.AcceptanceFieldInfo, Part.SelectedAcceptanceIndex);
                    var inputPanels = widgets.GetPanelDatas();
                    if (string.IsNullOrEmpty(Part.Key) || inputPanels.HasChanges())
                    {
                        var cresult = await _PartService.ChildEditServic.Save(inputPanels, _cancellationTokenSource.Token, Part.Key, Links);

                        if (cresult != null && string.IsNullOrWhiteSpace(Part.Key))
                        {
                            if (cresult.SavedRecord != null && !string.IsNullOrWhiteSpace(cresult.SavedRecord.RecordId))
                            {
                                Part.Key = cresult.SavedRecord.RecordId;
                            }
                        }
                    }
                }
            }
        }

        private string getFirstNoneEmptyData(ListDisplayRow recordRow)
        {
            foreach(var field in recordRow.Fields)
            {
                if(!string.IsNullOrWhiteSpace(field.Data.StringData))
                {
                    return field.Data.StringData;
                }
            }
            return string.Empty;
        }

        private void LoadSearchItems()
        {
            SearchFields = new List<ListDisplayField>();
            if (Data?.Fields.Count > 0)
            {
                foreach (var field in Data.Fields)
                {
                    if (field.Config.RecordSelectorAction != null)
                    {
                        SearchFields.Add(field);
                    }

                }
            }
        }

        public async Task<List<PopupListItem>> GetPoupList()
        {

            if (SelectedParticipantData != null && PopUpKey == Pagepoup.Requirements)
            {
                return SelectedParticipantData.Requirements;
            }
            else if (SelectedParticipantData != null && PopUpKey == Pagepoup.Acceptance)
            {
                return SelectedParticipantData.Acceptance;
            }
            else if (PopUpKey == Pagepoup.InfoAreaSelect)
            {
                if (SearchFields.Count > 0)
                {
                    var items = new List<PopupListItem>();
                    foreach (var field in SearchFields)
                    {
                        var fieldText = await _PartService.GetFieldText(field, _cancellationTokenSource.Token);
                        if (!string.IsNullOrEmpty(fieldText))
                        {
                            items.Add(new PopupListItem
                            {
                                RecordId = field.Config.FieldConfig.FieldId.ToString(),
                                DisplayText = fieldText,
                                OrginalObject = field
                            });
                        }
                    }
                    return items;
                }
            }
            return null;
        }

        public async Task PopupItemSelected(PopupListItem item)
        {
            if (item != null)
            {

                if (SelectedParticipantData != null && PopUpKey == Pagepoup.Requirements)
                {
                    var intex = SelectedParticipantData.Requirements.FindIndex(i => i.RecordId.Equals(item.RecordId, StringComparison.InvariantCultureIgnoreCase));
                    if (intex > -1)
                    {
                        SelectedParticipantData.SelectedRequirementIndex = intex;
                    }

                }
                else if (SelectedParticipantData != null && PopUpKey == Pagepoup.Acceptance)
                {
                    var intex = SelectedParticipantData.Acceptance.FindIndex(i => i.RecordId.Equals(item.RecordId, StringComparison.InvariantCultureIgnoreCase));
                    if (intex > -1)
                    {
                        SelectedParticipantData.SelectedAcceptanceIndex = intex;
                    }

                }
                else if (PopUpKey == Pagepoup.InfoAreaSelect)
                {
                    var field = item.OrginalObject as ListDisplayField;
                    var message = new WidgetMessage();
                    message.EventType = WidgetEventType.RecordSelectorTapped;
                    message.Data = field;
                    message.ControlKey = "LinkParticipant";
                    if (ParentBaseModel != null)
                    {
                        await ParentBaseModel?.PublishMessage(message);
                    }

                }

            }
        }

        public override PanelData ProcessedData
        {
            get
            {
                return null;

            }
        }
    }
}
