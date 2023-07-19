using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
    public class DataRequestDetails
    {
        public TableInfo TableInfo;
        public List<FieldControlField> Fields;
        public List<FieldControlSortField> SortFields = null;
        public List<FieldControlField> SearchFields = null;
        public string SearchValue = null;
        public string RecordId = null;
        public List<Filter> Filters = null;

        public DataRequestDetails()
        {
        }
    }
}
