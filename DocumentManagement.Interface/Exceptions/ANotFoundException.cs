using System.Collections.Generic;

namespace MRS.DocumentManagement.Interface.Exceptions
{
    public abstract class ANotFoundException : KeyNotFoundException
    {
        protected ANotFoundException(string message)
            : base(message)
        {
        }
    }
}
