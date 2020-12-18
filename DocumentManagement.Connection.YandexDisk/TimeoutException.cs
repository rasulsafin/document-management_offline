using System;

namespace DocumentManagement.Connection.YandexDisk
{
    public class TimeoutException : Exception
    {
        public TimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
