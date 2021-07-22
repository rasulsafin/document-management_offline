﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ProjectObjectiveConverter : IConverter<Project, ObjectiveExternalDto>
    {
        private readonly IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter;

        public ProjectObjectiveConverter(IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>> itemsConverter)
        {
            this.itemsConverter = itemsConverter;
        }

        public async Task<ObjectiveExternalDto> Convert(Project project)
        {
            var time = project.CreatedDate.ToLocalDateTime() ?? DateTime.Now;

            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = project.GetExternalId(),
                AuthorExternalID = project.Owner,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = project.Type },
                Title = project.Name,
                Description = string.Empty,
                ProjectExternalID = project.GetParentProjectId(),
                ParentObjectiveExternalID = project.Ancestry,
                Status = ObjectiveStatus.Open,
                CreationDate = time,
                DueDate = time,
                UpdatedAt = time,
                Items = await itemsConverter.Convert(project.Attachments),

                // TODO: DynamicFields
                // DynamicFields = GetDynamicFields(),
                // TODO: BimElements
                // BimElements = GetBimElements(),
            };

            return resultDto;
        }
    }
}
