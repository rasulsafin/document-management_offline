using Google.Apis.Drive.v3;

namespace MRS.DocumentManagement.Connection.GoogleDrive
{
    public static class GoogleDriveAuth
    {
        public static readonly string[] SCOPES = { DriveService.Scope.Drive, DriveService.Scope.DriveAppdata };

        public static readonly string AUTH_URI = "https://accounts.google.com/o/oauth2/auth";
        public static readonly string AUTH_PROVIDER = "https://www.googleapis.com/oauth2/v1/certs";
        public static readonly string TOKEN_URI = "https://oauth2.googleapis.com/token";

        public static readonly string CLIENT_ID = "1827523568-ha5m7ddtvckjqfrmvkpbhdsl478rdkfm.apps.googleusercontent.com";
        public static readonly string CLIENT_SECRET = "fA-2MtecetmXLuGKXROXrCzt";
        public static readonly string RETURN_URL = "http://localhost";
        public static readonly string APPLICATION_NAME = "BRIO MRS";
    }
}
