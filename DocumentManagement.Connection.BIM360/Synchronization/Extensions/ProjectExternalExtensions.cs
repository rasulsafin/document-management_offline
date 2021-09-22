using Brio.Docs.Connection.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Connection.Bim360.Synchronization.Extensions
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
