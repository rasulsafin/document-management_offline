using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions.IssueExtensions;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions
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
                    NgIssueSubtypeID = GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.NgIssueSubtypeID)),
                    AssignedTo = GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.AssignedTo)),
                    CreatedAt = objective.CreationDate,
                    DueDate = objective.DueDate,
                    UpdatedAt = objective.UpdatedAt,
                },
            };
        }

        private static string GetDynamicField(ICollection<DynamicFieldExternalDto> dynamicFields, string fieldName)
        {
            // TODO: implement after dynamic fields will be implemented
            return string.Empty;
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
    }
}
