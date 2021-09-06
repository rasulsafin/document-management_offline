namespace MRS.DocumentManagement.Connection.Bim360.Forge
{
    public static class Constants
    {
        public static readonly string FORGE_URL = "https://developer.api.autodesk.com";

        public static readonly string FOLDER_TYPE = "folders";
        public static readonly string ITEM_TYPE = "items";
        public static readonly string VERSION_TYPE = "versions";
        public static readonly string OBJECT_TYPE = "objects";
        public static readonly string ISSUE_TYPE = "quality_issues";
        public static readonly string ATTACHMENT_TYPE = "attachments";
        public static readonly string ATTACHMENT_URN_TYPE = "dm";
        public static readonly string ROOT_CAUSE_TYPE = "root_causes";
        public static readonly string PUSHPIN_TYPE = "TwoDVectorPushpin";

        public static readonly string AUTODESK_ITEM_FILE_TYPE = "items:autodesk.bim360:File";
        public static readonly string AUTODESK_VERSION_FILE_TYPE = "versions:autodesk.bim360:File";
        public static readonly string AUTODESK_VERSION_DELETED_TYPE = "versions:autodesk.core:Deleted";

        public static readonly string TOKEN_AUTH_NAME = "token";
        public static readonly string REFRESH_TOKEN_AUTH_NAME = "refreshtoken";

        public static readonly string INTEGRATION_TEST_PROJECT = "Integration Test Project (Don't change)";
        public static readonly string DEFAULT_PROJECT_FILES_FOLDER_NAME = "Project Files";

        internal static readonly string AUTHORIZATION_SCHEME = "Bearer";
        internal static readonly string DEFAULT_VERSION_ID = "1";

        internal static readonly string DEFAULT_EXTENSION_VERSION = "1.0";

        internal static readonly string DATA_PROPERTY = "data";
        internal static readonly string META_PROPERTY = "meta";
        internal static readonly string LINKS_PROPERTY = "links";
        internal static readonly string RESULTS_PROPERTY = "results";
        internal static readonly string INCLUDED_PROPERTY = "included";
        internal static readonly string TYPE_PROPERTY = "type";

        internal static readonly string FILTER_QUERY_PARAMETER = "filter[{0}]{1}={2}&";
        internal static readonly string PAGE_QUERY_PARAMETER = "page[{0}]={1}&";

        internal static readonly string LIMIT_PARAMETER_NAME = "limit";
        internal static readonly string OFFSET_PARAMETER_NAME = "offset";
        internal static readonly string NUMBER_QUERY_PARAMETER = "number";

        internal static readonly int ITEMS_ON_PAGE = 100;
        internal static readonly string JSON_API_VERSION = "1.0";
        internal static readonly string CALLBACK_URL_NAME = "RETURN_URL";
        internal static readonly string CLIENT_ID_NAME = "CLIENT_ID";
        internal static readonly string CLIENT_SECRET_NAME = "CLIENT_SECRET";

        internal static readonly string CODE_QUERY_KEY = "code";

        internal static readonly string AUTH_REQUEST_BODY_CLIENT_ID_FIELD = "client_id";
        internal static readonly string AUTH_REQUEST_BODY_CLIENT_SECRET_FIELD = "client_secret";
        internal static readonly string AUTH_REQUEST_BODY_GRANT_TYPE_FIELD = "grant_type";
        internal static readonly string AUTH_REQUEST_BODY_CODE_FIELD = "code";
        internal static readonly string AUTH_REQUEST_BODY_REDIRECT_URI_FIELD = "redirect_uri";
        internal static readonly string AUTH_REQUEST_BODY_REFRESH_TOKEN_FIELD = "refresh_token";
        internal static readonly string AUTH_REQUEST_BODY_SCOPE_FIELD = "scope";

        internal static readonly string AUTH_GRANT_TYPE_AUTHORIZATION_CODE_VALUE = "authorization_code";
        internal static readonly string AUTH_GRANT_TYPE_REFRESH_TOKEN_VALUE = "refresh_token";
        internal static readonly string AUTH_GRANT_TYPE_CLIENT_CREDENTIALS_VALUE = "client_credentials";

        internal static readonly string FILTER_KEY_ISSUE_UPDATED_AFTER = "synced_after";
    }
}
