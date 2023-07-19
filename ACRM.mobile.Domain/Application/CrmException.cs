using System;
namespace ACRM.mobile.Domain.Application
{
    public enum CrmExceptionType
    {
        GenericError,
        UserAction,
        CrmData
    }

    public enum CrmExceptionSubType
    {
        GenericErrorGeneric,
        UserActionCanceledByUser,
        CrmDataGenericError,
        CrmDataDataSyncInfoAreaNotDefined,
        CrmDataTableInfoDataSetMismatch,
        CrmDataFieldNotDefinedInModel,
        CrmDataRequestError
    }

    public class CrmException : Exception
    {
        public CrmExceptionType ExceptionType { get; set; }
        public CrmExceptionSubType ExceptionSubType { get; set; }
        public string Content { get; set; }

        public CrmException(string content, CrmExceptionType type = CrmExceptionType.GenericError, CrmExceptionSubType subType = CrmExceptionSubType.GenericErrorGeneric)
        {
            ExceptionType = type;
            ExceptionSubType = subType;
            Content = content;
        }
    }
}
