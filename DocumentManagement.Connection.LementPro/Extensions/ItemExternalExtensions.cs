using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro
{
    internal static class ItemExternalExtensions
    {
        internal static ItemExternalDto ToItemExternalDto(this ObjectBase model)
        {
            if (model == null)
                return null;

            var modifiedDateParsed = DateTime.TryParseExact(
                model.Values?.LastModifiedDate,
                DATE_FORMAT,
                result: out var modifyDate,
                provider: null,
                style: DateTimeStyles.None);

            return new ItemExternalDto
            {
                ExternalID = model.ID?.ToString(),
                FileName = model.Values?.Name,
                UpdatedAt = modifiedDateParsed ? modifyDate : default,
            };
        }

        internal static ItemExternalDto ToItemExternalDto(this File model, ItemExternalDto uploadedDto)
        {
            if (model == null)
                return null;

            var externalId = model.ID?.ToString();

            if (uploadedDto == null)
            {
                return new ItemExternalDto
                {
                    ExternalID = externalId,
                    FileName = model.FileName,
                };
            }

            uploadedDto.ExternalID = externalId;
            return uploadedDto;
        }

        internal static ICollection<ItemExternalDto> ToDtoItems(this List<File> files, ICollection<ItemExternalDto> updatingItems)
        {
            var dtoItems = new List<ItemExternalDto>();

            foreach (var file in files)
            {
                var correspondingItem = updatingItems.FirstOrDefault(ui => ui.FileName == file.FileName);
                var parsedFile = file.ToItemExternalDto(correspondingItem);

                if (parsedFile != null)
                    dtoItems.Add(parsedFile);
            }

            return dtoItems;
        }
    }
}
