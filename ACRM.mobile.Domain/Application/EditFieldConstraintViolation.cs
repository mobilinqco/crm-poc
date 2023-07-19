using System;
namespace ACRM.mobile.Domain.Application
{
    public class EditFieldConstraintViolation
    {
        public enum ViolationType
        {
            MustField = 1,
            ClientConstraint,
            SaveError
        }

        public ViolationType Type { get; private set; }
        public string LocalizedDescription { get; set; }
        public string ErrorLabel { get; set; }
        public ListDisplayField Field { get; private set; }


        public EditFieldConstraintViolation(ViolationType type, string localizedDescription, ListDisplayField field)
        {
            Type = type;
            if (string.IsNullOrWhiteSpace(localizedDescription))
            {
                LocalizedDescription = string.Empty;
            }
            else
            {
                LocalizedDescription = localizedDescription;
            }

            Field = field;
        }
    }
}

