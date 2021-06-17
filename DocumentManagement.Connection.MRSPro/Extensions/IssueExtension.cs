using System;
using System.Linq;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class IssueExtension
    {
        internal static ObjectiveExternalDto ToDto(this IElement element)
        {
            var time = element.CreatedDate.ToDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = $"{element.Id}{Constants.ID_SPLITTER}{element.Type}",
                AuthorExternalID = element.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = element.Type },
                Title = element.Title,
                Description = element.Description,
                ProjectExternalID = GetProject(element.Ancestry),
                ParentObjectiveExternalID = $"{element.ParentId}{Constants.ID_SPLITTER}{element.ParentType}",
                Status = GetStatus(element.State),
                CreationDate = time,
                DueDate = element.DueDate.ToDateTime() ?? time,
                UpdatedAt = element.LastModifiedDate.ToDateTime() ?? time,

                // TODO: DynamicFields
                // DynamicFields = GetDynamicFields(),
                // TODO: Items
                // Items = GetItems(),
                // TODO: BimElements
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }

        internal static IElement ToModel(this ObjectiveExternalDto dto, string type)
        {
            IElement element = GetElement(type);
            element.Id = dto.ExternalID?.Split(Constants.ID_SPLITTER)[0];
            element.Owner = dto.AuthorExternalID;
            element.Description = dto.Description;
            element.Title = dto.Title;
            element.CreatedDate = dto.CreationDate.ToUnixTime();
            element.DueDate = dto.DueDate.ToUnixTime();
            element.Type = Constants.ISSUE_TYPE;
            element.State = GetState(dto.Status);

            var parentData = dto.ParentObjectiveExternalID?.Split(Constants.ID_SPLITTER);
            element.ParentId = parentData?[0] ?? dto.ProjectExternalID;
            element.ParentType = parentData?[1] ?? Constants.ELEMENT_TYPE;

            // TODO: DynamicFields
            // TODO: Items
            // TODO: BimElements
            return element;
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

        private static ObjectiveStatus GetStatus(string state)
         => state switch
         {
             Constants.STATE_VERIFIED => ObjectiveStatus.Ready,
             Constants.STATE_OPENED => ObjectiveStatus.Open,
             Constants.STATE_COMPLETED => ObjectiveStatus.InProgress,
             _ => ObjectiveStatus.Undefined,
         };

        private static string GetState(ObjectiveStatus status)
        => status switch
        {
            ObjectiveStatus.Open => Constants.STATE_OPENED,
            ObjectiveStatus.Undefined => Constants.STATE_OPENED,
            ObjectiveStatus.InProgress => Constants.STATE_COMPLETED,
            ObjectiveStatus.Ready => Constants.STATE_VERIFIED,
            ObjectiveStatus.Late => Constants.STATE_OPENED,
            _ => Constants.STATE_OPENED,
        };

        private static IElement GetElement(string type)
            => type == Constants.ELEMENT_TYPE ? new Project() : new Issue();
    }
}
