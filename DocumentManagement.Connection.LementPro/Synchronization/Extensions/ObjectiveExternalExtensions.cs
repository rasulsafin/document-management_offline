using System;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    internal static class ObjectiveExternalExtensions
    {
        internal static ObjectBaseToCreate ToModelToCreate(this ObjectiveExternalDto objective)
        {
            var modelValue = new ObjectBaseValueToCreate
            {
                // Type should be mapped outside of the extension
                Name = objective.Title,
                Description = objective.Description,
                StartDate = objective.CreationDate.ToString(DATE_FORMAT),
                IsExpired = IsExpired(objective.Status),
            };

            var model = new ObjectBaseToCreate { Values = modelValue };
            return model;
        }

        internal static TaskToUpdate ToModelToUpdate(this ObjectiveExternalDto objective)
        {
            var modelValue = new TaskValueToUpdate
            {
                // Type should be mapped outside of the extension
                Name = objective.Title,
                Description = objective.Description,
                StartDate = objective.CreationDate.ToString(DATE_FORMAT),
                IsExpired = IsExpired(objective.Status),
                LastModifiedDate = objective.UpdatedAt.ToString(DATE_FORMAT),
            };

            if (!int.TryParse(objective.ExternalID, out var parsedId))
                return null;

            var model = new TaskToUpdate
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
                CreationDate = DateTime.ParseExact(model.Values.StartDate, DATE_FORMAT, null),
                Status = ParseStatus(model.Values.IsExpired),
                ProjectExternalID = model.Values.Project?.ID?.ToString(),
            };

            return resultDto;
        }

        private static bool IsExpired(ObjectiveStatus status)
            => status == ObjectiveStatus.Late;

        private static ObjectiveStatus ParseStatus(bool? isExpired)
        {
            return isExpired switch
            {
                null => ObjectiveStatus.Undefined,
                true => ObjectiveStatus.Late,
                false => ObjectiveStatus.InProgress,
            };
        }
    }
}
