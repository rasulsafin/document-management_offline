using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360
{
    internal class Bim360Storage : IConnectionStorage
    {
        private readonly ItemsService itemsService;
        private readonly Downloader downloader;

        public Bim360Storage(
            ItemsService itemsService,
            Downloader downloader)
        {
            this.itemsService = itemsService;
            this.downloader = downloader;
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
                await downloader.Download(projectId, file.item, file.version, item.FullPath);
                progress?.Report(++i / (double)count);
            }

            return true;
        }

        public Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            throw new NotImplementedException();
        }
    }
}
