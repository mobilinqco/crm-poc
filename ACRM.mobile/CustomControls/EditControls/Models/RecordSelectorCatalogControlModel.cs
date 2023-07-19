using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class RecordSelectorCatalogControlModel : CatalogControlModel
    {
        private SelectedRecordFieldData SelectedData;
        public ICommand RecordSelectorEntryCommand => new Command<ListDisplayField>(async (field) => await OnRecordSelectorEntry(field));

        private async Task OnRecordSelectorEntry(ListDisplayField field)
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

        internal override async Task SetSelectedResultEventHandler(WidgetMessage arg)
        {
            if (arg.Data != null && arg.Data is SelectedRecordFieldData selectedData)
            {
                var selectKey = selectedData.StringValue;
                SelectedData = selectedData;
                if (Field.Config.FieldConfig.InfoAreaId.Equals(selectedData.InfoAreaId))
                {
                    SelectedValue = selectedData.SelectedValue;
                    _logService.LogDebug($"Setting the value {selectedData.StringValue} for {Field.Config.FieldConfig.Function} ");
                }
                else
                {
                    var selectItem = AllowedValues?.Where(a => a.DisplayValue.Equals(selectKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    SelectedValue = selectItem;
                    _logService.LogDebug($"Setting the value {selectKey} for {Field.Config.FieldConfig.Function} ");
                }
                StringValue = selectKey;
            }
            else
            {
                SelectedData = null;
            }

        }

        public override string StringValue
        {
            get => Field?.EditData?.SelectedValue?.DisplayValue;
        }

        public override object ChangeOfflineRequest
        {
            get
            {
                if (Field == null || SelectedData == null || SelectedData.SelectedValue == null)
                {
                    return null;
                }

                if (Field.Config.FieldConfig.InfoAreaId.Equals(SelectedData.InfoAreaId))
                {
                    return new OfflineRecordLink()
                    {
                        InfoAreaId = Field.Config.FieldConfig.InfoAreaId,
                        LinkId = Field.Config.FieldConfig.LinkId,
                        RecordId = SelectedData.SelectedValue.RecordId
                    };
                }

                if(Field.Config.PresentationFieldAttributes.FieldInfo.IsCatalog)
                {
                    return new OfflineRecordField()
                    {
                        FieldId = Field.Config.FieldConfig.FieldId,
                        NewValue = Field.EditData.SelectedValue?.RecordId,
                        OldValue = Field.EditData.DefaultSelectedValue?.RecordId
                    };
                }
                
                return base.ChangeOfflineRequest;
            }
        }

        public RecordSelectorCatalogControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
        }
    }
}
