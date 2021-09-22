using Brio.Docs.Connection.LementPro.Models;
using Brio.Docs.Connection.Utils;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Brio.Docs.Connection.LementPro.LementProConstants;

namespace Brio.Docs.Connection.LementPro
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

                dto.Items = items;
            }

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
                LastModifiedDate = dto.UpdatedAt.ToString(DATE_FORMAT),
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
