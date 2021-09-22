using Brio.Docs.Connection.LementPro.Models;
using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Connection.LementPro
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
