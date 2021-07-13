using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class ModelsExtension
    {
        internal static ProjectExternalDto ToDto(this Project project)
        {
            return new ProjectExternalDto
            {
                ExternalID = project.Id,
                Title = project.Name,
            };
        }

        internal static string GetExternalId(this IElement element)
        {
            var type = element.Type == Constants.ISSUE_TYPE ? Constants.TASK :
                Constants.PROJECT;
            return $"{element.Ancestry}{Constants.ID_PATH_SPLITTER}{element.Id}{type}";
        }
    }
}
