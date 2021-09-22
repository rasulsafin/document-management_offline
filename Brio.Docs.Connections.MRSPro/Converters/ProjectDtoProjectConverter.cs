using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Threading.Tasks;

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
