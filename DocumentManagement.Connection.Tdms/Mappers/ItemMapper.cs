using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Mappers
{
    internal class ItemMapper
    {
        private static readonly IReadOnlyDictionary<string, ItemTypeDto> FILE_TYPES = new Dictionary<string, ItemTypeDto>
        {
            { FileTypeID.IFC, ItemTypeDto.Bim },
            { FileTypeID.CAD, ItemTypeDto.Bim },
            { FileTypeID.PICTURE, ItemTypeDto.Media },
            { FileTypeID.VIDEO, ItemTypeDto.Media },
        };

        public ItemExternalDto ToDto(TDMSFile tdmsObject)
        {
            var itemDto = new ItemExternalDto()
            {
                Name = tdmsObject.FileName,
                ExternalItemId = tdmsObject.Handle,
                ItemType = GetItemType(tdmsObject.FileDefName),
            };

            return itemDto;
        }

        internal void SetItems(TDMSObject tdmsObject, ObjectiveExternalDto objectDto)
        {
            if (objectDto.Items == null)
                return;

            var checkedfiles = objectDto.Items.Where(d => System.IO.File.Exists(d.FullPath) && !tdmsObject.Files.Has(d.Name));
            foreach (var file in checkedfiles)
                tdmsObject.Files.Create(FileTypeID.ANY, file.FullPath);

            var deletedFiles = tdmsObject.Files.Cast<TDMSFile>().Where(s => objectDto.Items.FirstOrDefault(item => item.Name == s.FileName) == default);
            foreach (var file in deletedFiles)
                tdmsObject.Files.Remove(file);

            tdmsObject.Update();

            var f = tdmsObject.Files.Count;
        }

        internal ICollection<ItemExternalDto> GetItems(TDMSObject tdmsObject)
            => tdmsObject.Files?.Cast<TDMSFile>()?.Select(x => ToDto(x)).ToArray();

        private ItemTypeDto GetItemType(string fileDefName)
        {
           var result = FILE_TYPES.TryGetValue(fileDefName, out ItemTypeDto typeDto);
           if (!result)
                return ItemTypeDto.File;

           return typeDto;
        }
    }
}
