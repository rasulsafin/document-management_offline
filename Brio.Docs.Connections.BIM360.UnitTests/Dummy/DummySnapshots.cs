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
        {
            get
            {
                var hub = Hub;
                return new Bim360Snapshot
                {
                    Hubs = new Dictionary<string, HubSnapshot>
                    {
                        [hub.ID] = hub,
                    },
                };
            }
        }

        public static HubSnapshot Hub
        {
            get
            {
                var hub = new HubSnapshot(DummyModels.Hub)
                {
                    Projects = new Dictionary<string, ProjectSnapshot>(),
                };
                var project = CreateProject(hub);
                hub.Projects.Add(project.ID, project);
                return hub;
            }
        }

        public static ProjectSnapshot CreateProject(HubSnapshot hub)
        {
            var project = new ProjectSnapshot(DummyModels.Project, hub)
            {
                Issues = new Dictionary<string, IssueSnapshot>(),
                IssueTypes = new Dictionary<string, IssueTypeSnapshot>(),
                RootCauses = new Dictionary<string, RootCauseSnapshot>(),
                Locations = new Dictionary<string, LocationSnapshot>(),
                AssignToVariants = new Dictionary<string, AssignToVariant>(),
                Statuses = new Dictionary<string, StatusSnapshot>(),
                Items = new Dictionary<string, ItemSnapshot>(),
                MrsFolderID = DummyStrings.FOLDER_ID,
                StatusesRelations = new StatusesRelations
                {
                    Get = System.Array.Empty<RelationRule<Status, ObjectiveStatus>>(),
                    Set = System.Array.Empty<RelationRule<ObjectiveStatus, Status>>(),
                },
            };
            var issue = CreateIssue(project);
            project.Issues.Add(issue.ID, issue);
            return project;
        }

        public static IssueSnapshot CreateIssue(ProjectSnapshot project)
            => new (DummyModels.Issue, project)
            {
                Attachments = new Dictionary<string, Attachment>(),
                Comments = new List<CommentSnapshot>(),
            };
    }
}
