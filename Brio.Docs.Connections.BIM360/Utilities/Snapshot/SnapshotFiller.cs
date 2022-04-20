using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;

namespace Brio.Docs.Connections.Bim360.Utilities.Snapshot
{
    internal class SnapshotFiller
    {
        private readonly Bim360Snapshot snapshot;
        private readonly HubsService hubsService;
        private readonly ProjectsService projectsService;
        private readonly IIssuesService issuesService;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly LocationEnumCreator locationEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;
        private readonly IssueSnapshotUtilities snapshotUtilities;
        private readonly ConfigurationsHelper configUtilities;
        private readonly StatusEnumCreator statusEnumCreator;
        private readonly ProjectSnapshotUtilities projectSnapshotUtilities;

        public SnapshotFiller(
            Bim360Snapshot snapshot,
            HubsService hubsService,
            ProjectsService projectsService,
            IIssuesService issuesService,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            AssignToEnumCreator assignToEnumCreator,
            LocationEnumCreator locationEnumCreator,
            IssueSnapshotUtilities snapshotUtilities,
            ConfigurationsHelper configUtilities,
            StatusEnumCreator statusEnumCreator,
            ProjectSnapshotUtilities projectSnapshotUtilities)
        {
            this.snapshot = snapshot;
            this.hubsService = hubsService;
            this.projectsService = projectsService;
            this.issuesService = issuesService;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
            this.snapshotUtilities = snapshotUtilities;
            this.configUtilities = configUtilities;
            this.statusEnumCreator = statusEnumCreator;
            this.projectSnapshotUtilities = projectSnapshotUtilities;
        }

        public bool IgnoreTestEntities { private get; set; } = true;

        public async Task UpdateHubsIfNull()
        {
            if (snapshot.Hubs != null)
                return;

            snapshot.Hubs = new Dictionary<string, HubSnapshot>();
            var hubs = await hubsService.GetHubsAsync();
            foreach (var hub in hubs)
                snapshot.Hubs.Add(hub.ID, new HubSnapshot(hub));
        }

        public async Task UpdateProjectsIfNull()
        {
            await UpdateHubsIfNull();

            foreach (var hub in snapshot.Hubs.Where(hub => hub.Value.Projects == null))
            {
                var projectsInHub = await projectsService.GetProjectsAsync(hub.Key);
                hub.Value.Projects = new Dictionary<string, ProjectSnapshot>();

                foreach (var p in IgnoreTestEntities
                    ? projectsInHub.Where(p => p.Attributes.Name != Constants.INTEGRATION_TEST_PROJECT)
                    : projectsInHub)
                {
                    if (hub.Value.Projects.ContainsKey(p.ID))
                        hub.Value.Projects.Remove(p.ID);
                    var projectSnapshot = await projectSnapshotUtilities.GetFullProjectSnapshot(hub, p);
                    if (projectSnapshot != null)
                        hub.Value.Projects.Add(p.ID, projectSnapshot);
                }
            }
        }

        public async Task UpdateStatusesConfigIfNull()
        {
            await UpdateProjectsIfNull();
            foreach (var project in snapshot.ProjectEnumerable.Where(x => x.StatusesRelations == null))
                project.StatusesRelations = await configUtilities.GetStatusesConfig(project);
        }

        public async Task UpdateIssuesIfNull(DateTime date = default)
        {
            await UpdateProjectsIfNull();

            foreach (var project in snapshot.ProjectEnumerable.Where(x => x.Issues == null))
            {
                var filters = new List<Filter>();

                if (date != default)
                {
                    var updatedFilter = new Filter(Constants.FILTER_KEY_ISSUE_UPDATED_AFTER, date.ToString("O"));
                    filters.Add(updatedFilter);
                }

                filters.Add(IssueUtilities.GetFilterForUnremoved());
                var issues = issuesService.GetIssuesAsync(project.IssueContainer, filters);
                project.Issues = new Dictionary<string, IssueSnapshot>();

                await foreach (var issue in issues)
                {
                    var issueSnapshot = new IssueSnapshot(issue, project);
                    project.Issues.Add(issue.ID, issueSnapshot);
                    issueSnapshot.Attachments = await snapshotUtilities.GetAttachments(issueSnapshot, project);
                    issueSnapshot.Comments = await snapshotUtilities.GetComments(issueSnapshot, project);
                }
            }
        }

        public async Task UpdateIssueTypes()
            => await UpdateProjectsEnums(
                p => p.IssueTypes = new Dictionary<string, IssueTypeSnapshot>(),
                subtypeEnumCreator,
                (project, variant) => project.IssueTypes.Add(variant.Entity.ID, variant));

        public async Task UpdateRootCauses()
            => await UpdateProjectsEnums(
                p => p.RootCauses = new Dictionary<string, RootCauseSnapshot>(),
                rootCauseEnumCreator,
                (project, variant) => project.RootCauses.Add(variant.Entity.ID, variant));

        public async Task UpdateLocations()
            => await UpdateProjectsEnums(
                p => p.Locations = new Dictionary<string, LocationSnapshot>(),
                locationEnumCreator,
                (project, variant) => project.Locations.Add(variant.Entity.ID, variant));

        public async Task UpdateAssignTo()
            => await UpdateProjectsEnums(
                p => p.AssignToVariants = new Dictionary<string, AssignToVariant>(),
                assignToEnumCreator,
                (project, variant) => project.AssignToVariants.Add(variant.Entity, variant));

        public async Task UpdateStatuses()
            => await UpdateProjectsEnums(
                p => p.Statuses = new Dictionary<string, StatusSnapshot>(),
                statusEnumCreator,
                (project, variant) => project.Statuses.Add(variant.Entity.GetEnumMemberValue(), variant));

        private async Task UpdateProjectsEnums<T, TSnapshot, TID>(
            Action<ProjectSnapshot> createEmptyEnumVariants,
            IEnumCreator<T, TSnapshot, TID> creator,
            Action<ProjectSnapshot, TSnapshot> addSnapshot)
            where TSnapshot : AEnumVariantSnapshot<T>
        {
            await UpdateProjectsIfNull();

            foreach (var project in snapshot.ProjectEnumerable)
                createEmptyEnumVariants(project);

            var variants = new List<TSnapshot>();

            foreach (var projectSnapshot in snapshot.ProjectEnumerable)
            {
                await foreach (var element in  creator.GetVariantsFromRemote(projectSnapshot))
                    variants.Add(element);
            }

            var groups = DynamicFieldUtilities.GetGroupedVariants(creator, variants);

            foreach (var group in groups)
            {
                var groupArray = group.ToArray();
                var externalID = DynamicFieldUtilities.GetExternalID(creator.GetOrderedIDs(groupArray).Distinct());

                foreach (var issueTypeSnapshot in groupArray)
                {
                    issueTypeSnapshot.SetExternalID(externalID);
                    addSnapshot(issueTypeSnapshot.ProjectSnapshot, issueTypeSnapshot);
                }
            }
        }
    }
}
