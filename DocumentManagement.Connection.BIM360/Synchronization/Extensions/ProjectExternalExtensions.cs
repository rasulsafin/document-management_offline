using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions
{
    internal static class ProjectExternalExtensions
    {
        public static ProjectExternalDto ToDto(this Project project)
        {
            return new ProjectExternalDto
            {
                ExternalID = project.ID,
                Title = project.Attributes.Name,
            };
        }
    }
}
