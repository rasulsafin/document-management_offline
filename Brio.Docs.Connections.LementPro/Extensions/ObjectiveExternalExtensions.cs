using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Connections.Utils;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using static Brio.Docs.Connections.LementPro.LementProConstants;

namespace Brio.Docs.Connections.LementPro
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
                I66444 = GetBimElements(objective),
                I66474 = GetAuthorExternalId(objective),
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
                I66444 = GetBimElements(objective),
                I66474 = GetAuthorExternalId(objective),
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
                BimElements = GetBimElements(model.Values.I66444),
                AuthorExternalID = GetAuthorExternalId(model.Values.I66474),
            };

            if (model.Values.StartDate != null)
                resultDto.CreationDate = DateTime.Parse(model.Values.StartDate, CultureInfo.InvariantCulture);

            if (model.Values.EndDate != null)
                // Due date stored at LementPro as UTC time
                resultDto.DueDate = DateTime.Parse(model.Values.EndDate, CultureInfo.InvariantCulture).AddHours(3);

            if (model.Values.LastModifiedDate != null)
                resultDto.UpdatedAt = DateTime.Parse(model.Values.LastModifiedDate, CultureInfo.InvariantCulture);

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

        private static ICollection<BimElementExternalDto> GetBimElements(string customField)
            => string.IsNullOrWhiteSpace(customField)
                ? ArraySegment<BimElementExternalDto>.Empty
                : JsonConvert.DeserializeObject<ICollection<BimElementExternalDto>>(
                    customField);

        private static string GetBimElements(ObjectiveExternalDto objectiveExternalDto)
            => objectiveExternalDto.BimElements == null
                ? null
                : JsonConvert.SerializeObject(objectiveExternalDto.BimElements);

        private static string GetAuthorExternalId(string customField)
            => string.IsNullOrWhiteSpace(customField)
                ? null
                : JsonConvert.DeserializeObject<string>(customField);

        private static string GetAuthorExternalId(ObjectiveExternalDto objectiveExternalDto)
            => objectiveExternalDto.AuthorExternalID == null
                ? null
                : JsonConvert.SerializeObject(objectiveExternalDto.AuthorExternalID);
    }
}
