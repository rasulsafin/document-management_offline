﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ProjectObjectiveConverter : IConverter<Project, ObjectiveExternalDto>
    {
        public async Task<ObjectiveExternalDto> Convert(Project project)
        {
            var time = project.CreatedDate.ToLocalDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = project.GetExternalId(),
                AuthorExternalID = project.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = project.Type },
                Title = project.Name,
                ProjectExternalID = project.GetParentProjectId(),
                ParentObjectiveExternalID = project.Ancestry,
                Status = ObjectiveStatus.Open,
                CreationDate = time,
                DueDate = time,

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
