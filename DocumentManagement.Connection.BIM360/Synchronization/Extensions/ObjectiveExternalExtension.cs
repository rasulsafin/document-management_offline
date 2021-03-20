﻿using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

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
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = ParseStatus(objective.Status),
                    AssignedTo = GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.AssignedTo)),
                    CreatedAt = objective.CreationDate == default ? (DateTime?)null : objective.CreationDate,
                    DueDate = objective.DueDate == default ? (DateTime?)null : objective.DueDate,
                    UpdatedAt = objective.UpdatedAt == default ? (DateTime?)null : objective.UpdatedAt,
                    LocationDescription = GetBimElements(objective),
                },
            };
        }

        internal static ObjectiveExternalDto ToExternalDto(this Issue issue, string project, string typeName)
        {
            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = issue.ID,
                AuthorExternalID = issue.Attributes.CreatedBy,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = issue.Type },
                Title = issue.Attributes.Title,
                Description = issue.Attributes.Description,
                ProjectExternalID = project,
                Status = ParseStatus(issue.Attributes.Status),
                DynamicFields = GetDynamicFields(issue, typeName),
                Items = GetItems(issue),
                BimElements = GetBimElements(issue),
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
            var field = dynamicFields?.FirstOrDefault(f => f.ExternalID == fieldName);
            return field?.Value;
        }

        private static ICollection<ItemExternalDto> GetItems(Issue issue)
        {
            // use issuesService.GetAttachmentsAsync
            // mb implement in Synchronizer
            return new List<ItemExternalDto>();
        }

        private static ICollection<DynamicFieldExternalDto> GetDynamicFields(Issue issue, string typeName)
        {
            var issueSubTypeField = new DynamicFieldExternalDto
            {
                ExternalID =
                    typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.NgIssueTypeID)),
                Name = STANDARD_NG_TYPES.Value.Name,
                Value = STANDARD_NG_TYPES.Value.EnumerationValues.FirstOrDefault(x => x.ExternalID == typeName)?.ExternalID ??
                    UNDEFINED_NG_TYPE.ExternalID,
                Type = DynamicFieldType.ENUM,
            };
            return new List<DynamicFieldExternalDto>
            {
                issueSubTypeField,
            };
        }

        private static ICollection<BimElementExternalDto> GetBimElements(Issue issue)
            => string.IsNullOrWhiteSpace(issue.Attributes.LocationDescription)
                ? ArraySegment<BimElementExternalDto>.Empty
                : JsonConvert.DeserializeObject<ICollection<BimElementExternalDto>>(
                    issue.Attributes.LocationDescription);

        private static string GetBimElements(ObjectiveExternalDto objectiveExternalDto)
            => objectiveExternalDto.BimElements == null
                ? null
                : JsonConvert.SerializeObject(objectiveExternalDto.BimElements);

        private static Status ParseStatus(ObjectiveStatus status)
        {
            return status switch
            {
                ObjectiveStatus.Open => Status.Open,
                ObjectiveStatus.InProgress => Status.Open,
                ObjectiveStatus.Ready => Status.Closed,
                _ => Status.Draft
            };
        }

        private static ObjectiveStatus ParseStatus(Status status)
            => status switch
            {
                Status.Open => ObjectiveStatus.Open,
                Status.Closed => ObjectiveStatus.Ready,
                Status.Answered => ObjectiveStatus.Ready,
                _ => ObjectiveStatus.Undefined,
            };
    }
}
