using ACRM.mobile.CustomControls.EditControls.Models;
using ACRM.mobile.Domain.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    internal class EditChildPanelControlModel : EditPanelControlModel
    {
        public ICommand ToggleSwitchCommand => new Command(async async => await ToggleSwitch());

        private async Task ToggleSwitch()
        {
            NotifyDirtyState();
            EnableChild = !EnableChild;
        }

        private bool _enableChild = false;
        public bool EnableChild
        {
            get => _enableChild;
            set
            {
                _enableChild = value;
                RaisePropertyChanged(() => EnableChild);
            }
        }
        public EditChildPanelControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource) : base(widgetArgs, parentCancellationTokenSource)
        {
        }

        public override PanelData ProcessedData
        {
            get
            {
                if (!EnableChild)
                {
                    return null;
                }
                else
                {
                    return base.ProcessedData;
                }

            }
        }
    }
}
