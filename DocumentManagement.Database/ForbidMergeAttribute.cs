using System;

namespace Brio.Docs.Database
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForbidMergeAttribute : Attribute
    {
    }
}
