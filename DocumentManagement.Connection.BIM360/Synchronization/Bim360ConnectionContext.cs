using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
{
    public class Bim360ConnectionContext : AConnectionContext
    {
        internal readonly IssuesService IssuesService;
        internal readonly HubsService HubsService;
        internal readonly ProjectsService ProjectsService;
        internal readonly ItemsService ItemsService;
        internal readonly FoldersService FoldersService;
        internal readonly ObjectsService ObjectsService;

        private readonly ForgeConnection connection;

        private List<Hub> hubs;
        private List<Project> bimProjects;

        public Bim360ConnectionContext(
            ForgeConnection connection,
            IssuesService issuesService,
            HubsService hubsService,
            ProjectsService projectsService,
            ItemsService itemsService,
            FoldersService foldersService,
            ObjectsService objectsService)
        {
            this.connection = connection;
            IssuesService = issuesService;
            HubsService = hubsService;
            ProjectsService = projectsService;
            ItemsService = itemsService;
            FoldersService = foldersService;
            ObjectsService = objectsService;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new Bim360ObjectivesSynchronizer(connection, this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
         => new Bim360ProjectsSynchronizer(connection, this);

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
                    root.ID,
                    Enumerable.Empty<(string filteringField, string filteringValue)>());
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
