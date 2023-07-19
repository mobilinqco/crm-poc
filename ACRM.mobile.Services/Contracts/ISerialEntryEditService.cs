using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface ISerialEntryEditService: INewOrEditService
    {
        string FieldGroupName { get; set; }
        Task<PanelData> GetPanelAsync(DataRow dataRow, CancellationToken cancellationToken);
        Task<bool> Delete(string destRecordId, CancellationToken token);
    }
}
