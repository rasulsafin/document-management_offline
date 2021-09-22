using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TDMS;

namespace Brio.Docs.Connections.Tdms
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

        private static readonly Dictionary<string, string> TDMS_FILE_TYPES = new Dictionary<string, string>
        {
           { ".ifc", FileTypeID.IFC },
           { ".ifczip", FileTypeID.IFC },
           { ".pdf", FileTypeID.PDF },
           { ".png", FileTypeID.PICTURE },
           { ".jpg", FileTypeID.PICTURE },
           { ".jpeg", FileTypeID.PICTURE },
           { ".xml", FileTypeID.XLS },
           { ".txt", FileTypeID.TEXT },
           { ".doc", FileTypeID.WORD },
           { ".docx", FileTypeID.WORD },
           { ".mp4", FileTypeID.VIDEO },
        };

        internal void SetItems(TDMSObject tdmsObject, IEnumerable<ItemExternalDto> items)
        {
            if (items == null)
                return;

            var checkedfiles = items.Where(d => File.Exists(d.FullPath) && !tdmsObject.Files.Has(d.FileName));
            foreach (var file in checkedfiles)
                tdmsObject.Files.Create(SetItemType(file.FullPath), file.FullPath);

            var deletedFiles = tdmsObject.Files.Cast<TDMSFile>().Where(s => items.FirstOrDefault(item => item.FileName == s.FileName) == default);
            foreach (var file in deletedFiles)
                tdmsObject.Files.Remove(file);

            tdmsObject.Update();

            var f = tdmsObject.Files.Count;
        }

        internal ICollection<ItemExternalDto> GetItems(TDMSObject tdmsObject)
            => tdmsObject.Files?.Cast<TDMSFile>()?.Select(x => ToDto(x)).ToList();

        private string SetItemType(string filePath)
        {
            TDMS_FILE_TYPES.TryGetValue(Path.GetExtension(filePath), out string type);
            return type != default ? type : FileTypeID.ANY;
        }

        private ItemType GetItemType(string fileDefName)
        {
           var result = FILE_TYPES.TryGetValue(fileDefName, out ItemType typeDto);
           return !result ? ItemType.File : typeDto;
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
