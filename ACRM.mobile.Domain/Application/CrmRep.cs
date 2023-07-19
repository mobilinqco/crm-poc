using System;
namespace ACRM.mobile.Domain.Application
{
    public class CrmRep
    {
        public enum CrmRepType
        {
            Rep = 1,
            Group = 2,
            Resource = 4,
            All = 7
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string OrgGroupId { get; private set; }
        public CrmRepType Type { get; private set; }
        public string RecordIdentification { get; private set; }

        public CrmRep(string id, string orgGroupId, string name, string recordIdentification, string repTypeString)
        {
            Id = FormatToAureaRepId(id);
            Name = name;
            OrgGroupId = FormatToAureaRepId(orgGroupId);
            RecordIdentification = recordIdentification;
            Type = ConvertToRepType(repTypeString);
        }

        private CrmRepType ConvertToRepType(string repTypeString)
        {
            if (string.IsNullOrEmpty(repTypeString))
            {
                return CrmRepType.Rep;
            }

            if (repTypeString.Equals("1"))
            {
                return CrmRepType.Group;
            }

            if (repTypeString.Equals("2"))
            {
                return CrmRepType.Resource;
            }

            return CrmRepType.Rep;
        }

        public static string FormatToAureaRepId(string source)
        {
            if (!string.IsNullOrEmpty(source) && source.Length < 9)
            {
                return NineDigitStringFromRep(IntRepId(source));
            }

            return source;
        }

        public static int IntRepId(string source)
        {
            int intValue;
            if (!string.IsNullOrEmpty(source) && source.Length == 9 && source.StartsWith("U"))
            {
                var repString = $"10{source.Substring(1)}";
                return int.TryParse(repString, out intValue) ? intValue : 0;
            }

            return int.TryParse(source, out intValue) ? intValue : 0;
        }

        public static string NineDigitStringFromRep(int rep)
        {
            if (rep == 0)
            {
                return string.Empty;
            }

            return rep >= 1000000000 ? $"U{rep - 1000000000:D8}" : $"{rep:D9}";
        }
    }
}
