﻿using System;
using System.Globalization;
using System.Linq;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    internal static class ProjectExternalExtension
    {
        internal static ProjectExternalDto ToProjectExternalDto(this ObjectBase model)
        {
            var dto = new ProjectExternalDto
            {
                ExternalID = model.ID.Value.ToString(CultureInfo.InvariantCulture),
                Title = model.Values.Name,
            };

            if (DateTime.TryParse(model.Values.LastModifiedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedLastModified))
                dto.UpdatedAt = parsedLastModified;

            return dto;
        }

        internal static ObjectBaseToCreate ToModelToCreate(this ProjectExternalDto dto)
        {
            return new ObjectBaseToCreate
            {
                FileIds = Enumerable.Empty<int>(),
                Values = new ObjectBaseValueToCreate
                {
                    Name = dto.Title,
                    StartDate = DateTime.Now.ToString(DATE_FORMAT),
                },
            };
        }

        internal static ObjectBaseToUpdate ToModelToUpdate(this ProjectExternalDto dto, ObjectBase notUpdatedProject)
        {
            var oldValues = notUpdatedProject.Values;
            var parsedId = int.Parse(dto.ExternalID);
            var oldCreationDate = DateTime.Parse(oldValues.CreationDate, CultureInfo.InvariantCulture);
            var oldStartDate = DateTime.Parse(oldValues.StartDate, CultureInfo.InvariantCulture);
            var modelValue = new ObjectBaseValueToUpdate
            {
                CreationDate = oldCreationDate.ToString(DATE_FORMAT),
                ID = parsedId,
                Type = oldValues.Type.ID,
                Name = dto.Title,
                LastModifiedDate = dto.UpdatedAt == default
                                ? null
                                : dto.UpdatedAt.ToString(DATE_FORMAT),
                Description = oldValues.Description,
                StartDate = oldStartDate.ToString(DATE_FORMAT),
            };

            // TODO: Add uploading projects items
            var model = new ObjectBaseToUpdate
            {
                ID = parsedId,
                Values = modelValue,
            };

            return model;
        }
    }
}
