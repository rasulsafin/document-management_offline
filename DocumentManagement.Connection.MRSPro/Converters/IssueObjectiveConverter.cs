using System;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class IssueObjectiveConverter : IConverter<Issue, ObjectiveExternalDto>
    {
        private readonly IConverter<string, ObjectiveStatus> statusConverter;

        public IssueObjectiveConverter(IConverter<string, ObjectiveStatus> statusConverter)
        {
            this.statusConverter = statusConverter;
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

                // TODO: DynamicFields
                // DynamicFields = GetDynamicFields(),
                // TODO: Items
                // Items = GetItems(),
                // TODO: BimElements
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }
    }
}
