using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Interfaces;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ElementAttachmentItemConverter : IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>>
    {
        public Task<ICollection<ItemExternalDto>> Convert(IEnumerable<IElementAttachment> attachments)
        {
            return Task.FromResult<ICollection<ItemExternalDto>>(attachments?.Select(x => new ItemExternalDto()
            {
                ExternalID = x.Ancestry,
                FileName = x.FileName,
                ItemType = ItemType.File,
                UpdatedAt = x.CreatedDate.ToLocalDateTime() ?? DateTime.Now,
            }).ToList() ?? new List<ItemExternalDto>());
        }
    }
}
