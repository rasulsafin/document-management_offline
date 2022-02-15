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
                BimElement bimElement => bimElement.ID,
                _ => throw new NotSupportedException()
            };

        public static string GetRemoteId(this object entity)
            => entity switch
            {
                Project project => project.ExternalID,
                DynamicField dynamicField => dynamicField.ExternalID,
                Objective objective => objective.ExternalID,
                Item item => item.ExternalID,
                BimElement bimElement => bimElement.GlobalID,
                _ => throw new NotSupportedException()
            };
    }
}
