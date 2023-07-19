using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ContactTimes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IContactTimesContentService : IContentServiceBase
    {
        public bool HasData { get; }
        public void SetWeekDayNames(List<string> weekDayNames);
        public List<ContactTimesType> GetContactTimesTypes();
        public ContactTimesIntervalSelectionData GetContactTimesIntervalSelectionData(ContactTimesDay selectedContactTimesDay);
        public void UpdateContactTimesTypeData(ContactTimesIntervalSelectionData contactTimesIsntervalSelectionData);
        public void UpdateContactTimesDayData(ContactTimesDay contactTimesDay);
        public List<ContactTimesDataGridEntry> GetContactTimesDataGridDays();
        public Task<ModifyRecordResult> Save(ContactTimesDay contactTimesDay, CancellationToken cancellationToken);
        public Task<ModifyRecordResult> Delete(string recordId, CancellationToken cancellationToken);
    }
}
