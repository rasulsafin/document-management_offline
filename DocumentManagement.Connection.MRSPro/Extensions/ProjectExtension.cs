using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class ProjectExtension
    {
        internal static ProjectExternalDto ToProjectExternalDto(this Project project)
        {
            return new ProjectExternalDto
            {
                ExternalID = project.Id,
                Title = project.Name,
            };
        }
    }
}
