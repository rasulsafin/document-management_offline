using Disk.SDK;
using Disk.SDK.Provider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Connection.YandexDisk
{
    public class YandexDiskController
    {
        private string accessToken;

        public YandexDiskController(string accessToken)
        {
            this.accessToken = accessToken;
        }
    }
}
