using System;
using System.Collections.Generic;
using System.Globalization;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro
{
    internal static class ObjectiveExternalExtensions
    {
        internal static ObjectBaseToCreate ToModelToCreate(this ObjectiveExternalDto objective)
        {
            if (!int.TryParse(objective.ObjectiveType.ExternalId, out var parsedTypeId))
                return null;

            var modelValue = new ObjectBaseValueToCreate
            {
                Type = parsedTypeId,
                Name = objective.Title,
                Description = objective.Description,
                StartDate = objective.CreationDate.ToString(DATE_FORMAT),
                IsExpired = IsExpired(objective.Status),
                Project = objective.ProjectExternalID,
                EndDate = objective.DueDate.ToString(DATE_FORMAT),
            };

            var model = new ObjectBaseToCreate { Values = modelValue };
            return model;
        }

        internal static ObjectBaseToUpdate ToModelToUpdate(this ObjectiveExternalDto objective)
        {
            if (!int.TryParse(objective.ObjectiveType.ExternalId, out var parsedTypeId)
                || !int.TryParse(objective.ExternalID, out var parsedId))
                return null;

            var modelValue = new ObjectBaseValueToUpdate
            {
                ID = parsedId,
                Type = parsedTypeId,
                Name = objective.Title,
                Description = objective.Description,
                StartDate = objective.CreationDate.ToString(DATE_FORMAT),
                IsExpired = IsExpired(objective.Status),
                LastModifiedDate = objective.UpdatedAt.ToString(DATE_FORMAT),
                EndDate = objective.DueDate.ToString(DATE_FORMAT),
            };

            if (int.TryParse(objective.ProjectExternalID, out var parsedProjectId))
                modelValue.Project = parsedProjectId;

            if (objective.ProjectExternalID != DEFAULT_PROJECT_STUB.ExternalID
                && int.TryParse(objective.ProjectExternalID, out var parsedExternalId))
            {
                modelValue.Project = parsedExternalId;
            }

            var model = new ObjectBaseToUpdate
            {
                ID = parsedId,
                Values = modelValue,
            };

            return model;
        }

        internal static ObjectiveExternalDto ToObjectiveExternalDto(this ObjectBase model)
        {
            var resultDto = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto
                {
                    ExternalId = model.Values.Type.ID.ToString(),
                    Name = model.Values.Type.Name,
                },
                Title = model.Values.Name,
                Description = model.Values.Description,
                Status = ParseStatus(model),
                ProjectExternalID = model.Values.Project?.ID?.ToString() ?? DEFAULT_PROJECT_STUB.ExternalID.ToString(),
                ExternalID = model.ID.ToString(),
            };

            //if (model.Values.Managers.ID != null)
            //    resultDto.AuthorExternalID = model.Values.Managers.ID.ToString();

            if (model.Values.StartDate != null)
                resultDto.CreationDate = DateTime.Parse(model.Values.StartDate, CultureInfo.InvariantCulture);

            if (model.Values.EndDate != null)
                resultDto.DueDate = DateTime.Parse(model.Values.EndDate, CultureInfo.InvariantCulture);

            if (model.Values.LastModifiedDate != null)
                resultDto.UpdatedAt = DateTime.Parse(model.Values.LastModifiedDate, CultureInfo.InvariantCulture).ToUniversalTime();

            if (model.Values.Files != null)
            {
                var items = new List<ItemExternalDto>();
                foreach (var file in model.Values.Files)
                {
                    items.Add(new ItemExternalDto
                    {
                        ExternalID = file.ID.ToString(),
                        FileName = file.FileName,
                        FullPath = file.FileName,
                        ItemType = ItemTypeHelper.GetTypeByName(file.FileName),
                    });
                }

                resultDto.Items = items;
            }

            return resultDto;
        }

        private static bool IsExpired(ObjectiveStatus status)
            => status == ObjectiveStatus.Late;

        private static ObjectiveStatus ParseStatus(ObjectBase model)
        {
            if (model.State.GetValueOrDefault() == STATE_ARCHIVED)
                return ObjectiveStatus.Ready;

            return model.Values.IsExpired switch
            {
                null => ObjectiveStatus.Undefined,
                true => ObjectiveStatus.Late,
                false => ObjectiveStatus.InProgress,
            };
        }
    }
}
