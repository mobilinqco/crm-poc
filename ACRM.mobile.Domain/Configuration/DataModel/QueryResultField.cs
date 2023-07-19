using System;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class QueryResultField
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public int TableNr { get; set; }
        public int FieldNr { get; set; }
        public int FieldId { get; set; }

        public QueryResultField()
        {
        }
    }
}
