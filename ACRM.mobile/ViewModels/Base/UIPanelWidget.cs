using System;
using System.Threading;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.ViewModels.Base
{
    public abstract class UIPanelWidget : UIWidget
    {
        public UIPanelWidget(CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {

        }

        public PanelData Data { get; set; }
        public virtual PanelData ProcessedData
        {
            get => Data;

        }

        private bool _hasData = true;
        public bool HasData
        {
            get => _hasData;
            set
            {
                _hasData = value;
                RaisePropertyChanged(() => HasData);
            }
        }
    }
}
