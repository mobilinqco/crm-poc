using System;
using System.Data;

namespace ACRM.mobile.Domain.Application
{
    public class DataResponse
    {
        public DataTable Result { get; set; }
        public bool IsRetrievedOnline { get; set; }

        public DataResponse()
        {
        }
    }
}
