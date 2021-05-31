using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
{
    public class Bim360ConnectionContext : AConnectionContext
    {
        private readonly HubsService hubsService;
        private readonly ProjectsService projectsService;
        private readonly FoldersSyncHelper foldersSyncHelper;
        private readonly Bim360ProjectsSynchronizer projectSynchronizer;
        private readonly Bim360ObjectivesSynchronizer objectiveSynchronizer;

        public Bim360ConnectionContext(
            HubsService hubsService,
            ProjectsService projectsService,
            FoldersSyncHelper foldersSyncHelper,
            Bim360ProjectsSynchronizer projectSynchronizer,
            Bim360ObjectivesSynchronizer objectiveSynchronizer)
        {
            this.hubsService = hubsService;
            this.projectsService = projectsService;
            this.foldersSyncHelper = foldersSyncHelper;
            this.projectSynchronizer = projectSynchronizer;
            this.objectiveSynchronizer = objectiveSynchronizer;
        }

        internal Bim360Snapshot Snapshot { get; set; }

        internal async Task UpdateProjects(bool mustUpdate)
        {
            await UpdateHubs();
            if (!mustUpdate && Snapshot.Hubs.All(x => x.Value.Projects != null))
                return;

            foreach (var hub in Snapshot.Hubs)
            {
                var projectsInHub = await projectsService.GetProjectsAsync(hub.Key);

                foreach (var p in projectsInHub.Where(p => p.Attributes.Name != INTEGRATION_TEST_PROJECT))
                {
                    if (hub.Value.Projects.ContainsKey(p.ID))
                        hub.Value.Projects.Remove(p.ID);
                    var projectSnapshot = new ProjectSnapshot(p);
                    hub.Value.Projects.Add(p.ID, projectSnapshot);
                    projectSnapshot.IssueContainer = p.Relationships.IssuesContainer.Data.ID;
                    var folder = await foldersSyncHelper.GetDefaultFolderAsync(
                        hub.Key,
                        p.ID,
                        folder1 => folder1.Attributes.DisplayName == "Project Files");
                    projectSnapshot.ProjectFilesFolder = folder;
                }
            }
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => objectiveSynchronizer;

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => projectSynchronizer;

        private async Task UpdateHubs()
        {
            if (Snapshot.Hubs != null)
                return;

            Snapshot.Hubs = new Dictionary<string, HubSnapshot>();
            var hubs = await hubsService.GetHubsAsync();
            foreach (var hub in hubs)
                Snapshot.Hubs.Add(hub.ID, new HubSnapshot(hub));
        }
    }
}
