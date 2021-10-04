using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro
{
    public class MrsProStorage : IConnectionStorage
    {
        private readonly AttachmentsService attachmentsService;
        private readonly PlansService plansService;
        private readonly ItemService itemService;

        public MrsProStorage(
            AttachmentsService attachmentsService,
            PlansService plansService,
            ItemService itemService)
        {
            this.attachmentsService = attachmentsService;
            this.plansService = plansService;
            this.itemService = itemService;
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

                string id = item.ExternalID.Split(ID_PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries)[^1]
                    .Split(ID_SPLITTER)[0];
                string parentId = item.ExternalID.Split(ID_PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries)[^2]
                    .Split(ID_SPLITTER)[0];

                Uri uri = null;

                if (item.ExternalID.EndsWith(ATTACHMENT))
                    uri = await attachmentsService.GetUriAsync(id);
                else
                    uri = await plansService.GetUriAsync(id, parentId);

                await itemService.GetAsync(uri.AbsoluteUri, item.FullPath);
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
