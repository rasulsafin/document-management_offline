using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronizers
{
    public class Bim360ProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly Bim360ConnectionContext context;
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly FoldersService foldersService;
        private readonly FoldersSyncHelper foldersSyncHelper;

        public Bim360ProjectsSynchronizer(
            Bim360ConnectionContext context,
            ItemsSyncHelper itemsSyncHelper,
            FoldersService foldersService)
        {
            this.context = context;
            this.itemsSyncHelper = itemsSyncHelper;
            this.foldersService = foldersService;
        }

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
            => throw new NotSupportedException();

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
            => throw new NotSupportedException();

        public async Task<ProjectExternalDto> Update(ProjectExternalDto obj)
        {
            var cashed = context.Snapshot.ProjectEnumerable.First(x => x.Key == obj.ExternalID);
            var toRemove = cashed.Value.Items.Where(a => obj.Items.All(b => b.ExternalID != a.Value.Entity.ID))
               .ToArray();
            var toAdd = obj.Items.Where(a => cashed.Value.Items.All(b => b.Value.Entity.ID != a.ExternalID)).ToArray();

            foreach (var item in toAdd)
            {
                var posted = await itemsSyncHelper.PostItem(
                    item,
                    context.Snapshot.ProjectEnumerable.First(x => x.Key == obj.ExternalID).Value.ProjectFilesFolder,
                    obj.ExternalID);
                cashed.Value.Items.Add(posted.ID, new ItemSnapshot(posted));
            }

            foreach (var dto in toRemove)
            {
                cashed.Value.Items.Remove(dto.Key);
                await itemsSyncHelper.Remove(obj.ExternalID, dto.Value.Entity);
            }

            return await GetFullProject(cashed.Value);
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await context.UpdateProjects(false);
            return context.Snapshot.ProjectEnumerable.Select(x => x.Key).ToList();
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var projects = new List<ProjectExternalDto>();

            foreach (var id in ids)
            {
                var project = context.Snapshot.ProjectEnumerable.FirstOrDefault(x => x.Key == id).Value;

                if (project != null)
                {
                    var folder = project.ProjectFilesFolder;
                    var items = await foldersService.GetItemsAsync(project.ID, folder.ID);
                    project.Items = new Dictionary<string, ItemSnapshot>();
                    foreach (var item in items)
                        project.Items.Add(item.ID, new ItemSnapshot(item));
                    projects.Add(await GetFullProject(project));
                }
            }

            return projects;
        }

        private async Task<ProjectExternalDto> GetFullProject(ProjectSnapshot project)
        {
            var dto = project.Entity.ToDto();
            var folder = project.ProjectFilesFolder;
            var items = await foldersService.GetItemsAsync(project.ID, folder.ID);
            dto.Items = items.Select(x => x.ToDto()).ToList();
            return dto;
        }
    }
}
