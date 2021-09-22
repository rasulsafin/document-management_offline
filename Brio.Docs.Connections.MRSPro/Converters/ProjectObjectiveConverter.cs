using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Connections.MrsPro.Extensions;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class ProjectObjectiveConverter : IConverter<Project, ObjectiveExternalDto>
    {
        private readonly IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter;

        public ProjectObjectiveConverter(IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter)
        {
            this.itemsConverter = itemsConverter;
        }

        public async Task<ObjectiveExternalDto> Convert(Project project)
        {
            var time = project.CreatedDate.ToLocalDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = project.GetExternalId(),
                AuthorExternalID = project.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = project.Type },
                Title = project.Name,
                Description = string.Empty,
                ProjectExternalID = project.GetParentProjectId(),
                ParentObjectiveExternalID = project.Ancestry,
                Status = ObjectiveStatus.Open,
                CreationDate = time,
                DueDate = time,
                UpdatedAt = time,
                Items = await itemsConverter.Convert(project.Attachments),

                // TODO: DynamicFields
                // DynamicFields = GetDynamicFields(),
                // TODO: BimElements
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }
    }
}
