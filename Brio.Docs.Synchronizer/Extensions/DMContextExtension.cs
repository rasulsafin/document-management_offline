using System;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Synchronization.Extensions
{
    internal static class DMContextExtension
    {
        public static int GetId(this object entity)
            => entity switch
            {
                DynamicField dynamicField => dynamicField.ID,
                Objective objective => objective.ID,
                _ => throw new NotSupportedException()
            };
    }
}
