using System;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Synchronization.Extensions
{
    internal static class DMContextExtension
    {
        public static int GetId(this object entity)
            => entity switch
            {
                Project project => project.ID,
                DynamicField dynamicField => dynamicField.ID,
                Objective objective => objective.ID,
                Item item => item.ID,
                _ => throw new NotSupportedException()
            };
    }
}
