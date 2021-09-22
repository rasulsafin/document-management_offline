using System;

namespace Brio.Docs.Connection
{
    public class TimeoutException : Exception
    {
        public TimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
