using System;

namespace MRS.DocumentManagement.Connection
{
    public class TimeoutException : Exception
    {
        public TimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
