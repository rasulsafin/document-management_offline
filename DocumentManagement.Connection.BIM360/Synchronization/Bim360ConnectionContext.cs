using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
{
    public class Bim360ConnectionContext : AConnectionContext
    {
        private FoldersSyncHelper foldersSyncHelper;

        private Bim360ConnectionContext()
        {
        }

        internal List<Hub> Hubs { get; } = new List<Hub>();

        internal Dictionary<string, (Project, ProjectExternalDto)> Projects { get; } =
            new Dictionary<string, (Project, ProjectExternalDto)>();

        internal Dictionary<string, Folder> DefaultFolders { get; } = new Dictionary<string, Folder>();

        internal IssuesService IssuesService { get; private set; }

        internal HubsService HubsService { get; private set; }

        internal ProjectsService ProjectsService { get; private set; }

        internal ItemsService ItemsService { get; private set; }

        internal FoldersService FoldersService { get; private set; }

        internal ObjectsService ObjectsService { get; private set; }

        internal VersionsService VersionsService { get; private set; }

        public static async Task<Bim360ConnectionContext> CreateContext(ConnectionInfoExternalDto connectionInfo)
        {
            var connection = new ForgeConnection();
            var authService = new AuthenticationService(connection);
            var authenticator = new Authenticator(authService);
            var context = new Bim360ConnectionContext();

            // Authorize
            _ = await authenticator.SignInAsync(connectionInfo);
            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            context.IssuesService = new IssuesService(connection);
            context.HubsService = new HubsService(connection);
            context.ProjectsService = new ProjectsService(connection);
            context.ItemsService = new ItemsService(connection);
            context.FoldersService = new FoldersService(connection);
            context.ObjectsService = new ObjectsService(connection);
            context.VersionsService = new VersionsService(connection);
            context.foldersSyncHelper = new FoldersSyncHelper(context.FoldersService, context.ProjectsService);

            return context;
        }

        internal async Task UpdateProjects(bool mustUpdate)
        {
            await UpdateHubs();
            if (!mustUpdate && Projects.Count != 0)
                return;

            foreach (var hub in Hubs)
            {
                var projectsInHub = await ProjectsService.GetProjectsAsync(hub.ID);

                foreach (var p in projectsInHub)
                {
                    if (Projects.ContainsKey(p.ID))
                        Projects.Remove(p.ID);
                    Projects.Add(p.ID, (p, p.ToDto()));

                    if (!DefaultFolders.ContainsKey(p.ID))
                    {
                        var folder = await foldersSyncHelper.GetDefaultFolderAsync(hub.ID, p.ID);
                        DefaultFolders.Add(p.ID, folder);
                    }
                }
            }
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new Bim360ObjectivesSynchronizer(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new Bim360ProjectsSynchronizer(this);

        private async Task UpdateHubs()
        {
            if (Hubs.Count == 0)
                Hubs.AddRange(await HubsService.GetHubsAsync());
        }
    }
}
