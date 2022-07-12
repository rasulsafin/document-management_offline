using System.Collections.Generic;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Synchronization.Models.StatusRelations;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.UnitTests.Dummy
{
    internal static class DummySnapshots
    {
        public static Bim360Snapshot Bim360Snapshot
            => new ()
            {
                Hubs = new Dictionary<string, HubSnapshot>(),
            };

        public static HubSnapshot Hub
            => new (DummyModels.Hub)
            {
                Projects = new Dictionary<string, ProjectSnapshot>(),
            };

        public static ProjectSnapshot CreateProject(HubSnapshot hub)
            => new (DummyModels.Project, hub)
            {
                Issues = new Dictionary<string, IssueSnapshot>(),
                IssueTypes = new Dictionary<string, IssueTypeSnapshot>(),
                RootCauses = new Dictionary<string, RootCauseSnapshot>(),
                Locations = new Dictionary<string, LocationSnapshot>(),
                AssignToVariants = new Dictionary<string, AssignToVariant>(),
                Statuses = new Dictionary<string, StatusSnapshot>(),
                Items = new Dictionary<string, ItemSnapshot>(),
                UploadFolderID = DummyStrings.FOLDER_ID,
                StatusesRelations = new StatusesRelations
                {
                    Get = System.Array.Empty<RelationRule<Status, ObjectiveStatus>>(),
                    Set = System.Array.Empty<RelationRule<ObjectiveStatus, Status>>(),
                },
            };

        public static IssueSnapshot CreateIssue(ProjectSnapshot project)
            => new (DummyModels.Issue, project)
            {
                Attachments = new Dictionary<string, Attachment>(),
                Comments = new List<CommentSnapshot>(),
            };
    }
}
