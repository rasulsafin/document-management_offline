using System;

namespace Brio.Docs.Connections.Bim360.UnitTests.Dummy
{
    internal static class DummyStrings
    {
        public static readonly string COMMENT_ID = $"comment-{Guid.NewGuid()}";

        public static readonly string ISSUE_ID = $"issue-{Guid.NewGuid()}";

        public static readonly string USER_ID = $"user-{Guid.NewGuid()}";

        public static readonly string HUB_ID = $"hub-{Guid.NewGuid()}";

        public static readonly string PROJECT_ID = $"project-{Guid.NewGuid()}";

        public static readonly string FOLDER_ID = $"folder-{Guid.NewGuid()}";

        public static readonly string ISSUE_CONTAINER_ID = $"issue-container-{Guid.NewGuid()}";

        public static readonly string LOCATION_CONTAINER_ID = $"location-container-{Guid.NewGuid()}";

        public static readonly string BIM_ELEMENT_GLOBAL_ID = $"bim-element-guid-{Guid.NewGuid()}";

        public static readonly string ITEM_ID = $"item-{Guid.NewGuid()}";
    }
}
