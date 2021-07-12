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

        public async Task<ObjectiveExternalDto> Convert(Issue element)
        {
            var time = element.CreatedDate.ToLocalDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = $"{element.Id}{Constants.ID_SPLITTER}{element.Type}", //TODO: just leave ancestry as id?
                AuthorExternalID = element.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = element.Type },
                Title = element.Title,
                Description = element.Description,
                ProjectExternalID = GetProject(element.Ancestry), //TODO: just leave ancestry as id?
                ParentObjectiveExternalID = $"{element.ParentId}{Constants.ID_SPLITTER}{element.ParentType}",
                Status = await statusConverter.Convert(element.State),
                CreationDate = time,
                DueDate = element.DueDate.ToLocalDateTime() ?? time,
                UpdatedAt = element.LastModifiedDate.ToLocalDateTime() ?? time,

                // TODO: DynamicFields
                // DynamicFields = GetDynamicFields(),
                // TODO: Items
                // Items = GetItems(),
                // TODO: BimElements
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }

        private static string GetProject(string projectId)
        {
            // from "/5ebb7cb7782f96000146e7f3:ORGANIZATION/5ebbff9021ccb400017d707b:PROJECT"
            // need "5ebbff9021ccb400017d707b"
            return projectId?
                .Split('/', StringSplitOptions.RemoveEmptyEntries)?
                .ElementAt(1)?
                .Split(':')?
                .ElementAt(0);
        }
    }
}
