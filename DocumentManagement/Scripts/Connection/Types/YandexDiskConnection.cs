using MRS.Bim.DocumentManagement.Utilities;
using MRS.Bim.Tools;
using MRS.Bim.DocumentManagement.YandexDisk;

namespace MRS.Bim.DocumentManagement
{
    public class YandexDiskConnection : CloudBimOnline<YandexDiskManager>
    {
        public YandexDiskConnection(IProgressing progress) : base(@"YandexDisk.db", progress)
        {
            loggerPath = "YandexDisk-Log.txt";
        }

    }
}
