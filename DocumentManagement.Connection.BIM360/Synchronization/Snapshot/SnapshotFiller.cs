using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Properties;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot
{
    internal class SnapshotFiller : IBim360SnapshotFiller
    {
        private readonly Bim360ConnectionContext context;
        private readonly HubsService hubsService;
        private readonly ProjectsService projectsService;
        private readonly IssuesService issuesService;
        private readonly FoldersService foldersService;
        private readonly TypeDFHelper typeDfHelper;

        public SnapshotFiller(
            Bim360ConnectionContext context,
            HubsService hubsService,
            ProjectsService projectsService,
            IssuesService issuesService,
            FoldersService foldersService,
            TypeDFHelper typeDfHelper)
        {
            this.context = context;
            this.hubsService = hubsService;
            this.projectsService = projectsService;
            this.issuesService = issuesService;
            this.foldersService = foldersService;
            this.typeDfHelper = typeDfHelper;
        }

        public bool IgnoreTestEntities { private get; set; } = true;

        private Bim360Snapshot Snapshot
        {
            get => context?.Snapshot;
            set => context.Snapshot = value;
        }

        public async Task UpdateHubsIfNull()
        {
            if (Snapshot.Hubs != null)
                return;

            Snapshot = new Bim360Snapshot { Hubs = new Dictionary<string, HubSnapshot>() };
            var hubs = await hubsService.GetHubsAsync();
            foreach (var hub in hubs)
                Snapshot.Hubs.Add(hub.ID, new HubSnapshot(hub));
        }

        public async Task UpdateProjectsIfNull()
        {
            foreach (var hub in Snapshot.Hubs.Where(hub => hub.Value.Projects == null))
            {
                var projectsInHub = await projectsService.GetProjectsAsync(hub.Key);
                hub.Value.Projects = new Dictionary<string, ProjectSnapshot>();

                foreach (var p in IgnoreTestEntities
                    ? projectsInHub.Where(p => p.Attributes.Name != Constants.INTEGRATION_TEST_PROJECT)
                    : projectsInHub)
                {
                    if (hub.Value.Projects.ContainsKey(p.ID))
                        hub.Value.Projects.Remove(p.ID);
                    var projectSnapshot = new ProjectSnapshot(p);
                    hub.Value.Projects.Add(p.ID, projectSnapshot);
                    var topFolders = await projectsService.GetTopFoldersAsync(hub.Key, p.ID);
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
            foreach (var project in context.Snapshot.ProjectEnumerable.Where(x => x.Value.Issues == null))
            {
                var filters = new List<Filter>();

                if (date != default)
                {
                    var updatedFilter = new Filter(Constants.FILTER_KEY_ISSUE_UPDATED_AFTER, date.ToString("O"));
                    filters.Add(updatedFilter);
                }

                filters.Add(IssueUtilities.GetFilterForUnremoved());
                var issues = await issuesService.GetIssuesAsync(project.Value.IssueContainer, filters);
                project.Value.Issues = issues.ToDictionary(x => x.ID, x => new IssueSnapshot(x, project.Value));
            }
        }

        public async Task UpdateIssueTypes()
        {
            var dictionary =
                new Dictionary<IssueSubtype, (ProjectSnapshot projectSnapshot, IssueTypeSnapshot snapshot)>();

            foreach (var project in context.Snapshot.ProjectEnumerable.Select(x => x.Value))
            {
                var types = await issuesService.GetIssueTypesAsync(project.IssueContainer);

                foreach (var info in types.SelectMany(
                    type => type.Subtypes.Select(
                        subtype => (project, snapshot: new IssueTypeSnapshot(type, subtype)))))
                    dictionary.Add(info.snapshot.Entity, info);

                project.IssueTypes = new Dictionary<string, IssueTypeSnapshot>();
            }

            var groups = TypeDFHelper.GetGroupedTypes(
                dictionary.Select(x => (x.Value.snapshot.ParentType, x.Value.snapshot.Entity)));

            foreach (var info in groups)
            {
                var externalID = TypeDFHelper.GetExternalID(info);

                foreach (var infoType in info)
                {
                    (ProjectSnapshot projectSnapshot, IssueTypeSnapshot snapshot) = dictionary[infoType.subtype];
                    snapshot.SetExternalID(externalID);
                    projectSnapshot.IssueTypes.Add(infoType.subtype.ID, snapshot);
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
