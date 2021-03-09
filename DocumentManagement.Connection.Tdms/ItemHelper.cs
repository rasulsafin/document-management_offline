using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    internal class ItemHelper
    {
        private static readonly IReadOnlyDictionary<string, ItemTypeDto> FILE_TYPES = new Dictionary<string, ItemTypeDto>
        {
            { FileTypeID.IFC, ItemTypeDto.Bim },
            { FileTypeID.CAD, ItemTypeDto.Bim },
            { FileTypeID.PICTURE, ItemTypeDto.Media },
            { FileTypeID.VIDEO, ItemTypeDto.Media },
        };

        internal void SetItems(TDMSObject tdmsObject, IEnumerable<ItemExternalDto> items)
        {
            if (items == null)
                return;

            var checkedfiles = items.Where(d => System.IO.File.Exists(d.FullPath) && !tdmsObject.Files.Has(d.FileName));
            foreach (var file in checkedfiles)
                tdmsObject.Files.Create(FileTypeID.ANY, file.FullPath);

            var deletedFiles = tdmsObject.Files.Cast<TDMSFile>().Where(s => items.FirstOrDefault(item => item.FileName == s.FileName) == default);
            foreach (var file in deletedFiles)
                tdmsObject.Files.Remove(file);

            tdmsObject.Update();

            var f = tdmsObject.Files.Count;
        }

        internal ICollection<ItemExternalDto> GetItems(TDMSObject tdmsObject)
            => tdmsObject.Files?.Cast<TDMSFile>()?.Select(x => ToDto(x)).ToList();

        private ItemTypeDto GetItemType(string fileDefName)
        {
           var result = FILE_TYPES.TryGetValue(fileDefName, out ItemTypeDto typeDto);
           if (!result)
                return ItemTypeDto.File;

           return typeDto;
        }

        private ItemExternalDto ToDto(TDMSFile tdmsObject)
        {
            var itemDto = new ItemExternalDto()
            {
                FileName = tdmsObject.FileName,
                ExternalID = tdmsObject.Handle,
                ItemType = GetItemType(tdmsObject.FileDefName),
            };

            return itemDto;
        }
    }
}
