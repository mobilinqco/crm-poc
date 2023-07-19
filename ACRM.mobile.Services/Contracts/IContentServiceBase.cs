using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IContentServiceBase
    {
        public event EventHandler DataReady;

        string PageTitle();
        string PageAccentColor(InfoArea infoArea = null);
        List<UserAction> HeaderRelatedInfoAreas();
        Task<List<UserAction>> HeaderButtons(CancellationToken cancellationToken);
        bool AreResultsRetrievedOnline();

        void SetSourceAction(UserAction action);
        Task PrepareContentAsync(CancellationToken cancellationToken);
        void SetAdditionalParams(Dictionary<string, string> additionalParams);
        List<UserAction> HeaderButtons();
        string InfoAreaId
        {
            get;
        }
    }
}
