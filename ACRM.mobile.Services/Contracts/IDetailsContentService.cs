using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface IDetailsContentService : IContentServiceBase
    {
        public string HeaderImageName();
        ListDisplayRow OrganizerHeaderSubText();
        ListDisplayRow HeaderTableCaptionText();
        List<PanelData> Panels();
        Task<List<PanelData>> LoadPanelsContentAsync(CancellationToken cancellationToken);
        Task LoadHeadersAsync(CancellationToken cancellationToken);
        Task<PanelData> PreparePanelDataAsync(PanelData inputArgs, CancellationToken token);
        (string imageName, string glyphText) ExtractImage(string resourceName);

        bool IsRecordFavorite();
        bool IsFavoriteServiceBusy();
        Task<bool> HandleFavoriteStatus(CancellationToken cancellationToken);
        (string imageName, string glyphText) GetAddToFavoritesImageName();
        (string imageName, string glyphText) GetRemoveFromFavoritesImageName();

        bool IsSyncRecordServiceBusy();
        Task SyncRecord(UserAction userAction, CancellationToken cancellationToken);
        Task<ModifyRecordResult> DeleteRecord(UserAction userAction, CancellationToken cancellationToken);
        bool IsOnlineRecord();
        bool DisplayEmptyPanels();
    }
}
