using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProStorage : IConnectionStorage
    {
        private readonly AttachmentsService attachmentsService;

        public MrsProStorage(
            AttachmentsService attachmentsService)
        {
            this.attachmentsService = attachmentsService;
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
                var attachment = await attachmentsService.GetByIdAsync(item.ExternalID);

                string link = $"https://s3-eu-west-1.amazonaws.com/plotpad-org/{attachment.UrlToFile}";
                string dirPath = item.FullPath;
                string path = dirPath + item.FileName;

                Directory.CreateDirectory(dirPath);

                WebClient webClient = new WebClient();
                await webClient.DownloadFileTaskAsync(new Uri(link), path);

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
