using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ContactTimes
{
    public class ContactTimesType
    {

        public string TypeRecordId { get; }
        public string TypeName { get; }
        public List<ContactTimesDay> ContactTimesDays { get; }

        public ContactTimesType(string typeRecordId, string typeName, List<ContactTimesDay> contactTimesDays)
        {
            TypeRecordId = typeRecordId;
            TypeName = typeName;
            ContactTimesDays = contactTimesDays;
        }

        public ContactTimesType Clone()
        {
            return new ContactTimesType(TypeRecordId, TypeName, ContactTimesDays);
        }
    }
}
