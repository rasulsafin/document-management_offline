using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Factories;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
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

        // TODO: replace to an interface.
        private readonly ProjectSynchronizerFactory projectSynchronizerFactory;
        private readonly ObjectiveSynchronizerFactory objectiveSynchronizerFactory;

        public Bim360ConnectionContext(
            HubsService hubsService,
            ProjectsService projectsService,
            FoldersSyncHelper foldersSyncHelper,
            ProjectSynchronizerFactory projectSynchronizerFactory,
            ObjectiveSynchronizerFactory objectiveSynchronizerFactory)
        {
            this.hubsService = hubsService;
            this.projectsService = projectsService;
            this.foldersSyncHelper = foldersSyncHelper;
            this.projectSynchronizerFactory = projectSynchronizerFactory;
            this.objectiveSynchronizerFactory = objectiveSynchronizerFactory;
        }

        internal List<Hub> Hubs { get; } = new List<Hub>();

        internal Dictionary<string, (Project, ProjectExternalDto)> Projects { get; } =
            new Dictionary<string, (Project, ProjectExternalDto)>();

        internal Dictionary<string, Folder> DefaultFolders { get; } = new Dictionary<string, Folder>();

        internal async Task UpdateProjects(bool mustUpdate)
        {
            await UpdateHubs();
            if (!mustUpdate && Projects.Count != 0)
                return;

            foreach (var hub in Hubs)
            {
                var projectsInHub = await projectsService.GetProjectsAsync(hub.ID);

                foreach (var p in projectsInHub.Where(p => p.Attributes.Name != INTEGRATION_TEST_PROJECT))
                {
                    if (Projects.ContainsKey(p.ID))
                        Projects.Remove(p.ID);
                    Projects.Add(p.ID, (p, p.ToDto()));

                    if (!DefaultFolders.ContainsKey(p.ID))
                    {
                        var folder = await foldersSyncHelper.GetDefaultFolderAsync(
                            hub.ID,
                            p.ID,
                            folder1 => folder1.Attributes.DisplayName == "Project Files");
                        DefaultFolders.Add(p.ID, folder);
                    }
                }
            }
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => objectiveSynchronizerFactory.Create(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => projectSynchronizerFactory.Create(this);

        private async Task UpdateHubs()
        {
            if (Hubs.Count == 0)
                Hubs.AddRange(await hubsService.GetHubsAsync());
        }
    }
}
