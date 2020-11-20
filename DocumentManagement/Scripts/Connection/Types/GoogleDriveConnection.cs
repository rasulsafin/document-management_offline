using MRS.Bim.DocumentManagement.GoogleDrive;
using MRS.Bim.DocumentManagement.Utilities;
using MRS.Bim.Tools;

namespace MRS.Bim.DocumentManagement
{
    public class GoogleDriveConnection : CloudBimOnline<GoogleDriveManager>
    {
        public GoogleDriveConnection(IProgressing progress) : base(@"GoogleDrive.db", progress)
            => loggerPath = "GoogleDrive-Log.txt";
    }
}