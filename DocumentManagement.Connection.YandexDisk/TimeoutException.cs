using System;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class TimeoutException : Exception
    {
        public TimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
