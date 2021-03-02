using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Models.Issue;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions
{
    public static class IssueExtensions
    {
        internal enum Status
        {
            Draft,
            Open,
            Close,
        }

        public static Issue GetPatchableIssue(this Issue issue)
        {
            var result = new Issue
            {
                ID = issue.ID,
                Type = issue.Type,
                Attributes = new IssueAttributes(),
            };

            var type = typeof(IssueAttributes);
            var properties = type
                    .GetProperties()
                    .ToDictionary(p => type.GetDataMemberName(p), p => p);

            foreach (var attribute in issue.Attributes.PermittedAttributes)
            {
                if (properties.TryGetValue(attribute, out var property))
                    property.SetValue(result.Attributes, property.GetValue(issue.Attributes));
            }

            return result;
        }

        internal static ObjectiveExternalDto ToExternalDto(this Issue issue)
        {
            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = issue.ID,
                // TODO: check via tests what GET request returns for issue.Relationships.Container URL
                // ProjectExternalID,
                AuthorExternalID = issue.Attributes.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = issue.Attributes.NgIssueTypeID },
                Title = issue.Attributes.Title,
                Description = issue.Attributes.Description,
                Status = ParseStatus(issue.Attributes.Status),
                DynamicFields = GetDynamicFields(issue),
                Items = GetItems(issue),

                // TODO: add BimElements retrieving
                // BimElements,
            };

            if (issue.Attributes.CreatedAt.HasValue)
                resultDto.CreationDate = issue.Attributes.CreatedAt.Value;

            if (issue.Attributes.DueDate.HasValue)
                resultDto.DueDate = issue.Attributes.DueDate.Value;

            if (issue.Attributes.UpdatedAt.HasValue)
                resultDto.UpdatedAt = issue.Attributes.UpdatedAt.Value;

            return resultDto;
        }

        private static ICollection<ItemExternalDto> GetItems(Issue issue)
        {
            // use issuesService.GetAttachmentsAsync
            // mb implement in Synchronizer
            throw new NotImplementedException();
        }

        private static ICollection<DynamicFieldExternalDto> GetDynamicFields(Issue issue)
        {
            return new List<DynamicFieldExternalDto>
            {
                // At leasy NgIssueSubType and AssignedTo fields should be added

                //new DynamicFieldExternalDto
                //{
                    
                //}
            };
        }

        private static ObjectiveStatus ParseStatus(string stringStatus)
        {
            if (string.IsNullOrWhiteSpace(stringStatus) || Enum.TryParse<Status>(stringStatus, true, out var parsedStatus))
                return ObjectiveStatus.Undefined;

            return parsedStatus switch
            {
                Status.Open => ObjectiveStatus.Open,
                Status.Close => ObjectiveStatus.Ready,
                _ => ObjectiveStatus.Undefined,
            };
        }
    }
}
