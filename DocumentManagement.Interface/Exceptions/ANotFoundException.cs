using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Exceptions
{
    public abstract class ANotFoundException : DocumentManagementException
    {
        protected ANotFoundException(string details, string title = null, IReadOnlyDictionary<string, string[]> errors = null)
            : base(title ?? "Not found", details, errors)
        {
        }
    }
}
