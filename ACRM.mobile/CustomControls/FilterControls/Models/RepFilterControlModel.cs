using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class RepFilterControlModel: CatalogFilterControlModel
    {
        private readonly IRepService _repComponent;
        public RepFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
            _repComponent = AppContainer.Resolve<IRepService>();
            
        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (Filter.FieldInfo != null)
            {
                CatalogItems = new List<FilterCatalogItem>();
                if (Filter.FilterData == null)
                {
                    var Reps = await _repComponent.GetAllCrmReps(_cancellationTokenSource.Token);
                    if (Reps != null && Reps.Count > 0)
                    {
                        foreach (var item in Reps)
                        {
                            CatalogItems.Add(new FilterCatalogItem
                            {
                                CatalogItem = new SelectableFieldValue
                                {
                                    DisplayValue = item.Name,
                                    RecordId = item.Id
                                }
                            });
                        }
                    }
                }
                else if (Filter.FilterData is List<FilterCatalogItem> catalogItems)
                {
                    foreach (var item in catalogItems)
                    {
                        CatalogItems.Add(item);
                    }
                }

                await PerformAsyncSearch();

            }
            return true;
        }
    }
}
