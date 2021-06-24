using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    public class Bim360Storage : IConnectionStorage
    {
        private readonly ObjectsService objectsService;
        private readonly ItemsService itemsService;

        public Bim360Storage(
            ObjectsService objectsService,
            ItemsService itemsService)
        {
            this.objectsService = objectsService;
            this.itemsService = itemsService;
        }

        public async Task<bool> DownloadFiles(string projectId,
            IEnumerable<ItemExternalDto> itemExternalDto,
            IProgress<double> progress,
            CancellationToken cancelToken)
        {
            int i = 0;
            var count = itemExternalDto.Count();

            foreach (var item in itemExternalDto)
            {
                cancelToken.ThrowIfCancellationRequested();
                var file = await itemsService.GetAsync(projectId, item.ExternalID);

                var storage = file.version.GetStorage() ??
                    (await itemsService.GetVersions(projectId, item.ExternalID))
                   .Select(x => (x.Attributes.VersionNumber, storage: x.GetStorage()))
                   .Where(x => x.storage != null)
                   .Aggregate((max, vs) => max.VersionNumber >= vs.VersionNumber ? max : vs)
                   .storage;

                if (storage != null)
                {
                    var (bucketKey, hashedName) = storage.ParseStorageId();
                    await objectsService.GetAsync(bucketKey, hashedName, item.FullPath);
                }

                progress?.Report(++i / (double)count);
            }

            return true;
        }

        public Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            throw new System.NotImplementedException();
        }
    }
}
