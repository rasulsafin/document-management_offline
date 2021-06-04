using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProStorage : IConnectionStorage
    {
        public Task<bool> DeleteFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDtos)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDto, IProgress<double> progress, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
