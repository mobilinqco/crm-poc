using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class ParentPanelModel : UIPanelWidget
    {
        private readonly IDetailsContentService _contentService;
        private PanelData _inputArgs;

        public ParentPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<IDetailsContentService>();
            if (widgetArgs is PanelData)
            {
                _inputArgs = widgetArgs as PanelData;
            }

        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (_inputArgs != null)
            {
                _contentService.SetSourceAction(_inputArgs.action);
                Data = await _contentService.PreparePanelDataAsync(_inputArgs, _cancellationTokenSource.Token);
                HasData = Data.HasData();
            }
            return true;
        }
    }
}
