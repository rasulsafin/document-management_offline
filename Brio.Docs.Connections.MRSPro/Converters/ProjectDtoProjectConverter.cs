using System;
using System.Threading.Tasks;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    public class ProjectDtoProjectConverter : IConverter<ProjectExternalDto, Project>
    {
        public async Task<Project> Convert(ProjectExternalDto from)
        {
            throw new NotImplementedException();
        }
    }
}
