using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.Bim360.Utilities
{
    internal static class DynamicFieldUtilities
    {
        internal static readonly string NULL_VALUE_ID = "_null_value";

        internal static IEnumerable<TID> DeserializeID<TID>(string externalID)
        {
            try
            {
                return string.IsNullOrWhiteSpace(externalID)
                    ? Enumerable.Empty<TID>()
                    : JsonConvert.DeserializeObject<IEnumerable<TID>>(externalID);
            }
            catch (Exception)
            {
                return Enumerable.Empty<TID>();
            }
        }

        internal static IEnumerable<IGrouping<string, TSnapshot>> GetGroupedVariants<T, TSnapshot, TID>(
            IEnumCreator<T, TSnapshot, TID> helper,
            IEnumerable<TSnapshot> allVariants)
            where TSnapshot : AEnumVariantSnapshot<T>
            => allVariants.Select(item => (displayName: helper.GetVariantDisplayName(item), tupleTypes: item))
               .GroupBy(x => x.displayName, x => x.tupleTypes, StringComparer.CurrentCultureIgnoreCase);

        internal static string GetExternalID<TID>(IEnumerable<TID> types)
            => JsonConvert.SerializeObject(types);

        internal static DynamicFieldExternalDto CreateField(
            string valueID,
            IEnumIdentification helper)
            => new ()
            {
                ExternalID = helper.EnumExternalID,
                Name = helper.EnumDisplayName,
                Type = DynamicFieldType.ENUM,
                Value = valueID,
            };

        internal static TSnapshot GetValue<T, TSnapshot, TID>(
            IEnumCreator<T, TSnapshot, TID> creator,
            ProjectSnapshot projectSnapshot,
            ObjectiveExternalDto obj,
            Func<IEnumerable<TID>, TSnapshot, bool> findPredicate,
            out IEnumerable<TID> deserializedIDs)
            where TSnapshot : AEnumVariantSnapshot<T>
        {
            deserializedIDs = null;
            var dynamicField = obj.DynamicFields.FirstOrDefault(d => d.ExternalID == creator.EnumExternalID);

            if (dynamicField == null || (creator.CanBeNull && dynamicField.Value == creator.NullID))
                return null;

            var ids = creator.DeserializeID(dynamicField.Value).ToArray();
            deserializedIDs = ids;
            return creator.GetSnapshots(projectSnapshot)
               .FirstOrDefault(x => findPredicate(ids, x));
        }
    }
}
