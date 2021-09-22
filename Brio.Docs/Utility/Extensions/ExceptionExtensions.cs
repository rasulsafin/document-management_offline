using System;

namespace Brio.Docs.Utility.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception ConvertToBase(this Exception exception)
            => exception == null ? null : new Exception(exception.Message, exception.InnerException.ConvertToBase());
    }
}
