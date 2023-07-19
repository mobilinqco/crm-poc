using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class TextFilterControlModel : BaseFilterControlModel
    {
        public TextFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
        }
    }
}
