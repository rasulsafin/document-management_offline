using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    internal class ItemMapper : IModelMapper<ItemDto, TDMSFile>
    {
        private static readonly IReadOnlyDictionary<string, ItemTypeDto> FILE_TYPES = new Dictionary<string, ItemTypeDto>
        {
            { FileTypeID.IFC, ItemTypeDto.Bim },
            { FileTypeID.CAD, ItemTypeDto.Bim },
            { FileTypeID.PICTURE, ItemTypeDto.Media },
            { FileTypeID.VIDEO, ItemTypeDto.Media },
        };

        public ItemDto ToDto(TDMSFile tdmsObject)
        {
            var itemDto = new ItemDto()
            {
                Name = tdmsObject.FileName,
                ExternalItemId = tdmsObject.Handle,
                ItemType = GetItemType(tdmsObject.FileDefName),
            };

            return itemDto;
        }

        public TDMSFile ToModel(ItemDto objectDto, TDMSFile model)
        {
            throw new NotImplementedException();
        }

        private ItemTypeDto GetItemType(string fileDefName)
        {
           var result = FILE_TYPES.TryGetValue(fileDefName, out ItemTypeDto typeDto);
           if (!result)
                return ItemTypeDto.File;

           return typeDto;
        }
    }
}
