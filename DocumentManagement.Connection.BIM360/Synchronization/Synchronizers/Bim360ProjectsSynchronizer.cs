﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
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
            var cashed = context.Projects[obj.ExternalID];
            var toRemove = cashed.Item2.Items.Where(a => obj.Items.All(b => b.ExternalID != a.ExternalID)).ToArray();
            var toAdd = obj.Items.Where(a => cashed.Item2.Items.All(b => b.ExternalID != a.ExternalID)).ToArray();

            foreach (var item in toAdd)
            {
                cashed.Item2.Items.Add(
                    (await itemsSyncHelper.PostItem(
                        item,
                        context.DefaultFolders[obj.ExternalID],
                        obj.ExternalID)).ToDto());
            }

            foreach (var dto in toRemove)
                await itemsSyncHelper.Remove(obj.ExternalID, dto);

            cashed.Item2 = await GetFullProject(cashed.Item1);
            context.Projects[cashed.Item1.ID] = cashed;
            return cashed.Item2;
        }

        public async Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            await context.UpdateProjects(false);
            return context.Projects.Keys;
        }

        public async Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var projects = new List<ProjectExternalDto>();

            foreach (var id in ids)
            {
                (Project, ProjectExternalDto) value = default;
                if (context.Projects.TryGetValue(id, out var project))
                {
                    value = project;
                }
                else
                {
                    await context.UpdateProjects(true);
                    if (context.Projects.ContainsKey(id))
                        value = project;
                }

                if (value != default)
                {
                    if ((value.Item2.Items?.Count ?? 0) == 0)
                    {
                        value.Item2 = await GetFullProject(value.Item1);
                        context.Projects[value.Item1.ID] = value;
                    }

                    projects.Add(value.Item2);
                }
            }

            return projects;
        }

        private async Task<ProjectExternalDto> GetFullProject(Project project)
        {
            var dto = project.ToDto();
            if (!context.DefaultFolders.TryGetValue(project.ID, out var folder))
                return dto;
            var items = await foldersService.GetItemsAsync(project.ID, folder.ID);
            dto.Items = items.Select(x => x.ToDto()).ToList();
            return dto;
        }
    }
}
