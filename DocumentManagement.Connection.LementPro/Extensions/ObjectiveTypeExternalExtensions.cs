using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro
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
