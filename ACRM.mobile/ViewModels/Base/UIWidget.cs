using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels.Base
{
    public abstract class UIWidget : BaseViewModel
    {
        public FormItemData WidgetConfig { get; set; }
        public string WidgetKey { get; set; }
        public bool NotifyValueChanged { get; set; }
        public bool lazyInitialize { get; set; } = false;

        private bool _isDirtyStateSet = false;

        public UIWidget(CancellationTokenSource parentCancellationTokenSource)
        {
            _cancellationTokenSource = parentCancellationTokenSource;
        }

        public abstract ValueTask<bool> InitializeControl();
        public virtual async Task LazyInitializeControl()
        {

        }
        public ICommand DataChangedEvent => new Command(() =>
        {
            NotifyDirtyState();
        });

        protected void NotifyDirtyState()
        {
            if (!_isDirtyStateSet)
            {
                _isDirtyStateSet = true;
                new Action(async () => await PublishMessage(new WidgetMessage
                {
                    ControlKey = "UIDataChanged",
                    Data = true,
                    EventType = WidgetEventType.UIDataChanged
                }, MessageDirections.ToParent))();
            }
        }
        public virtual void CancelChilds()
        {

        }
    }
}
