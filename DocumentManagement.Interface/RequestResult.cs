using System;
using Newtonsoft.Json;

namespace Brio.Docs.Interface
{
    public class RequestResult
    {
        public RequestResult(object value)
            : this(value, null)
        {
        }

        public RequestResult(Exception exception)
            : this(null, exception)
        {
        }

        [JsonConstructor]
        public RequestResult(object value, Exception exception)
        {
            Value = value;
            Exception = exception;
        }

        public object Value { get; private set; }

        public Exception Exception { get; set; }
    }
}
