using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services.Utils
{
    public class CrmDataFieldResolver: ICrmDataFieldResolver
    {
        protected readonly ICacheService _cacheService;
        protected readonly ILogService _logService;
        protected readonly List<TableInfo> _allTables;

        public CrmDataFieldResolver(ICacheService cacheService, ILogService logService)
        {
            _cacheService = cacheService;
            _logService = logService;
            // TODO: maybe is better to use the configuration service here.
            _allTables = (List<TableInfo>)_cacheService.GetItem(CacheItemKeys.TableInfos);
        }

        public (LinkInfo linkInfo, TableInfo relatedTable) GetLinkInfo(TableInfo tableInfo, string infoAreaId, int linkId)
        {
            if (string.IsNullOrWhiteSpace(infoAreaId))
            {
                return (null, null);
            }

            TableInfo relatedTable = _allTables.First(t => t.InfoAreaId == infoAreaId);
            if(relatedTable == null)
            {
                return (null, null);
            }

            LinkInfo linkInfo = tableInfo.GetLinkInfo(infoAreaId, linkId);
            
            if (linkInfo == null)
            {
                if (relatedTable.InfoAreaId != relatedTable.RootInfoAreaId)
                {
                    linkInfo = tableInfo.GetLinkInfo(relatedTable.RootInfoAreaId, linkId);
                }

                if(linkInfo == null && relatedTable != null)
                {
                    var reverselink = relatedTable.Links.Where(a => a.TargetInfoAreaId.Equals(tableInfo.InfoAreaId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (reverselink != null)
                    {
                        linkInfo = new LinkInfo();
                    }
                }
            }

            return (linkInfo, relatedTable);
        }

        public TableInfo TableInfoForInfoArea(string infoAreaId)
        {
            return _allTables.First(t => t.InfoAreaId == infoAreaId);
        }

        public LinkInfo GetIdentLinkInfo(string infoAreaId, int linkId = -1)
        {
            if (!string.IsNullOrWhiteSpace(infoAreaId))
            {
                TableInfo table = _allTables.First(t => t.InfoAreaId == infoAreaId);
                if (table != null)
                {
                    return table.GetLinkInfo(infoAreaId, linkId);
                }
            }

            return null;
        }
    }
}
