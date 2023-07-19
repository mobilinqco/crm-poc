using System;
using System.Collections.Generic;
using System.Data;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Logging;

namespace ACRM.mobile.Services.Processors
{
    public class SubNodeProcessor
    {
        private ILogService _logService;

        public SubNodeProcessor(ILogService logService)
        {
            _logService = logService;
        }


        public void PopulateRowRecIds(SubNode subNode, List<string> recordIds, DataRow dataRow)
        {
            if (recordIds.Count > 0)
            {
                dataRow["recid"] = recordIds[0];
                var parts = recordIds[0].Split('.');
                dataRow["title"] = parts.Length > 1 ? parts[0] : string.Empty;

                int i = 1;
                foreach (string recIdAlias in subNode.SubNodesRecIdAliasList())
                {
                    if (recordIds.Count > i)
                    {
                        dataRow[recIdAlias] = recordIds[i];
                    }
                    i++;
                }
            }
        }

        public void PopulateRowData(SubNode subNode, List<List<string>> values, DataRow dataRow)
        {
            List<SubNode> subNodes = subNode.SubNodes();
            List<string> fieldsAliases = subNode.OnlySubNodeFieldsAlias();
            int j = 0;
            values.ForEach(values =>
            {
                if (values != null)
                {
                    int i = 0;
                    values.ForEach(value =>
                    {
                        if (i < fieldsAliases.Count)
                        {
                            dataRow[fieldsAliases[i]] = value != null ? value : "";
                            i++;
                        }
                    });
                }

                if(subNodes.Count > j)
                {
                    fieldsAliases = subNodes[j].OnlySubNodeFieldsAlias();
                    j++;
                }
            });
        }
    }
}
