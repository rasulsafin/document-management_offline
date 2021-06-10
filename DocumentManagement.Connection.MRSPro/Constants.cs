namespace MRS.DocumentManagement.Connection.MrsPro
{
    public static class Constants
    {
        public static readonly string BASE_URL = "https://service-{0}.plotpad.com";

        public static readonly string AUTH_PASS = "password";
        public static readonly string AUTH_EMAIL = "email";
        public static readonly string COMPANY_CODE = "companyCode";

        public static readonly string ISSUE_TYPE = "task";
        public static readonly string ELEMENT_TYPE = "project";

        public static readonly string ROOT = ":ORGANIZATION";
        public static readonly string PROJECT = ":PROJECT";

        public static readonly char ID_SPLITTER = ':';
    }
}
