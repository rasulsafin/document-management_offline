using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.MrsPro.Extensions;
using Brio.Docs.Connections.MrsPro.Interfaces;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class ElementAttachmentItemConverter : IConverter<IEnumerable<IElementAttachment>, ICollection<ItemExternalDto>>
    {
        public Task<ICollection<ItemExternalDto>> Convert(IEnumerable<IElementAttachment> attachments)
        {
            return Task.FromResult<ICollection<ItemExternalDto>>(attachments?.Select(x => new ItemExternalDto()
            {
                ExternalID = x.GetExternalId(),
                FileName = x.OriginalFileName,
                ItemType = ItemType.File, // TODO: Differentiate
                UpdatedAt = x.CreatedDate.ToLocalDateTime() ?? DateTime.Now,
            }).ToList() ?? new List<ItemExternalDto>());
        }
    }
}
