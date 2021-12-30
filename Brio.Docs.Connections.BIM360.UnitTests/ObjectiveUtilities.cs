using System.Collections.Generic;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.UnitTests
{
    internal static class ObjectiveUtilities
    {
        public static IEnumerable<DynamicFieldExternalDto> EnumerateAll(IEnumerable<DynamicFieldExternalDto> dynamicFields)
        {
            if (dynamicFields == null)
                yield break;

            foreach (var dynamicField in dynamicFields)
            {
                yield return dynamicField;

                foreach (var child in EnumerateAll(dynamicField.ChildrenDynamicFields))
                    yield return child;
            }
        }
    }
}
