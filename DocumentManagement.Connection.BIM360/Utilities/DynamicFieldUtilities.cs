using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    internal static class DynamicFieldUtilities
    {
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

        internal static IEnumerable<IGrouping<string, T>> GetGroupedTypes<T, TID>(
            IEnumCreator<T, TID> helper,
            IEnumerable<T> allTypes)
            => allTypes.Select(item => (displayName: helper.GetVariantDisplayName(item), tupleTypes: item))
               .GroupBy(x => x.displayName, x => x.tupleTypes, StringComparer.CurrentCultureIgnoreCase);

        internal static string GetExternalID<TID>(IOrderedEnumerable<TID> types)
            => JsonConvert.SerializeObject(types);

        internal static DynamicFieldExternalDto CreateField<T, TID>(string valueID, IEnumCreator<T, TID> helper)
            => new ()
            {
                ExternalID = helper.EnumExternalID,
                Name = helper.EnumDisplayName,
                Type = DynamicFieldType.ENUM,
                Value = valueID,
            };
    }
}
