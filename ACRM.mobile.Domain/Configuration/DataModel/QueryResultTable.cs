using System;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class QueryResultTable
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public int ParentTableNr { get; set; }
        public int TableNr { get; set; }
        public string Relation { get; set; }
        public string InfoAreaId { get; set; }
        public string Alias { get; set; }

        public QueryResultTable()
        {
        }
    }
}
