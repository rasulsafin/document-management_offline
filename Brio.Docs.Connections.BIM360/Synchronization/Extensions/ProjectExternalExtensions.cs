using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Extensions
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
