using ACRM.mobile.DataAccess;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.Utils
{
    public class BackgroundSyncManager
    {
        private readonly ISessionContext _sessionContext;
        private BackgroundSyncWorker _backgroundSyncWorker;

        private bool _isForegroundSyncing = false;

        private bool _hasOfflineRequestsSyncConflicts = false;

        public BackgroundSyncManager(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        public void UpdateOfflineRequestsSyncConflictsStatus(bool flag)
        {
            _hasOfflineRequestsSyncConflicts = flag;
            SendOfflineRequestsSyncErrorStatus();
        }

        private void SendOfflineRequestsSyncErrorStatus()
        {
            MessagingCenter.Send(this, InAppMessages.SendOfflineRequestsSyncErrorsFlag, _hasOfflineRequestsSyncConflicts);
        }

        public bool GetOfflineRequestsSyncErrorStatus()
        {
            return _hasOfflineRequestsSyncConflicts;
        }

        public void ForegroundSyncStarted()
        {
            _isForegroundSyncing = true;
            StopBackgroundSyncWorker();
        }

        public void ForegroundSyncEnded()
        {
            _isForegroundSyncing = false;
            InitNewBackgroundSyncWorker();
        }

        public void ConnectivityChanged(ConnectivityChangedEventArgs e)
        {
            if(_sessionContext.User != null)
            {
                if (e.NetworkAccess == NetworkAccess.None)
                {
                    StopBackgroundSyncWorker();
                }
                else
                {
                    StopBackgroundSyncWorker();
                    InitNewBackgroundSyncWorker(e.ConnectionProfiles);
                }
            }
        }

        public void InitNewBackgroundSyncWorker()
        {
            if(!_isForegroundSyncing)
            {
                _backgroundSyncWorker = AppContainer.Resolve<BackgroundSyncWorker>();
                _backgroundSyncWorker.StartBackgroundSync(Connectivity.ConnectionProfiles);
            }
        }

        public void InitNewBackgroundSyncWorker(IEnumerable<ConnectionProfile> connectionProfiles)
        {
            if (!_isForegroundSyncing)
            {
                _backgroundSyncWorker = AppContainer.Resolve<BackgroundSyncWorker>();
                _backgroundSyncWorker.StartBackgroundSync(connectionProfiles);
            }
        }

        public void StopBackgroundSyncWorker()
        {
            if(_backgroundSyncWorker != null)
            {
                _backgroundSyncWorker.StopBackgroundSync();
                _backgroundSyncWorker = null;
            }
        }
    }
}
