using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.General.Utils.Factories;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
{
    public class Bim360ConnectionContext : AConnectionContext, IDisposable
    {
        private readonly HubsService hubsService;
        private readonly ProjectsService projectsService;
        private readonly FoldersSyncHelper foldersSyncHelper;
        private readonly IFactory<Bim360ProjectsSynchronizer> projectSynchronizer;
        private readonly IFactory<Bim360ObjectivesSynchronizer> objectiveSynchronizer;

        private bool isDisposed = false;

        public Bim360ConnectionContext(
            HubsService hubsService,
            ProjectsService projectsService,
            FoldersSyncHelper foldersSyncHelper,
            IFactory<Bim360ProjectsSynchronizer> projectSynchronizer,
            IFactory<Bim360ObjectivesSynchronizer> objectiveSynchronizer)
        {
            this.hubsService = hubsService;
            this.projectsService = projectsService;
            this.foldersSyncHelper = foldersSyncHelper;
            this.projectSynchronizer = projectSynchronizer;
            this.objectiveSynchronizer = objectiveSynchronizer;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            GC.SuppressFinalize(this);
            isDisposed = true;
            Scope.Dispose();
        }

        internal IServiceScope Scope { get; set; }

        internal Bim360Snapshot Snapshot { get; set; }

        internal async Task UpdateProjects(bool mustUpdate)
        {
            await UpdateHubs();
            if (!mustUpdate && Snapshot.Hubs.All(x => x.Value.Projects != null))
                return;

            foreach (var hub in Snapshot.Hubs)
            {
                var projectsInHub = await projectsService.GetProjectsAsync(hub.Key);
                hub.Value.Projects = new Dictionary<string, ProjectSnapshot>();

                foreach (var p in projectsInHub.Where(p => p.Attributes.Name != INTEGRATION_TEST_PROJECT))
                {
                    if (hub.Value.Projects.ContainsKey(p.ID))
                        hub.Value.Projects.Remove(p.ID);
                    var projectSnapshot = new ProjectSnapshot(p);
                    hub.Value.Projects.Add(p.ID, projectSnapshot);
                    projectSnapshot.IssueContainer = p.Relationships.IssuesContainer.Data.ID;
                    projectSnapshot.ProjectFilesFolder = await foldersSyncHelper.GetDefaultFolderAsync(
                        hub.Key,
                        p.ID,
                        folder1 => folder1.Attributes.DisplayName == "Project Files");
                }
            }
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => objectiveSynchronizer.Create();

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => projectSynchronizer.Create();

        private async Task UpdateHubs()
        {
            Snapshot ??= new Bim360Snapshot();

            if (Snapshot.Hubs != null)
                return;

            Snapshot.Hubs = new Dictionary<string, HubSnapshot>();
            var hubs = await hubsService.GetHubsAsync();
            foreach (var hub in hubs)
                Snapshot.Hubs.Add(hub.ID, new HubSnapshot(hub));
        }
    }
}
