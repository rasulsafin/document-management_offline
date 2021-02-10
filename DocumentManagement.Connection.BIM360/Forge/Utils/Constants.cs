namespace MRS.DocumentManagement.Connection.BIM360.Forge
{
    public static class Constants
    {
        public static readonly string FOLDER_TYPE = "folders";
        public static readonly string ITEM_TYPE = "items";
        public static readonly string VERSION_TYPE = "versions";
        public static readonly string OBJECT_TYPE = "objects";

        public static readonly string AUTODESK_FILE_TYPE = "items:autodesk.core:File";

        internal static readonly string DATA_PROPERTY = "data";
        internal static readonly string META_PROPERTY = "meta";
        internal static readonly string RESULTS_PROPERTY = "results";
        internal static readonly string INCLUDED_PROPERTY = "included";

        internal static readonly int ITEMS_ON_PAGE = 100;
        internal static readonly string JSON_API_VERSION = "1.0";

        internal static readonly string TOKEN_AUTH_NAME = "Token";
        internal static readonly string REFRESH_TOKEN_AUTH_NAME = "RefreshToken";
        internal static readonly string END_AUTH_NAME = "End";
        internal static readonly string CALLBACK_URL_NAME = "callBackUrl";
        internal static readonly string CLIENT_ID_NAME = "clientId";
        internal static readonly string CLIENT_SECRET_NAME = "clientSecret";

        internal static readonly string AUTH_REQUEST_BODY_CLIENT_ID_FIELD = "client_id";
        internal static readonly string AUTH_REQUEST_BODY_CLIENT_SECRET_FIELD = "client_secret";
        internal static readonly string AUTH_REQUEST_BODY_GRANT_TYPE_FIELD = "grant_type";
        internal static readonly string AUTH_REQUEST_BODY_CODE_FIELD = "code";
        internal static readonly string AUTH_REQUEST_BODY_REDIRECT_URI_FIELD = "redirect_uri";
        internal static readonly string AUTH_REQUEST_BODY_REFRESH_TOKEN_FIELD = "refresh_token";

        internal static readonly string AUTH_GRANT_TYPE_AUTHORIZATION_CODE_VALUE = "authorization_code";
        internal static readonly string AUTH_GRANT_TYPE_REFRESH_TOKEN_VALUE = "refresh_token";
    }
}
