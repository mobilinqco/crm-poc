using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Domain.Application
{
    public class ModifyRecordResult
    {
        public ServerErrorResponse ServerErrorResponse { get; set; }
        public string LocalSaveError { get; set; }
        public DataSaveRecord SavedRecord { get; set; }
        public List<DataSetRecord> OfflineDataSetRecords { get; set; }


        public ModifyRecordResult()
        {
        }

        public bool IsSavedSuccessfuly()
        {
            if(ServerErrorResponse == null && !string.IsNullOrEmpty(LocalSaveError))
            {
                return false;
            }

            return true;
        }

        public bool HasSaveErrors()
        {
            if (ServerErrorResponse != null || !string.IsNullOrEmpty(LocalSaveError))
            {
                return true;
            }

            return false;
        }

        public string ErrorMessage()
        {
            if (!string.IsNullOrEmpty(LocalSaveError))
            {
                return LocalSaveError;
            }

            if(ServerErrorResponse != null)
            {
                return ServerErrorResponse.ErrorString;
            }

            return string.Empty;
        }

        public string UserErrorMessage()
        {
            if (!string.IsNullOrEmpty(LocalSaveError))
            {
                return LocalSaveError;
            }

            if (ServerErrorResponse != null)
            {
                return ServerErrorResponse.UserText;
            }

            return string.Empty;
        }
    }
}
