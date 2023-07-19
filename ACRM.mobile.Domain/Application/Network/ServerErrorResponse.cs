using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.Network
{
    public class ServerErrorResponse
    {
        public string ErrorString { get; private set; }
        public string Domain { get; private set; }
        public string Severity { get; private set; }
        public string UserText { get; private set; }
        public string TechnicalText { get; private set; }
        public int ErrorCode { get; private set; }
        public string RecordIdentification { get; private set; }
        public int FieldId { get; private set; }
        public string Details { get; private set; }
        public ServerErrorResponse ParentError { get; private set; }
        public string AddInfo { get; private set; }
        public string ServerDateTime { get; private set; }
        public string ServerSessionId { get; private set; }

        public ServerErrorResponse(List<object> errorArray)
        {
            ErrorString = string.Join(",\r\n", errorArray);
            Domain = errorArray.Count > 0 ? errorArray[0] as string : null;
            Severity = errorArray.Count > 1 ? errorArray[1] as string : null;
            UserText = errorArray.Count > 2 ? errorArray[2] as string : null;
            TechnicalText = errorArray.Count > 3 ? errorArray[3] as string : null;
            var errorCodeStr = errorArray.Count > 4 ? errorArray[4] as string : null;

            ErrorCode = -1;
            if (int.TryParse(errorCodeStr, out int errorCode))
            {
                ErrorCode = errorCode;
            }

            RecordIdentification = errorArray.Count > 5 ? errorArray[5] as string : null;
            var fieldIdStr = errorArray.Count > 6 ? errorArray[6] as string : null;

            FieldId = -1;
            if (int.TryParse(fieldIdStr, out int fieldId))
            {
                FieldId = fieldId;
            }

            Details = errorArray.Count > 7 ? errorArray[7] as string : null;
            //ParentError = errorArray.Count > 8 ? new ServerErrorResponse(errorArray[8] as List<object>) : null;
            AddInfo = errorArray.Count > 9 ? errorArray[9] as string : null;
            ServerDateTime = errorArray.Count > 10 ? errorArray[10] as string : null;
            ServerSessionId = errorArray.Count > 11 ? errorArray[11] as string : null;
        }

        public ServerErrorResponse(int httpCode, string errorMessage)
        {
            ErrorString = errorMessage;
            ErrorCode = httpCode;
            UserText = $"Server Error: {httpCode}. Details: {errorMessage}";
        }

        public ServerErrorResponse(Exception ex)
        {
            ErrorString = ex.Message;
            ErrorCode = -1;
            UserText = $"Network Error. Details: {ex.Message}";
            Details = ex.StackTrace;
        }
    }
}
