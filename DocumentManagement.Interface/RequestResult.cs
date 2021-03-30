using System;

namespace MRS.DocumentManagement.Interface
{
    public class RequestResult
    {
        public RequestResult(object value) => Value = value;

        public RequestResult(Exception exception) => Exception = exception;

        public object Value { get; private set; }

        public Exception Exception { get; set; }
    }
}
