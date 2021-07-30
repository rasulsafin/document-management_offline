using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Exceptions
{
    public class ArgumentValidationException : DocumentManagementException
    {
        public ArgumentValidationException(string details, string title = null, IReadOnlyDictionary<string, string[]> errors = null)
            : base(title ?? "Validation Error", details, errors)
        {
        }
    }
}
