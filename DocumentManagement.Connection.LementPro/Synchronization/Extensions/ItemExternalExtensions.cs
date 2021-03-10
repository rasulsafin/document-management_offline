using System;
using System.Globalization;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
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

        internal static ItemExternalDto ToItemExternalDto(this File model)
        {
            if (model == null)
                return null;

            return new ItemExternalDto
            {
                ExternalID = model.ID?.ToString(),
                FileName = model.FileName,
            };
        }
    }
}
