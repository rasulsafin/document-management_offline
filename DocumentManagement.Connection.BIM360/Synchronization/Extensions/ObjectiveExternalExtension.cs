using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Extensions
{
    internal static class ObjectiveExternalExtension
    {
        internal static Issue ToIssue(this ObjectiveExternalDto objective)
        {
            return new Issue
            {
                ID = objective.ExternalID,
                Attributes = new Issue.IssueAttributes
                {
                    Owner = objective.AuthorExternalID,
                    NgIssueTypeID = objective.ObjectiveType.Name,
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = ParseStatus(objective.Status),
                    NgIssueSubtypeID =
                        GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.NgIssueSubtypeID)),
                    AssignedTo = GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.AssignedTo)),
                    CreatedAt = objective.CreationDate,
                    DueDate = objective.DueDate,
                    UpdatedAt = objective.UpdatedAt,
                },
            };
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

        private static string GetDynamicField(ICollection<DynamicFieldExternalDto> dynamicFields, string fieldName)
        {
            // TODO: remove boxing to dynamic after DynamicFieldExternalDto implementation
            var field = dynamicFields.Select(f => (dynamic)f).FirstOrDefault(f => f.Name == fieldName);
            return field.Value;
        }

        private static string ParseStatus(ObjectiveStatus status)
        {
            return status switch
            {
                ObjectiveStatus.Open => nameof(Status.Open).ToLower(),
                ObjectiveStatus.Ready => nameof(Status.Close).ToLower(),
                _ => string.Empty,
            };
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
                // TODO: At least NgIssueSubType fields should be added

                //new DynamicFieldExternalDto
                //{

                //}
            };
        }

        private static ObjectiveStatus ParseStatus(string stringStatus)
        {
            if (string.IsNullOrWhiteSpace(stringStatus) ||
                Enum.TryParse<Status>(stringStatus, true, out var parsedStatus))
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
