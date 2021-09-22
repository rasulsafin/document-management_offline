using Brio.Docs.Client.Dtos;
using System;
using System.Globalization;

namespace Brio.Docs.Connections.LementPro
{
    /// <summary>
    /// Contains constants to use in LementProConnection.
    /// </summary>
    public static class LementProConstants
    {
        // 2 MB
        public static readonly long UPLOAD_FILES_CHUNKS_SIZE = 5242880;

        public static readonly int STATE_ARCHIVED = 2;

        /// <summary>
        /// ISO 8601.
        /// </summary>
        public static readonly string DATE_FORMAT = "O";

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
        public static readonly string OBJECTTYPE_PROJECT = "Project";

        public static readonly string RESPONSE_OBJECT_NAME = "object";
        public static readonly string RESPONSE_COLLECTION_ITEMS_NAME = "items";
        public static readonly string RESPONSE_COOKIES_AUTH_NAME = "auth";
        public static readonly string RESPONSE_COOKIES_EXPIRES_NAME = "expires";
        public static readonly char RESPONSE_COOKIE_VALUES_SEPARATOR = ';';
        public static readonly char RESPONSE_COOKIE_KEY_VALUE_SEPARATOR = '=';

        private static readonly string DEFAULT_PROJECT_STUB_ID = "LementPro 63cbe66b-b7fb-4465-a5b4-d585765b33af";
        private static readonly string DEFAULT_PROJECT_STUB_TITLE = "Default Project";

        public static ProjectExternalDto DEFAULT_PROJECT_STUB => new ProjectExternalDto
        {
            ExternalID = DEFAULT_PROJECT_STUB_ID,
            Title = DEFAULT_PROJECT_STUB_TITLE,
            UpdatedAt = DateTime.MinValue.AddMilliseconds(1),
        };
    }
}
