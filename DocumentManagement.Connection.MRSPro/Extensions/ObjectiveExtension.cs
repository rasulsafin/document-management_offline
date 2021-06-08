using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Extensions
{
    internal static class ObjectiveExtension
    {
        internal static ObjectiveExternalDto ToObjectiveExternalDto(this IElement element)
        {
            return element switch
            {
                Objective obj => obj.ToObjectiveExternalDto(),
                Project project => project.ToObjectiveExternalDto(),
                _ => null
            };
        }

        internal static ObjectiveExternalDto ToObjectiveExternalDto(this Objective objective)
        {
            var time = objective.CreatedDate.ToDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = objective.Id,
                AuthorExternalID = objective.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = Constants.ISSUE_TYPE },
                Title = objective.Title,
                Description = objective.Description,
                ProjectExternalID = GetProject(objective.Ancestry),
                ParentObjectiveExternalID = objective.ParentId,
                Status = GetStatus(objective.State),
                CreationDate = time,
                DueDate = objective.DueDate.ToDateTime() ?? time,
                UpdatedAt = objective.LastModifiedDate.ToDateTime() ?? time,
                // DynamicFields = GetDynamicFields(),
                // Items = GetItems(),
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }

        internal static ObjectiveExternalDto ToObjectiveExternalDto(this Project project)
        {
            var time = project.CreatedDate.ToDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = project.Id,
                AuthorExternalID = project.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = Constants.ELEMENT_TYPE },
                Title = project.Name,
                Description = string.Empty,
                ProjectExternalID = GetProject(project.Ancestry),
                ParentObjectiveExternalID = project.ParentId,
                Status = ObjectiveStatus.Open,
                CreationDate = time,
                DueDate = time,
                UpdatedAt = time,
                // DynamicFields = GetDynamicFields(),
                // Items = GetItems(),
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

        private static ObjectiveStatus GetStatus(string state)
        {
            // TODO: Status
            return ObjectiveStatus.Open;
        }
    }
}
