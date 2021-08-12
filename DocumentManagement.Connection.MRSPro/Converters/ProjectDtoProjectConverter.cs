using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    public class ProjectDtoProjectConverter : IConverter<ProjectExternalDto, Project>
    {
        public async Task<Project> Convert(ProjectExternalDto from)
        {
            throw new NotImplementedException();
        }
    }
}
