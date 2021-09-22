using Brio.Docs.Connection.LementPro.Models;
using Brio.Docs.Connection.LementPro.Utilities;
using Brio.Docs.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brio.Docs.Connection.LementPro.Synchronization
{
    internal static class ItemsSynchronizationHelper
    {
        internal static async Task<IEnumerable<int>> UploadFilesAsync(ICollection<ItemExternalDto> items, CommonRequestsUtility utility)
        {
            var fileIds = new List<int>();
            var existingFiles = items.Where(i => !string.IsNullOrWhiteSpace(i.ExternalID));
            var filesToUpload = items.Where(i => string.IsNullOrWhiteSpace(i.ExternalID));

            foreach (var file in filesToUpload)
            {
                if (string.IsNullOrWhiteSpace(file.FileName) || string.IsNullOrWhiteSpace(file.FullPath))
                    continue;

                var uploaded = await utility.AddFileAsync(file.FileName, file.FullPath);
                if (uploaded.IsSuccess.GetValueOrDefault())
                    fileIds.Add(uploaded.ID.Value);
            }

            foreach (var file in existingFiles)
            {
                if (int.TryParse(file.ExternalID, out var parsed))
                    fileIds.Add(parsed);
            }

            return fileIds;
        }

        internal static async Task<ObjectBaseToUpdate> UpdateFilesAsync(
            ObjectBase existingModel,
            ObjectBaseToUpdate modelToUpdate,
            ICollection<ItemExternalDto> items,
            CommonRequestsUtility utility)
        {
            var existingFileIds = existingModel.Values.Files.Select(f => f.ID.Value);
            var dtoFileIds = new List<int>();
            foreach (var pi in items)
            {
                if (int.TryParse(pi.ExternalID, out var parsedFileId))
                    dtoFileIds.Add(parsedFileId);
            }

            modelToUpdate.RemovedFileIds = existingFileIds.Where(ef => !dtoFileIds.Any(pi => pi == ef)).ToList();
            var filesToAdd = items.Where(pi => string.IsNullOrWhiteSpace(pi.ExternalID));
            var filesToAddIds = new List<int>();
            foreach (var item in filesToAdd)
            {
                var addingResult = await utility.AddFileAsync(item.FileName, item.FullPath);
                if (addingResult.IsSuccess.GetValueOrDefault())
                    filesToAddIds.Add(addingResult.ID.Value);
            }

            modelToUpdate.AddedFileIds = filesToAddIds;

            return modelToUpdate;
        }
    }
}
