using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class
        RecordViewDetailsModel : UIWidget
    {
        private readonly IDetailsContentService _contentService;
        private readonly UserAction _userAction;

        private ObservableCollection<UIWidget> _panels = new ObservableCollection<UIWidget>();
        public ObservableCollection<UIWidget> Panels
        {
            get => _panels;
            set
            {
                _panels = value;
                RaisePropertyChanged(() => Panels);
            }
        }

        public RecordViewDetailsModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<IDetailsContentService>();
            if (widgetArgs !=null  && widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
                _contentService.SetSourceAction(_userAction);
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (_contentService != null)
            {
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                var items = await _contentService.LoadPanelsContentAsync(_cancellationTokenSource.Token);

                if (items != null && items.Count > 0)
                {

                    Panels = await items.BuildWidgetsAsyc(this, _cancellationTokenSource, !_contentService.DisplayEmptyPanels());
                }
            }
            return true;
        }

        public override void CancelChilds()
        {
            if(Panels != null)
            {
                foreach(var panel in Panels)
                {
                    panel.CancelChilds();
                    panel.Cancel();
                }
            }
        }
    }
}
