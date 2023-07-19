using System;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICrmDataFieldResolver
    {
        (LinkInfo linkInfo, TableInfo relatedTable) GetLinkInfo(TableInfo tableInfo, string infoAreaId, int linkId);
        TableInfo TableInfoForInfoArea(string infoAreaId);
        LinkInfo GetIdentLinkInfo(string infoAreaId, int linkId = -1);
    }
}
