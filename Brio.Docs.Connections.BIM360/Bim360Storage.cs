using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.Logging;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360
{
    internal class Bim360Storage : IConnectionStorage
    {
        private readonly ItemsService itemsService;
        private readonly Downloader downloader;
        private readonly VersionsService versionsService;
        private readonly ILogger<Bim360Storage> logger;

        public Bim360Storage(
            ItemsService itemsService,
            Downloader downloader,
            VersionsService versionsService,
            ILogger<Bim360Storage> logger)
        {
            this.itemsService = itemsService;
            this.downloader = downloader;
            this.versionsService = versionsService;
            this.logger = logger;
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

        public async Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos, IProgress<double> progress)
        {
            logger.LogTrace(
                "DeleteFiles started with projectId: {@ProjectID}, itemExternalDtos: {@Items}",
                projectId,
                itemExternalDtos);

            try
            {
                int i = 0;
                var count = itemExternalDtos.Count();

                foreach (var itemExternalDto in itemExternalDtos)
                {
                    var item = new ObjectInfo() { ID = itemExternalDto.ExternalID, Type = ITEM_TYPE };

                    var deletedVersion = new Forge.Models.DataManagement.Version
                    {
                        Attributes = new Forge.Models.DataManagement.Version.VersionAttributes
                        {
                            Extension = new Extension { Type = AUTODESK_VERSION_DELETED_TYPE },
                        },
                        Relationships = new Forge.Models.DataManagement.Version.VersionRelationships
                        {
                            Item = item.ToDataContainer(),
                        },
                    };

                    var deleteResult = await versionsService.PostVersionAsync(projectId, deletedVersion);
                    if (deleteResult.item == null || deleteResult.version == null)
                        return false;

                    progress?.Report(++i / (double)count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't delete files");
                return false;
            }

            return true;
        }
    }
}
