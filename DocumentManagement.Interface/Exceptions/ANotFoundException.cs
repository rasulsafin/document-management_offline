using System;

namespace MRS.DocumentManagement.Interface.Exceptions
{
    public abstract class ANotFoundException : Exception
    {
        protected ANotFoundException(string message)
            : base(message)
        {
        }
    }
}
