namespace MRS.DocumentManagement.Connection.MrsPro
{
    internal static class Constants
    {
        internal const string STATE_OPENED = "opened";
        internal const string STATE_COMPLETED = "completed";
        internal const string STATE_VERIFIED = "verified ";

        internal static readonly string BASE_URL = "https://service-{0}.plotpad.com/nrs";

        internal static readonly string AUTH_PASS = "password";
        internal static readonly string AUTH_EMAIL = "email";
        internal static readonly string COMPANY_CODE = "companyCode";

        internal static readonly string ISSUE_TYPE = "task";
        internal static readonly string ELEMENT_TYPE = "project";

        internal static readonly string ROOT = ":ORGANIZATION";
        internal static readonly string PROJECT = ":PROJECT";
        internal static readonly string TASK = ":TASK";

        internal static readonly char ID_SPLITTER = ':';
        internal static readonly char ID_PATH_SPLITTER = '/';
        internal static readonly char QUERY_SEPARATOR = ',';

        internal static readonly string OP_REPLACE = "replace";

        internal static string GetByIds(string url)
            => url + "?ids={0}";
    }
}
