using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.UIModels;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.Utils
{
    public static class PanelExtension
    {
        public static void SetSelectedIndex(this ObservableCollection<UIWidget> panelwidgets, FieldInfo fieldInfo, int selectedIndex)
        {
            if (panelwidgets == null)
                return;
            foreach (var panelWidget in panelwidgets)
            {
                panelWidget?.PublishMessage(new WidgetMessage
                {
                    EventType = WidgetEventType.SetSelectedIndex,
                    ControlKey = fieldInfo.FieldId.ToString(),
                    Data = selectedIndex
                }, MessageDirections.ToChildren);
               
            }

        }

        public static List<PanelData> GetPanelDatas(this ObservableCollection<UIWidget> panelDatas)
        {
            var resultPanels = new List<PanelData>();
            if (panelDatas == null)
                return resultPanels;
            foreach (var panel in panelDatas)
            {
                if (panel != null && panel is UIPanelWidget panelWidget)
                {
                    var data = panelWidget.ProcessedData;
                    if (data != null && data is PanelData plData)
                    {
                        resultPanels.Add(plData);
                    }
                }

            }

            return resultPanels;
        }


        public static async Task UpdateField(this ObservableCollection<UIWidget> widgets, string function, object value)
        {
            if (widgets != null && widgets.Count > 0)
            {
                foreach (var widget in widgets)
                {
                    if (widget is SerialEntryEditPanelControlModel panelCtrl)
                    {
                        await panelCtrl.SetValue(function, value.ToString());
                    }
                }
            }
        }

        public static async Task UpdateField(this UIWidget widget, string function, object value)
        {
            if (widget!=null && widget is SerialEntryEditPanelControlModel panelCtrl)
            {
                await panelCtrl.SetValue(function, value.ToString());
            }
        }

        internal static void RegisterWidgetEvents(this ObservableCollection<UIWidget> widgets, WidgetEventType eventType, string controlKey, Func<WidgetMessage, Task> messageHandler)
        {
            if (widgets != null && widgets.Count > 0)
            {
                foreach (var widget in widgets)
                {
                    if (!widget.EventSubscriptions.Any(a => a.ControlKey == controlKey && a.EventType == eventType))
                    {
                        widget.EventSubscriptions.Add(new WidgetEventSubscription(eventType, controlKey, messageHandler));
                    }
                }
            }
        }
    }
}
