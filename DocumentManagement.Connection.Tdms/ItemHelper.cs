using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    internal class ItemHelper
    {
        private static readonly IReadOnlyDictionary<string, ItemType> FILE_TYPES = new Dictionary<string, ItemType>
        {
            { FileTypeID.IFC, ItemType.Bim },
            { FileTypeID.CAD, ItemType.Bim },
            { FileTypeID.PICTURE, ItemType.Media },
            { FileTypeID.VIDEO, ItemType.Media },
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

        private ItemType GetItemType(string fileDefName)
        {
           var result = FILE_TYPES.TryGetValue(fileDefName, out ItemType typeDto);
           if (!result)
                return ItemType.File;

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
