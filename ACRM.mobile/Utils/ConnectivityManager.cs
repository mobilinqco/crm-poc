using ACRM.mobile.DataAccess;
using Xamarin.Essentials;

namespace ACRM.mobile.Utils
{
    public class ConnectivityManager
    {
        private readonly ISessionContext _sessionContext;
        private readonly BackgroundSyncManager _backgroundSyncManager;

        public ConnectivityManager(ISessionContext sessionContext,
            BackgroundSyncManager backgroundSyncManager)
        {
            _sessionContext = sessionContext;
            _backgroundSyncManager = backgroundSyncManager;

            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            _sessionContext.IsInOfflineMode = e.NetworkAccess == NetworkAccess.None;
            _backgroundSyncManager.ConnectivityChanged(e);
        }
    }
}
