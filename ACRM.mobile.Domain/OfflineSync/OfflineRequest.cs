using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class OfflineRequest
    {
        public int Id { get; set; }
        public string SyncDate { get; set; }
        public string RequestType { get; set; }
        public string JsonData { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorStack { get; set; }
        public int GroupRequestNr { get; set; }
        public int Draft { get; set; }
        public string ProcessType { get; set; }
        public string Response { get; set; }
        public string TitleLine { get; set; }
        public string DetailsLine { get; set; }
        public string ImageName { get; set; }
        public int ServerRequestNumber { get; set; }
        public string ServerTime { get; set; }
        public string SessionId { get; set; }
        public int FolloowUpRoot { get; set; }
        public string TranslationKey { get; set; }
        public string RelatedInfo { get; set; }
        public int BaseError { get; set; }
        public string AppVersion { get; set; }

        public List<OfflineRecord> Records { get; set; }
        public List<DocumentUpload> DocumentUploads { get; set; }

        public OfflineRequest()
        {
        }

        public bool IsValid()
        {
            return (Records != null && Records.Count > 0)
                || (DocumentUploads != null && DocumentUploads.Count > 0);
        }
    }
}