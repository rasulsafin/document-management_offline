namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal static class MrsConstants
    {
        public static readonly string CONFIG_EXTENSION = ".mrsbc";
        public static readonly string STATUSES_CONFIG_NAME = "statuses";

        public static readonly string ISSUE_TYPE_NAME = "Issue";
        public static readonly string LOCATION_DETAILS_FIELD_NAME = "Location Details";
        public static readonly string RESPONSE_FIELD_NAME = "Response";
        public static readonly string TYPE_FIELD_NAME = "Type";
        public static readonly string ROOT_CAUSE_FIELD_NAME = "Root Cause";
        public static readonly string LOCATION = "Location";
        public static readonly string ASSIGN_TO_FIELD_NAME = "Assign To";
        public static readonly string COMMENT_FIELD_NAME = "Comment";
        public static readonly string NEW_COMMENT_FIELD_NAME = "New Comment";
        public static readonly string AUTHOR_FIELD_NAME = "Author";
        public static readonly string DATE_FIELD_NAME = "Date";
        public static readonly string STATUS_FIELD_NAME = "Status";
        public static readonly string DEFAULT_AUTHOR_NAME = "Unauthorized name";

        public static readonly string DRAFT_STATUS_TITLE = "Draft";
        public static readonly string OPEN_STATUS_TITLE = "Open";
        public static readonly string CLOSED_STATUS_TITLE = "Closed";
        public static readonly string ANSWERED_STATUS_TITLE = "Answered";
        public static readonly string WORK_COMPLETED_STATUS_TITLE = "Work completed";
        public static readonly string READY_TO_INSPECT_STATUS_TITLE = "Ready to inspect";
        public static readonly string NOT_APPROVED_STATUS_TITLE = "Not approved";
        public static readonly string IN_DISPUTE_STATUS_TITLE = "In dispute";

        public static readonly string NEW_COMMENT_ID = "new_comment";

        public static readonly string META_COMMENT_TAG = "#mrs";
        public static readonly string BIM_ELEMENTS_META_COMMENT_TAG = "#be";
        public static readonly string LINKED_INFO_META_COMMENT_TAG = "#li";
        public static readonly int MAX_COMMENT_LENGTH = 1900;
    }
}
