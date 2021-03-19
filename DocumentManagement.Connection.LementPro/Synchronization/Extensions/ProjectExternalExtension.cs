using System;
using System.Globalization;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Interface.Dtos;

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
    }
}
