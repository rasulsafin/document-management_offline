using System;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Exceptions
{
    public class ConfigIncorrectException : Exception
    {
        public ConfigIncorrectException(
            string message,
            string propertyName,
            int? elementNumber = null,
            Exception inner = null)
            : base(message, inner)
        {
            PropertyName = propertyName;
            ElementNumber = elementNumber;
        }

        public string PropertyName { get; }

        public int? ElementNumber { get; }
    }
}
