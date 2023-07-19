using System;
using ACRM.mobile.Utils;
using Syncfusion.SfSchedule.XForms;

namespace ACRM.mobile.CustomControls
{
    public class CustomSchedule: SfSchedule
    {

        public CustomSchedule()
        {
            CellTapped += OnCellTapped;
        }

        private void OnCellTapped(object sender, CellTappedEventArgs args)
        {
            if (sender is SfSchedule sfSchedule)
            {
                if (sfSchedule.ScheduleView != ScheduleView.MonthView)
                {
                    if(args.Appointment is CrmScheduleAppointment appointment)
                    {
                        sfSchedule.SelectedDate = new DateTime(appointment.StartTime.Year,
                            appointment.StartTime.Month, appointment.StartTime.Day, appointment.StartTime.Hour, 0, 0);
                    }
                    else
                    {
                        sfSchedule.SelectedDate = args.Datetime;
                    }
                }
                else
                {
                    sfSchedule.SelectedDate = args.Datetime;
                    sfSchedule.NavigateTo(args.Datetime);
                }
            }
        }
    }
}
