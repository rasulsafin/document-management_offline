using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge;
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
        private List<Hub> hubs;
        private List<Project> bimProjects;

        private Bim360ConnectionContext()
        {
        }

        internal IssuesService IssuesService { get; private set; }

        internal HubsService HubsService { get; private set; }

        internal ProjectsService ProjectsService { get; private set; }

        internal ItemsService ItemsService { get; private set; }

        internal FoldersService FoldersService { get; private set; }

        internal ObjectsService ObjectsService { get; private set; }

        public static async Task<Bim360ConnectionContext> CreateContext(ConnectionInfoExternalDto connectionInfo, DateTime lastSynchronizationDate)
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

            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new Bim360ObjectivesSynchronizer(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new Bim360ProjectsSynchronizer(this);

        protected override async Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives()
        {
            objectives = new List<ObjectiveExternalDto>();

            foreach (var project in await GetProjectsPrivate())
            {
                var issues = await IssuesService.GetIssuesAsync(project.Relationships.IssuesContainer.Data.ID);

                foreach (var issue in issues)
                {
                    var dto = issue.ToExternalDto();
                    dto.Items ??= new List<ItemExternalDto>();

                    var attachments = await IssuesService.GetAttachmentsAsync(
                        project.Relationships.IssuesContainer.Data.ID,
                        issue.ID);

                    foreach (var attachment in attachments)
                        dto.Items.Add((await ItemsService.GetAsync(project.ID, attachment.Attributes.Urn)).ToDto());

                    objectives.Add(dto);
                }
            }

            return objectives;
        }

        protected override async Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects()
        {
            projects = new List<ProjectExternalDto>();

            foreach (var project in await GetProjectsPrivate())
            {
                var dto = project.ToDto();
                var root = project.Relationships.RootFolder.Data;
                var items = await FoldersService.SearchAsync(
                    project.ID,
                    root.ID);
                dto.Items = items.Select(x => x.item.ToDto()).ToList();
                projects.Add(dto);
            }

            return projects;
        }

        private async Task<List<Project>> GetProjectsPrivate()
        {
            if (bimProjects == null)
            {
                bimProjects = new List<Project>();
                foreach (var hub in await GetHubs())
                    bimProjects.AddRange(await ProjectsService.GetProjectsAsync(hub.ID));
            }

            return bimProjects;
        }

        private async Task<List<Hub>> GetHubs()
            => hubs ??= await HubsService.GetHubsAsync();
    }
}
