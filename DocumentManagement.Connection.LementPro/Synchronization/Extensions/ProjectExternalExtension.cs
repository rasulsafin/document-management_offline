using System;
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

            if (DateTime.TryParse(model.Values.LastModifiedDate, out var parsedLastModified))
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
    }
}
