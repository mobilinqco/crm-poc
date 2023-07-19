using System;
using System.Collections.Generic;
using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.UIModels;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class DashboardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DashboardCalender { get; set; }
        public DataTemplate EditControlTemplate { get; set; }
        public DataTemplate SerialEntryEditPanelTemplate { get; set; }
        public DataTemplate ParticipentEditPanelTemplate { get; set; }
        public DataTemplate LinkParticipentEditPanelTemplate { get; set; }
        public DataTemplate InsightBoard { get; set; }
        public DataTemplate FieldPanel { get; set; }
        public DataTemplate MapControl { get; set; }
        public DataTemplate UnsupportedType { get; set; }
        public DataTemplate RecordList { get; set; }
        public DataTemplate ParticipantPanel { get; set; }
        public DataTemplate RecordViewDetailsPanel { get; set; }
        public DataTemplate ChildRecordsPanel { get; set; }
        public DataTemplate CalendarControl { get; set; }
        public DataTemplate CalendarScheduleControl { get; set; }

        public DataTemplate CalendarListControl { get; set; }
        public DataTemplate WebViewControl { get; set; }
        public DataTemplate WebViewPanelControl { get; set; }
        public DataTemplate SerialEntryListingControl { get; set; }
        public DataTemplate DocPanelControl { get; set; }
        public DataTemplate CharacteristicsPanel { get; set; }
        public DataTemplate DocumentViewControl { get; set; }
        public DataTemplate ClientReportControl { get; set; }
        public DataTemplate QuestionnaireEditControl { get; set; }
        public DataTemplate PDFViewerControl { get; set; }
        public DataTemplate ContactTimesPanelControl { get; set; }
        public DataTemplate ContactTimeModelControl { get; set; }
        public DataTemplate QueryViewControl { get; set; }
        public DataTemplate CoreReportControl { get; set; }
        public DataTemplate ConfigPanelControl { get; set; }
        public DataTemplate ConfigEditPanelControl { get; set; }
        public DataTemplate EditChildPanelTemplate { get; set; }
        

        public DashboardTemplateSelector()
        {

        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item is DashboardCalenderModel)
            {
                return DashboardCalender;
            }
            else if(item is InsightBoardModel)
            {
                return InsightBoard;
            }
            else if (item is MapControlModel)
            {
                return MapControl;
            }
            else if (item is PanelControlModel || item is ParentPanelModel)
            {
                return FieldPanel;
            }
            else if (item is RepParticipantEditPanelModel)
            {
                return ParticipentEditPanelTemplate;
            }
            else if(item is LinkParticipantEditPanelModel)
            {
                return LinkParticipentEditPanelTemplate;
            }
            else if (item is SerialEntryEditPanelControlModel)
            {
                return SerialEntryEditPanelTemplate;
            }
            else if (item is EditChildPanelControlModel)
            {
                return EditChildPanelTemplate;
            }
            else if (item is EditPanelControlModel)
            {
                return EditControlTemplate;
            }
            else if (item is UnsupportedTypeModel)
            {
                return UnsupportedType;
            }
            else if (item is RecordListModel)
            {
                return RecordList;
            }
            else if (item is ParticipantPanelControlModel)
            {
                return ParticipantPanel;
            }
            else if (item is RecordViewDetailsModel)
            {
                return RecordViewDetailsPanel;
            }
            else if (item is DocumentViewModel)
            {
                return DocumentViewControl;
            }
            else if (item is ClientReportViewModel)
            {
                return ClientReportControl;
            }
            else if (item is CoreReportViewModel)
            {
                return CoreReportControl;
            }
            else if (item is ChildRecordsModel)
            {
                return ChildRecordsPanel;
            }
            else if (item is CalendarControlModel)
            {
                return CalendarControl;
            }
            else if (item is CalendarScheduleModel)
            {
                return CalendarScheduleControl;
            }
            else if (item is CalendarListModel)
            {
                return CalendarListControl;
            }
            else if (item is WebControlModel)
            {
                return WebViewControl;
            }
            else if (item is PdfViewerControlModel)
            {
                return PDFViewerControl;
            }
            else if (item is DocumentsPanelModel)
            {
                return DocPanelControl;
            }
            else if (item is WebControlPanelModel)
            {
                return WebViewPanelControl;
            }
            else if (item is SerialEntryControlModel)
            {
                return SerialEntryListingControl;
            }
            else if (item is CharacteristicsPanelModel)
            {
                return CharacteristicsPanel;
            }
            else if (item is ContactTimesPanelModel)
            {
                return ContactTimesPanelControl;
            }
            else if (item is ContactTimesModel)
            {
                return ContactTimeModelControl;
            }
            else if (item is QuestionnaireEditModel)
            {
                return QuestionnaireEditControl;
            }
            else if (item is QueryViewModel)
            {
                return QueryViewControl;
            }
            else if (item is UserConfigEditPanelModel)
            {
                return ConfigEditPanelControl;
            }
            else if (item is ConfigEditPanelModel)
            {
                return ConfigEditPanelControl;
            }
            else if (item is UserConfigPanelModel)
            {
                return ConfigPanelControl;
            }
            else if (item is ConfigPanelModel)
            {
                return ConfigPanelControl;
            }
            
            else
            {
                return null;
            }
        }
    }
}
