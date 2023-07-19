using System;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class QueryResult
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public string Sync { get; set; }

        public QueryResult()
        {
        }
    }
}
