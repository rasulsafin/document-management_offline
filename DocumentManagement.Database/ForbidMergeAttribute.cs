using System;

namespace MRS.DocumentManagement.Database
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForbidMergeAttribute : Attribute
    {
    }
}
