using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
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

                string id = item.ExternalID.Split("/")[3].Split(":")[0];
                string parentId = item.ExternalID.Split("/")[2].Split(":")[0];

                Uri uri = null;

                try
                {
                    uri = await attachmentsService.GetAttachmentUriAsync(id);
                }
                catch
                {
                    uri = await plansService.GetPlanUriAsync(id, parentId);
                }

                string dirPath = "Downloads\\";

                string name = WebUtility.UrlDecode(uri.Segments[uri.Segments.Length - 1]);
                string path = dirPath + name;

                await itemService.GetAsync(uri.AbsoluteUri, path);

                progress?.Report(++i / (double)count);
            }

            return true;
        }

        public Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            throw new NotImplementedException();
        }

        private class WebDownload : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                if (request != null)
                {
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                return request;
            }
        }
    }
}
