using Google.Apis.Drive.v3.Data;
using MRS.DocumentManagement.Connection.Utils;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public class GoogleDriveElement : CloudElement
    {
        public GoogleDriveElement(File file)
        {
            IsDirectory = file.MimeType.Contains("folder");
            DisplayName = file.Name;
            Href = file.Id;
            ContentType = file.MimeType;

            if (file.Size.HasValue)
                ContentLength = (ulong)file.Size;
            if (file.ModifiedTime.HasValue)
                LastModified = file.ModifiedTime.Value;
            if (file.CreatedTime.HasValue)
                CreationDate = file.CreatedTime.Value;
        }

        public string MulcaFileUrl { get; private set; }

        public string MulcaDigestUrl { get; private set; }

        public string ETag { get; private set; }
    }
}
