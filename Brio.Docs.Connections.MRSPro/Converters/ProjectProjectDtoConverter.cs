using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class ProjectProjectDtoConverter : IConverter<Project, ProjectExternalDto>
    {
        private readonly IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter;

        public ProjectProjectDtoConverter(IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter)
        {
            this.itemsConverter = itemsConverter;
        }

        public async Task<ProjectExternalDto> Convert(Project project)
        {
            return new ProjectExternalDto
            {
                ExternalID = project.Id,
                Title = project.Name,
                Items = await itemsConverter.Convert(project.Attachments),
            };
        }
    }
}
