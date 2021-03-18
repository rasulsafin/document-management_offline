using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsStorage : IConnectionStorage
    {
        private readonly TDMSApplication tdms;

        public TdmsStorage(TDMSApplication tdms)
        {
            this.tdms = tdms;
        }

        public bool DeleteFiles(IEnumerable<ItemExternalDto> itemExternalDto)
        {
            throw new NotImplementedException();
        }

        public bool DownloadFiles(string projectId, IEnumerable<ItemExternalDto> itemExternalDto)
        {
            TDMSObject obj = tdms.GetObjectByGUID(projectId);
            foreach (var item in itemExternalDto)
            {
                var tdmsfile = obj.Files.Cast<TDMSFile>().First(f =>
                    item.ExternalID == f.Handle);

                tdmsfile?.CheckOut(item.FullPath);
            }

            return true;
        }
    }
}
