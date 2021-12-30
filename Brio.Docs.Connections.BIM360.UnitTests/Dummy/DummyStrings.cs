using System;

namespace Brio.Docs.Connections.Bim360.UnitTests.Dummy
{
    internal static class DummyStrings
    {
        public static readonly string COMMENT_ID = GetCommentId();

        public static readonly string ISSUE_ID = GetIssueId();

        public static readonly string USER_ID = GetUserId();

        public static readonly string HUB_ID = GetHubId();

        public static readonly string PROJECT_ID = GetProjectId();

        public static readonly string FOLDER_ID = GetFolderId();

        public static readonly string ISSUE_CONTAINER_ID = GetIssueContainerId();

        public static readonly string LOCATION_CONTAINER_ID = GetLocationContainerId();

        public static readonly string BIM_ELEMENT_GLOBAL_ID = GetBimElementGlobalId();

        public static readonly string ITEM_ID = GetItemId();

        public static readonly string USER_NAME = "Dummy user";

        public static string GetCommentId()
            => $"comment-{Guid.NewGuid()}";

        public static string GetIssueId()
            => $"issue-{Guid.NewGuid()}";

        public static string GetUserId()
            => $"user-{Guid.NewGuid()}";

        public static string GetHubId()
            => $"hub-{Guid.NewGuid()}";

        public static string GetProjectId()
            => $"project-{Guid.NewGuid()}";

        public static string GetFolderId()
            => $"folder-{Guid.NewGuid()}";

        public static string GetIssueContainerId()
            => $"issue-container-{Guid.NewGuid()}";

        public static string GetLocationContainerId()
            => $"location-container-{Guid.NewGuid()}";

        public static string GetBimElementGlobalId()
            => $"bim-element-guid-{Guid.NewGuid()}";

        public static string GetItemId()
            => $"item-{Guid.NewGuid()}";
    }
}
