using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

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
            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];
            int i = 0;
            foreach (var item in itemExternalDto)
            {
                cancelToken.ThrowIfCancellationRequested();
                var file = await itemsService.GetAsync(projectId, item.ExternalID);

                var storage = file.version.Relationships.Storage?.Data
                   .ToObject<StorageObject,
                        StorageObject.StorageObjectAttributes,
                        StorageObject.StorageObjectRelationships>();
                if (storage == null)
                    continue;

                var (bucketKey, hashedName) = storage.ParseStorageId();
                await objectsService.GetAsync(bucketKey, hashedName, item.FullPath);
                progress?.Report(++i / (double)itemExternalDto.Count());
            }

            return true;
        }

        public Task<bool> DeleteFiles(IEnumerable<ItemExternalDto> itemExternalDto)
        {
            throw new System.NotImplementedException();
        }
    }
}
