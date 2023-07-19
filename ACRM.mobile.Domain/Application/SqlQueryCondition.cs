using System;
namespace ACRM.mobile.Domain.Application
{
    public class SqlQueryCondition
    {
        public string Left { get; set; }
        public string Right { get; set; }
        public string Operator { get; set; }

        public SqlQueryCondition()
        {
        }
    }
}
