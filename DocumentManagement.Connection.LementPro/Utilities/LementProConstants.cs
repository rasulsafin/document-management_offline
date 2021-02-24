﻿namespace MRS.DocumentManagement.Connection.LementPro
{
    /// <summary>
    /// Contains constants to use in LementProConnection.
    /// </summary>
    public static class LementProConstants
    {
        public const string DATE_FORMAT = "yyyy - MM - ddThh: mm:ss.FFFZ";

        public const int STATE_ARCHIVED = 2;

        public const string AUTH_NAME_LOGIN = "login";
        public const string AUTH_NAME_PASSWORD = "password";
        public const string AUTH_NAME_TOKEN = "token";
        public const string AUTH_NAME_END = "end";

        public const string CONTENT_TYPE_LABEL = "content-type";
        public const string CONTENT_ACCEPT_LANGUAGE = "Accept-Language";

        public const string STANDARD_CONTENT_TYPE = "application/json";
        public const string STANDARD_ACCEPT_LANGUAGE = "ru-RU";
        public const string STANDARD_AUTHENTICATION_SCHEME = "auth";

        public const string OBJECTTYPE_BIM = "Bim";
        public const string OBJECTTYPE_BIM_ATTRIBUTE_VERSION = "modelVersions";
        public const string OBJECTTYPE_TASKS = "Tasks";
        public const string OBJECTTYPE_SINGLE_TASK = "Task";

        public const string RESPONSE_OBJECT_NAME = "object";
        public const string RESPONSE_COLLECTION_ITEMS_NAME = "items";
        public const string RESPONSE_COOKIES_AUTH_NAME = "auth";
        public const string RESPONSE_COOKIES_EXPIRES_NAME = "expires";
        public const char RESPONSE_COOKIE_VALUES_SEPARATOR = ';';
        public const char RESPONSE_COOKIE_KEY_VALUE_SEPARATOR = '=';
    }
}
