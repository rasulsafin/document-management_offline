using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.MrsPro.Extensions;
using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class IssueObjectiveConverter : IConverter<Issue, ObjectiveExternalDto>
    {
        private readonly IConverter<string, ObjectiveStatus> statusConverter;
        private readonly IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter;

        public IssueObjectiveConverter(
            IConverter<string, ObjectiveStatus> statusConverter,
            IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter)
        {
            this.statusConverter = statusConverter;
            this.itemsConverter = itemsConverter;
        }

        public async Task<ObjectiveExternalDto> Convert(Issue issue)
        {
            var time = issue.CreatedDate.ToLocalDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = issue.GetExternalId(),
                AuthorExternalID = issue.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = issue.Type },
                Title = issue.Title,
                Description = issue.Description,
                ProjectExternalID = issue.GetParentProjectId(),
                ParentObjectiveExternalID = issue.Ancestry,
                Status = await statusConverter.Convert(issue.State),
                CreationDate = time,
                DueDate = issue.DueDate.ToLocalDateTime() ?? time,
                UpdatedAt = issue.LastModifiedDate.ToLocalDateTime() ?? time,
                Items = await itemsConverter.Convert(issue.Attachments),

                // TODO: DynamicFields
                // DynamicFields = GetDynamicFields(),
                // TODO: BimElements
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }
    }
}
