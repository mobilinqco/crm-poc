using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services.SubComponents
{
    public abstract class QueryBuilderBase
    {
        protected readonly ICacheService _cacheService;
        protected readonly ILogService _logService;
        protected readonly ICrmDataFieldResolver _crmDataFieldResolver;

        public QueryBuilderBase(ICrmDataFieldResolver crmDataFieldResolver, ICacheService cacheService, ILogService logService)
        {
            _cacheService = cacheService;
            _logService = logService;
            _crmDataFieldResolver = crmDataFieldResolver;
        }

        public TableInfo TableInfoForInfoArea(string infoAreaId)
        {
            return _crmDataFieldResolver.TableInfoForInfoArea(infoAreaId);
        }

    }
}
