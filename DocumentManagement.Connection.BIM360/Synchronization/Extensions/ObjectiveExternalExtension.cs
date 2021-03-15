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
                    CreatedAt = objective.CreationDate == default ? (DateTime?)null : objective.CreationDate,
                    DueDate = objective.DueDate == default ? (DateTime?)null : objective.DueDate,
                    UpdatedAt = objective.UpdatedAt == default ? (DateTime?)null : objective.UpdatedAt,
                },
            };
        }

        internal static ObjectiveExternalDto ToExternalDto(this Issue issue, string project)
        {
            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = issue.ID,

                // TODO: check via tests what GET request returns for issue.Relationships.Container URL
                
                //AuthorExternalID = issue.Attributes.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "Test Job Type" },//issue.Attributes.NgIssueTypeID },
                Title = issue.Attributes.Title,
                Description = issue.Attributes.Description,
                ProjectExternalID = project,
                //Status = ParseStatus(issue.Attributes.Status),
                //DynamicFields = GetDynamicFields(issue),
                //Items = GetItems(issue),

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
            var field = dynamicFields?.FirstOrDefault(f => f.Name == fieldName);
            return field?.Value;
        }

        private static ICollection<ItemExternalDto> GetItems(Issue issue)
        {
            // use issuesService.GetAttachmentsAsync
            // mb implement in Synchronizer
            return new List<ItemExternalDto>();
        }

        private static ICollection<DynamicFieldExternalDto> GetDynamicFields(Issue issue)
        {
            // At least NgIssueSubType fields should be added
            var issueSubType = issue.Attributes.NgIssueSubtypeID;
            var issueSubTypeField = new DynamicFieldExternalDto
            {
                Name = nameof(issue.Attributes.NgIssueSubtypeID),
                Value = issueSubType,
                Type = DynamicFieldType.STRING,
            };
            return new List<DynamicFieldExternalDto>
            {
                issueSubTypeField,
            };
        }

        private static string ParseStatus(ObjectiveStatus status)
        {
            return status switch
            {
                ObjectiveStatus.Open => nameof(Status.Open).ToLower(),
                ObjectiveStatus.Ready => nameof(Status.Closed).ToLower(),
                _ => null,
            };
        }

        private static ObjectiveStatus ParseStatus(string stringStatus)
        {
            if (string.IsNullOrWhiteSpace(stringStatus) ||
                !Enum.TryParse<Status>(stringStatus, true, out var parsedStatus))
                return ObjectiveStatus.Undefined;

            return parsedStatus switch
            {
                Status.Open => ObjectiveStatus.Open,
                Status.Closed => ObjectiveStatus.Ready,
                _ => ObjectiveStatus.Undefined,
            };
        }
    }
}
