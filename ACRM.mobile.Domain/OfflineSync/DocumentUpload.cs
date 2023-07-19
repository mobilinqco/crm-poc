using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class DocumentUpload
    {
        public int Id { get; set; }
        public string LocalFileName { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string InfoAreaId { get; set; }
        public string RecordId { get; set; }
        public long Size { get; set; }
        public int FieldId { get; set; }

        [JsonIgnore]
        public OfflineRequest OfflineRequest { get; set; }

        public DocumentUpload()
        {
        }
    }
}
