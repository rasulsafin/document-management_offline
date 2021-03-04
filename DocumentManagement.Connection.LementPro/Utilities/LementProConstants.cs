namespace MRS.DocumentManagement.Connection.LementPro
{
    /// <summary>
    /// Contains constants to use in LementProConnection.
    /// </summary>
    public static class LementProConstants
    {
        // 2 MB
        public static readonly long UPLOAD_FILES_CHUNKS_SIZE = 5242880;

        public static readonly int STATE_ARCHIVED = 2;

        public static readonly string DATE_FORMAT = "yyyy - MM - ddThh: mm:ss.FFFZ";

        public static readonly string REQUEST_UPLOAD_POSTEDFILE_FIELDNAME = "postedFile";
        public static readonly string REQUEST_UPLOAD_FILEPART_FIELDNAME = "filePart";
        public static readonly string REQUEST_UPLOAD_FILENAME_FIELDNAME = "fileName";
        public static readonly string REQUEST_UPLOAD_SIZE_FIELDNAME = "size";
        public static readonly string REQUEST_UPLOAD_ID_FIELDNAME = "id";
        public static readonly string REQUEST_UPLOAD_ENDUPLOAD_FIELDNAME = "endUpload";

        public static readonly string AUTH_NAME_LOGIN = "login";
        public static readonly string AUTH_NAME_PASSWORD = "password";
        public static readonly string AUTH_NAME_TOKEN = "token";
        public static readonly string AUTH_NAME_END = "end";

        public static readonly string CONTENT_TYPE_LABEL = "content-type";
        public static readonly string CONTENT_ACCEPT_LANGUAGE = "Accept-Language";

        public static readonly string STANDARD_CONTENT_TYPE = "application/json";
        public static readonly string STANDARD_ACCEPT_LANGUAGE = "ru-RU";
        public static readonly string STANDARD_AUTHENTICATION_SCHEME = "auth";

        public static readonly string OBJECTTYPE_BIM = "Bim";
        public static readonly string OBJECTTYPE_BIM_ATTRIBUTE_VERSION = "modelVersions";
        public static readonly string OBJECTTYPE_TASKS = "Tasks";
        public static readonly string OBJECTTYPE_SINGLE_TASK = "Task";

        public static readonly string RESPONSE_OBJECT_NAME = "object";
        public static readonly string RESPONSE_COLLECTION_ITEMS_NAME = "items";
        public static readonly string RESPONSE_COOKIES_AUTH_NAME = "auth";
        public static readonly string RESPONSE_COOKIES_EXPIRES_NAME = "expires";
        public static readonly char RESPONSE_COOKIE_VALUES_SEPARATOR = ';';
        public static readonly char RESPONSE_COOKIE_KEY_VALUE_SEPARATOR = '=';
    }
}
