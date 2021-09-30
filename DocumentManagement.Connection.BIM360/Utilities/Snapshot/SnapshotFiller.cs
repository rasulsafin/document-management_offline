using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot
{
    internal class SnapshotFiller
    {
        private readonly Bim360Snapshot snapshot;
        private readonly HubsService hubsService;
        private readonly ProjectsService projectsService;
        private readonly IssuesService issuesService;
        private readonly FoldersService foldersService;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly LocationEnumCreator locationEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;
        private readonly IssueSnapshotUtilities snapshotUtilities;

        public SnapshotFiller(
            Bim360Snapshot snapshot,
            HubsService hubsService,
            ProjectsService projectsService,
            IssuesService issuesService,
            FoldersService foldersService,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            AssignToEnumCreator assignToEnumCreator,
            IssueSnapshotUtilities snapshotUtilities)
            LocationEnumCreator locationEnumCreator,
            AssignToEnumCreator assignToEnumCreator)
        {
            this.snapshot = snapshot;
            this.hubsService = hubsService;
            this.projectsService = projectsService;
            this.issuesService = issuesService;
            this.foldersService = foldersService;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
            this.snapshotUtilities = snapshotUtilities;
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
                    var projectSnapshot = new ProjectSnapshot(p, hub.Value);
                    var topFolders = await projectsService.GetTopFoldersAsync(hub.Key, p.ID);

                    if (!topFolders.Any())
                        continue;

                    hub.Value.Projects.Add(p.ID, projectSnapshot);
                    var topFolder = (topFolders.FirstOrDefault(
                            x => x.Attributes.DisplayName == Constants.DEFAULT_PROJECT_FILES_FOLDER_NAME ||
                                x.Attributes.Extension.Data.VisibleTypes.Contains(Constants.AUTODESK_ITEM_FILE_TYPE)) ??
                        topFolders.First()).ID;
                    var items = await GetAllItems(p.ID, topFolders);
                    projectSnapshot.Items = new Dictionary<string, ItemSnapshot>();

                    foreach (var iv in items)
                    {
                        if (iv.item.Attributes.DisplayName != Resources.MrsFileName &&
                            iv.version?.Attributes.Name != Resources.MrsFileName)
                            projectSnapshot.Items.Add(iv.item.ID, new ItemSnapshot(iv.item) { Version = iv.version });
                        else
                            topFolder = iv.item.Relationships.Parent.Data.ID;
                    }

                    projectSnapshot.MrsFolderID = topFolder;
                }
            }
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
                var issues = await issuesService.GetIssuesAsync(project.IssueContainer, filters);
                project.Issues = issues.ToDictionary(x => x.ID, x => new IssueSnapshot(x, project));

                foreach (var issueSnapshot in project.Issues.Values)
                {
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

        private async Task UpdateProjectsEnums<T, TSnapshot, TID>(
            Action<ProjectSnapshot> createEmptyEnumVariants,
            IEnumCreator<T, TSnapshot, TID> creator,
            Action<ProjectSnapshot, TSnapshot> addSnapshot)
            where TSnapshot : AEnumVariantSnapshot<T>
        {
            await UpdateProjectsIfNull();
            var dictionary = new List<TSnapshot>();

            foreach (var project in snapshot.ProjectEnumerable)
            {
                var variants = await creator.GetVariantsFromRemote(project);
                createEmptyEnumVariants(project);
                dictionary.AddRange(variants);
            }

            var groups = DynamicFieldUtilities.GetGroupedTypes(creator, dictionary);

            foreach (var group in groups)
            {
                var externalID = DynamicFieldUtilities.GetExternalID(creator.GetOrderedIDs(group).Distinct());

                foreach (var issueTypeSnapshot in group)
                {
                    issueTypeSnapshot.SetExternalID(externalID);
                    addSnapshot(issueTypeSnapshot.ProjectSnapshot, issueTypeSnapshot);
                }
            }
        }

        private async Task<IEnumerable<(Item item, Version version)>> GetAllItems(
            string projectID,
            IEnumerable<Folder> folders)
        {
            var result = Enumerable.Empty<(Item, Version)>();

            foreach (var folder in folders)
            {
                result = result
                   .Concat(await GetAllItems(projectID, await foldersService.GetFoldersAsync(projectID, folder.ID)))
                   .Concat(await foldersService.GetItemsAsync(projectID, folder.ID));
            }

            return result;
        }
    }
}
