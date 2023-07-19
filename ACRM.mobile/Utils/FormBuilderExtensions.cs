using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.UIModels;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.Utils
{
    public static class FormBuilderExtensions
    {
        public static async Task<ObservableCollection<UIWidget>> BuildFormWidgets (this FormTab formTab, BaseViewModel parentBaseModel, Dictionary<string, Dictionary<string, string>> formParams, UserAction userAction, CancellationTokenSource parentCancellationTokenSource)
        {
            ObservableCollection<UIWidget> Widgets = new ObservableCollection<UIWidget>();

            if (formTab != null)
            {
                foreach(FormRow formRow in formTab.Rows.OrderBy(a=>a.OrderId))
                {
                    foreach(FormItem formItem in formRow.Items.OrderBy(a => a.OrderId))
                    {
                        FormItemData formItemData = new FormItemData()
                        {
                            Action = userAction,
                            FormItem = formItem,
                            FormParams = formParams
                        };
                        var widget = await BuildWidget($"Form${ formItem.ControlName}", formItemData, parentBaseModel, parentCancellationTokenSource);
                        if (widget != null)
                        {
                            Widgets.Add(widget);
                        }
                    }
                }
            }

            return Widgets;
        }

        public static async Task<ObservableCollection<UIWidget>> BuildWidgetsAsyc(this List<PanelData> panelDatas, BaseViewModel parentBaseModel, CancellationTokenSource parentCancellationTokenSource, bool skipEmptyPanels = false)
        {
            ObservableCollection<UIWidget> Widgets = new ObservableCollection<UIWidget>();

            if (panelDatas != null)
            {
                foreach (PanelData panels in panelDatas)
                {
                    var widget = await BuildWidgetAsync(panels.Type, panels, parentBaseModel, parentCancellationTokenSource);
                    if (widget != null && widget is UIPanelWidget pWidget)
                    {
                        if (!skipEmptyPanels || pWidget.HasData)
                        {
                            Widgets.Add(widget);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

            return Widgets;
        }

        public static async Task<UIWidget> BuildWidgetAsync(PanelType type, object widgetArgs, BaseViewModel parentBaseModel, CancellationTokenSource parentCancellationTokenSource)
        {
            UIWidget widget = BuildWidgetFromType(type, widgetArgs, parentCancellationTokenSource);

            if (widget != null)
            {
                widget.ParentBaseModel = parentBaseModel;
                await widget.InitializeControl();
            }

            return widget;
        }

        private static UIWidget BuildWidgetFromType(PanelType type, object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
        {
            UIWidget widget = null;
            switch (type)
            {
                case PanelType.Map:
                    widget = new MapControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Participants:
                    widget = new ParticipantPanelControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Parent:
                    widget = new ParentPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Repparticipant:
                    widget = new RepParticipantEditPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Linkparticipant:
                    widget = new LinkParticipantEditPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.NotSupported:
                    widget = new UnsupportedTypeModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Children:
                    widget = new ChildRecordsModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.WebView:
                    widget = new WebControlPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Insightboard:
                    widget = new InsightBoardModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.EditPanel:
                    widget = new EditPanelControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.EditPanelChildren:
                    widget = new EditChildPanelControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.SerialEntryEditPanel:
                    widget = new SerialEntryEditPanelControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Doc:
                    widget = new DocumentsPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.Characteristics:
                    widget = new CharacteristicsPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case PanelType.ContactTimes:
                    widget = new ContactTimesPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                default:
                    widget = new PanelControlModel(widgetArgs, parentCancellationTokenSource);
                    break;

            }

            return widget;
        }
        
        public static async Task<UIWidget> BuildWidget(string widgetKey, object widgetArgs, BaseViewModel parentBaseModel, CancellationTokenSource parentCancellationTokenSource)
        {
            UIWidget widget = null;
            switch (widgetKey)
            {
                case "ShowRecord":
                    widget = new RecordViewDetailsModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "Form$DatePicker":
                    widget = new DashboardCalenderModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "Form$RecordList":
                    widget = new FormRecordListModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "Form$InsightBoard":
                case "InsightBoard":
                    widget = new InsightBoardModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "NotImplemented":
                    widget = new UnsupportedTypeModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "DocumentView":
                    widget = new DocumentViewModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "RecordList":
                case "RecordLists":
                    widget = new RecordListModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "Calendar":
                    widget = new CalendarControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "WebContentView":
                    widget = new WebControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "PDFViewer":
                    widget = new PdfViewerControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "ClientReportWithAction":
                case "ClientReport":
                    widget = new ClientReportViewModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "Report":
                    widget = new CoreReportViewModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "SerialEntryListing":
                    widget = new SerialEntryControlModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "QuestionnaireEdit":
                    widget = new QuestionnaireEditModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "Query":
                    widget = new QueryViewModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "ConfigPanel":
                    widget = new ConfigPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "UserConfigPanel":
                    widget = new UserConfigPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "ConfigEditPanel":
                    widget = new ConfigEditPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;
                case "UserConfigEditPanel":
                    widget = new UserConfigEditPanelModel(widgetArgs, parentCancellationTokenSource);
                    break;

            }

            if(widget != null)
            {
                if (widgetArgs is FormItemData)
                {
                    widget.WidgetConfig = widgetArgs as FormItemData;
                }
                widget.ParentBaseModel = parentBaseModel;
                await widget.InitializeControl();
                if (widget.lazyInitialize)
                {
                    _ = Task.Run(async () => await widget.LazyInitializeControl());
                }
            }
            return widget;
        }
    }
}
