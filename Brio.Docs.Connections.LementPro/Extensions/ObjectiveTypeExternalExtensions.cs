using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Connections.LementPro
{
    internal static class ObjectiveTypeExternalExtensions
    {
        internal static ObjectiveTypeExternalDto ToObjectiveTypeExternal(this LementProType typeModel)
        {
            return new ObjectiveTypeExternalDto
            {
                ExternalId = typeModel.ID.ToString(),
                Name = typeModel.Name,
            };
        }
    }
}
