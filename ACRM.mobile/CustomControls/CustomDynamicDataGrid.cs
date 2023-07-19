using System;
using System.Collections.ObjectModel;
using ACRM.mobile.Domain.Application;
using Syncfusion.SfDataGrid.XForms;

namespace ACRM.mobile.CustomControls
{
    public class CustomDynamicDataGrid: SfDataGrid
    {
        public CustomDynamicDataGrid()
        {
            ItemsSourceChanged += (sender, args) => OnItemsSourceChanged(sender, args);
        }

        private void OnItemsSourceChanged(object sender, GridItemsSourceChangedEventArgs gridItemsSourceChangedEventArgs)
        {
            if (sender is CustomDynamicDataGrid customDynamicDataGrid && gridItemsSourceChangedEventArgs.NewItemSource is ObservableCollection<DynamicStringModel> newItemSource)
            {
                customDynamicDataGrid.Columns.Clear();

                if (newItemSource.Count > 0)
                {
                    DynamicStringModel model = newItemSource[0];

                    foreach (string columnName in model.Values.Keys)
                    {
                        customDynamicDataGrid.Columns.Add(new GridTextColumn()
                        {
                            HeaderText = columnName,
                            MappingName = $"Values[{columnName}]"
                        });
                    }
                }

                customDynamicDataGrid.Refresh();
            }
        }
    }
}
