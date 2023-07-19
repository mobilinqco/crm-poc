using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Utils;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class RecordSelectorControlModel : BaseEditControlModel
    {
        public ICommand RecordSelectorEntryCommand => new Command<ListDisplayField>(async (field) => await OnRecordSelectorEntry(field));

        private async Task OnRecordSelectorEntry(ListDisplayField field)
        {
            if (!field.Config.PresentationFieldAttributes.ReadOnly)
            {
                var message = new WidgetMessage();
                message.EventType = WidgetEventType.RecordSelectorTapped;
                message.Data = field;
                message.ControlKey = "EditRecordSelectorTapped";
                if (ParentBaseModel != null)
                {
                    await ParentBaseModel?.PublishMessage(message);
                }
            }
        }

        public override object ChangeOfflineRequest
        {
            get
            {
                if (Field == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(Field.EditData.SelectedValue?.RecordId))
                {
                    if (Field.EditData.HasStringChanged)
                    {
                        return new OfflineRecordField()
                        {
                            FieldId = Field.Config.FieldConfig.FieldId,
                            NewValue = Field.EditData.StringValue,
                            OldValue = Field.EditData.DefaultStringValue,
                            Offline = 0
                        };
                    }

                    return null;
                }

                return new OfflineRecordLink()
                {
                    InfoAreaId = Field.Config.FieldConfig.InfoAreaId,
                    LinkId = Field.Config.FieldConfig.LinkId,
                    RecordId = Field.EditData.SelectedValue.RecordId
                };

            }
        }

        public RecordSelectorControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
            string parentInfoArea = field.Config?.RecordSelectorAction?.RecordSelector?.ParentLink();
            if (!string.IsNullOrWhiteSpace(parentInfoArea))
            {
                EventSubscriptions.Add(new WidgetEventSubscription(WidgetEventType.SetLinkedRecordSelectorParentId, parentInfoArea, SetParentRecordId));
            }
        }

        private async Task SetParentRecordId(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is string selectedRecordId)
            {
                Field.Config.RecordSelectorAction.RecordId = selectedRecordId;
                Field.Config.RecordSelectorAction.SourceInfoArea = arg.ControlKey;
            }
        }
    }
}
