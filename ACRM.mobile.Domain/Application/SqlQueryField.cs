using System;
namespace ACRM.mobile.Domain.Application
{
    public class SqlQueryField
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string Alias { get; set; }

        public SqlQueryField()
        {
        }

        public string QueryRepresentation()
        {
            return TableName + "." + FieldName + " AS " + Alias;
        }
    }
}
