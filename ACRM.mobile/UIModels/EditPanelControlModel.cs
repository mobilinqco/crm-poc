using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class EditPanelControlModel : UIPanelWidget
    {
        public override PanelData ProcessedData
        {
            get
            {
                if (Data == null || Widgets.Count == 0)
                {
                    return Data;
                }
                else
                {
                    var Fields = new List<ListDisplayField>();
                    foreach (var widgetObject in Widgets)
                    {
                        if (widgetObject is BaseEditControlModel widget)
                        {
                            if (widget.Fields != null)
                            {
                                Fields.AddRange(widget.Fields);
                            }
                        }

                    }
                    var processedData = new PanelData(Data);
                    processedData.Fields = Fields;
                    return processedData;
                }

            }
        }

        public string Title { get; set; }

        public EditPanelControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            NotifyValueChanged = true;
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }

            Widgets = new ObservableCollection<UIWidget>();
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (Data ==null || Data.Fields == null || Data.Fields.Count == 0)
            {
                return false;
            }

            Title = string.IsNullOrEmpty(Data.Label) ? " " : Data.Label.ToUpper().TrimEnd('\n', '\r');

            foreach (ListDisplayField field in Data.Fields)
            {
                BaseEditControlModel ctrl = null;
                if (field.Config.PresentationFieldAttributes.Hide)
                {
                    ctrl = new BaseEditControlModel(field, _cancellationTokenSource);
                }
                else if (field.Config.PresentationFieldAttributes.IsBoolean)
                {
                    ctrl = new BoolControlModel(field, _cancellationTokenSource);
                }
                else if (field.Config.PresentationFieldAttributes.IsSelectableInput())
                {
                    if (!string.IsNullOrEmpty(field.Config.PresentationFieldAttributes.FieldInfo.ArrayFieldIndices)
                        && field.Data.ColspanData != null
                        && field.Data.ColspanData.Count > 0)
                    {
                        ctrl = new MultiSelectInputModel(field, _cancellationTokenSource);
                    }
                    else
                    {
                        if (field.Config.RecordSelectorAction != null)
                        {
                            ctrl = new RecordSelectorCatalogControlModel(field, _cancellationTokenSource);
                        }
                        else
                        {
                            ctrl = new CatalogControlModel(field, _cancellationTokenSource);
                        }
                    }
                }
                else if (field.Config.RecordSelectorAction != null)
                {
                    ctrl = new RecordSelectorControlModel(field, _cancellationTokenSource);
                }
                else if (field.HasDate() || field.HasTime())
                {
                    ctrl = new DateTimeControlModel(field, _cancellationTokenSource);

                }
                else if (field.Config.PresentationFieldAttributes.Image)
                {
                    ctrl = new ImageControlModel(field, _cancellationTokenSource);
                }
                else
                {
                    if (field.Config.PresentationFieldAttributes.MultiLine)
                    {
                        ctrl = new TextEditorControlModel(field, _cancellationTokenSource);
                    }
                    else
                    {
                        ctrl = new TextControlModel(field, _cancellationTokenSource);
                    }
                }
                if (ctrl != null)
                {
                    ctrl.ParentBaseModel = this;
                    ctrl.NotifyValueChanged = this.NotifyValueChanged;
                    if (Data.action != null && ctrl.GetType() == typeof(RecordSelectorControlModel))
                    {
                        ctrl.Field.Config.RecordSelectorAction.RecordId = Data.action.RecordId;
                        ctrl.Field.Config.RecordSelectorAction.SourceInfoArea = Data.action.SourceInfoArea;
                    }
                    await ctrl.InitializeControl();
                    Widgets.Add(ctrl);
                }

            }

            if (Data.CopyValues?.Count > 0)
            {
                foreach (var fun in Data.CopyValues.Keys)
                {
                    var value = Data.CopyValues[fun];
                    await PublishMessage(
                            new WidgetMessage
                            {
                                EventType = WidgetEventType.SetValue,
                                ControlKey = fun,
                                Data = value
                            }, MessageDirections.ToChildren);
                }
            }

            return true;
        }

        public async Task SetValue(string fun, string value)
        {
            await PublishMessage(
                            new WidgetMessage
                            {
                                EventType = WidgetEventType.SetValue,
                                ControlKey = fun,
                                Data = value
                            }, MessageDirections.ToChildren);
        }
    }
}
