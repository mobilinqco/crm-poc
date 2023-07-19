using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Characteristics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICharacteristicsContentService: IContentServiceBase
    {
        List<CharacteristicGroup> GetEditableCharacteristicGroups();
        List<CharacteristicGroup> GetCharacteristicGroups();
        Task<ModifyRecordResult> Save(Dictionary<int, string> currentFieldValues, Dictionary<int, string> oldFieldValues, string recordId, CancellationToken cancellationToken);
        Task<ModifyRecordResult> Delete(string recordId, CancellationToken cancellationToken);
    }
}
