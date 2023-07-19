using ACRM.mobile.Domain.Application.ContactTimes;
using ACRM.mobile.ViewModels.Base;
using Syncfusion.SfDataGrid.XForms;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.DotNet.PlatformAbstractions;

namespace ACRM.mobile.UIModels
{
    public class ContactTimesModel : UIWidget
    {
        public ICommand ItemsSourceChangedCommand => new Command<GridItemsSourceChangedEventArgs>((args) => OnItemsSourceChanged(args));
        public ICommand QueryCellStyleCommand => new Command<QueryCellStyleEventArgs>((args) => OnQueryCellStyleChanged(args));

        private DataGridStyle _dataGridStyle;
        public DataGridStyle DataGridStyle
        {
            get => _dataGridStyle;
            set
            {
                _dataGridStyle = value;
                RaisePropertyChanged(() => DataGridStyle);
            }
        }

        private Columns _columns;
        public Columns Columns
        {
            get => _columns;
            set
            {
                _columns = value;
            }
        }

        private List<ContactTimesDataGridEntry> _contactTimesDataGridEntryList = new List<ContactTimesDataGridEntry>();

        private ObservableCollection<ContactTimesDataGridEntry> _contactTimesDataGridEntries = new ObservableCollection<ContactTimesDataGridEntry>();
        public ObservableCollection<ContactTimesDataGridEntry> ContactTimesDataGridEntries
        {
            get => _contactTimesDataGridEntries;
            set
            {
                _contactTimesDataGridEntries = value;
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        private double _gridHeightRequest;
        public double GridHeightRequest
        {
            get => _gridHeightRequest;
            set
            {
                _gridHeightRequest = value;
                RaisePropertyChanged(() => GridHeightRequest);
            }
        }

        private double _gridHeaderRowHeightRequest;
        public double GridHeaderRowHeightRequest
        {
            get => _gridHeaderRowHeightRequest;
            set
            {
                _gridHeaderRowHeightRequest = value;
                RaisePropertyChanged(() => GridHeaderRowHeightRequest);
            }
        }

        private double _gridRowHeightRequest;
        public double GridRowHeightRequest
        {
            get => _gridRowHeightRequest;
            set
            {
                _gridRowHeightRequest = value;
                RaisePropertyChanged(() => GridRowHeightRequest);
            }
        }

        public ContactTimesModel(List<ContactTimesDataGridEntry> contactTimesDataGridEntries, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contactTimesDataGridEntryList.AddRange(contactTimesDataGridEntries);
        }

        private void InitProperties()
        {
            GridHeaderRowHeightRequest = 40;
            GridRowHeightRequest = 50;
            if(Device.RuntimePlatform == Device.UWP)
            {
                GridHeightRequest = 40;
            }
        }

        private void InitDataGridStyle()
        {
            DataGridStyle = new DataGridStyle()
            {
                GridCellBorderWidth = 0,
                HeaderCellBorderWidth = 0,
                HeaderCellBorderColor = Color.Transparent,
                SelectionBackgroundColor = Color.DarkGray,
                SelectionForegroundColor = Color.Black,
                CurrentCellBorderWidth = 0,
                HeaderBackgroundColor = Color.White
            };
        }

        private void InitColumns(ContactTimesDataGridEntry contactTimesDataGridEntry)
        {
            Columns columns = new Columns
            {
                new GridTextColumn()
                {
                    HeaderText = " ",
                    MappingName = $"WeekDayName"
                }
            };

            foreach (string contactTimesTypeName in contactTimesDataGridEntry.OrderedContactTimesTypeNames)
            {
                columns.Add(new GridTextColumn()
                {
                    HeaderText = contactTimesTypeName,
                    MappingName = $"TypeNameTimeIntervalsStringDict[{contactTimesTypeName}]"
                });
            }

            Columns = columns;
        }

        private void SetSelectedIndex(List<ContactTimesDataGridEntry> contactTimesDataGridEntries)
        {
            for (int i = 0; i < contactTimesDataGridEntries.Count; i++)
            {
                if (contactTimesDataGridEntries[i].IsToday())
                {
                    SelectedIndex = i + 1;
                    break;
                }
            }
        }

        // Any ItemSource changes of the SfDataGrid need to be explicitly invoked on the UI Thread.
        // Reference: https://www.syncfusion.com/feedback/25597/error-collection-was-modified-enumeration-operation-may-not-execute-using-xf-5-0
        public override async ValueTask<bool> InitializeControl()
        {
            if (_contactTimesDataGridEntryList.Count > 0)
            {
                InitProperties();
                InitDataGridStyle();
                InitColumns(_contactTimesDataGridEntryList[0]);
                Device.BeginInvokeOnMainThread(() => {
                    foreach (ContactTimesDataGridEntry contactTimesDataGridEntry in _contactTimesDataGridEntryList)
                    {
                        _contactTimesDataGridEntries.Add(contactTimesDataGridEntry);
                    }
                    ContactTimesDataGridEntries = _contactTimesDataGridEntries;
                    SetSelectedIndex(_contactTimesDataGridEntryList);
                });
            }
            return true;
        }

        // Any HeighRequest changes of the SfDataGrid need to be explicitly invoked on the UI Thread.
        // Reference: https://www.syncfusion.com/feedback/25597/error-collection-was-modified-enumeration-operation-may-not-execute-using-xf-5-0
        private void OnItemsSourceChanged(GridItemsSourceChangedEventArgs args)
        {
            if (args.NewItemSource != null && args.NewItemSource is ObservableCollection<ContactTimesDataGridEntry> contactTimesDataGridEntries)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    GridHeightRequest = GridRowHeightRequest * contactTimesDataGridEntries.Count + GridHeaderRowHeightRequest;
                });
            }
        }

        private void OnQueryCellStyleChanged(QueryCellStyleEventArgs args)
        {
            if (args.Column.MappingName == "WeekDayName")
            {
                args.Style.BackgroundColor = Color.LightGray;
                args.Style.ForegroundColor = Color.DimGray;
            }
            args.Handled = true;
        }
    }
}
