﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ProjectSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private Project local;
        private ProjectDto remote;
        private IMapper mapper;

        public ProjectSynchro(IDiskManager disk, DMContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.disk = disk;
            this.context = context;
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Projects.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(TableRevision.Projects);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
        }

        public async Task DeleteLocal(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
                context.Projects.Remove(local);
            action.IsComplete = true;
        }

        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<ProjectDto>(action.ID.ToString());
            action.IsComplete = true;
        }

        public async Task Download(SyncAction action)
        {
            await GetRemote(action.ID);
            await GetLocal(action.ID);
            if (remote != null)
            {
                action.IsComplete = true;
                var exist = local != null;
                if (!exist)
                    local = mapper.Map<Project>(remote);
                else
                    local = mapper.Map(remote, local);

                await ItemSync();

                if (!exist)
                    context.Projects.Add(local);
                await context.SaveChangesAsync();
            }
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(TableRevision.Projects).Select(pr => (Revision)pr).ToList();
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(TableRevision.Projects, rev.ID).Rev = rev.Rev;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                action.IsComplete = true;
                remote = new ProjectDto()
                {
                    ID = new ID<ProjectDto>(local.ID),
                    Title = local.Title,
                };
                if (local.Items == null)
                {
                    remote.Items = new List<ItemDto>();
                }
                else
                {
                    remote.Items = local.Items?.Select(x => Convert(x.Item));
                }

                await disk.Push(remote, action.ID.ToString());
            }
        }

        private ItemDto Convert(Item item)
        {
            return new ItemDto()
            {
                ID = new ID<ItemDto>(item.ID),
                ExternalItemId = item.ExternalItemId,
                ItemType = (ItemTypeDto)item.ItemType,
                Name = item.Name,
            };
        }

        private async Task GetLocal(int id)
        {
            if (local?.ID != id)
                local = await context.Projects.FindAsync(id);
        }

        private async Task GetRemote(int id)
        {
            if (remote?.ID != (ID<ProjectDto>)id)
                remote = await disk.Pull<ProjectDto>(id.ToString());
        }

        private async Task ItemSync()
        {
            // Добавим отсутвуюшие item
            if (local.Items == null) local.Items = new List<ProjectItem>();
            local.Items.Clear();

            foreach (var itemDto in remote.Items)
            {
                var item = await context.Items.FindAsync((int)itemDto.ID);
                var existItem = item != null;
                if (!existItem)
                {
                    item = mapper.Map<Item>(itemDto);
                    item.ItemType = (int)itemDto.ItemType;
                    context.Items.Add(item);
                }
                else
                {
                    item = mapper.Map(itemDto, item);
                    item.ItemType = (int)itemDto.ItemType;
                    context.Items.Update(item);
                }

                await context.SaveChangesAsync();
                local.Items.Add(new ProjectItem() { ProjectID = local.ID, ItemID = item.ID });
            }
        }
    }
}
