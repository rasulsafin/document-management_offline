using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;

namespace MRS.DocumentManagement.Connection.Synchronizator
{

    public class ObjectiveSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private IMapper mapper;
        private Objective local;
        private ObjectiveDto remote;

        public ObjectiveSynchro(IDiskManager disk, DMContext context, IMapper mapper)
        {
            this.disk = disk;
            this.context = context;
            this.mapper = mapper;
        }

        public async Task DeleteLocal(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                context.Objectives.Remove(local);
                await context.SaveChangesAsync();
            }
        }


        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<ObjectiveDto>(action.ID.ToString());
        }

        public async Task Download(SyncAction action)
        {
            await GetRemote(action.ID);
            await GetLocal(action.ID);
            if (remote != null)
            {
                var exist = local != null;
                if (!exist)
                    local = mapper.Map<Objective>(remote);
                else
                    local = mapper.Map(remote, local);

                await ItemSync();
                await BinElementSync();
                await DynamicFieldSync();

                if (!exist)
                    context.Objectives.Add(local);

                // else
                //    context.Objectives.Attach(local);
                await context.SaveChangesAsync();
            }
        }

        private async Task DynamicFieldSync()
        {
            foreach (var dynamicDto in remote.DynamicFields)
            {
                var dynamic = await context.DynamicFields.FindAsync((int)dynamicDto.ID);
                var existItem = dynamic != null;
                dynamic = mapper.Map<DynamicField>(dynamicDto);
                dynamic.ObjectiveID = local.ID;

                if (!existItem)
                    context.DynamicFields.Add(dynamic);
                else
                    context.DynamicFields.Update(dynamic);

                // local.DynamicFields.Add(dynamic);
                await context.SaveChangesAsync();
            }
        }

        private async Task BinElementSync()
        {
            // Добавим отсутвуюшие BimElement
            if (local.BimElements == null) local.BimElements = new List<BimElementObjective>();
            local.BimElements.Clear();

            foreach (var bimDto in remote.BimElements)
            {
                var bim = await context.BimElements.FirstOrDefaultAsync(x => x.GlobalID == bimDto.GlobalID);
                var existBim = bim != null;
                var item = await context.Items.FindAsync((int)bimDto.ItemID);
                if (item != null)
                {
                    bim = mapper.Map<BimElement>(bimDto);
                    if (!existBim)
                    {
                        context.BimElements.Add(bim);
                    }
                    else
                    {
                        context.BimElements.Update(bim);
                    }
                    await context.SaveChangesAsync();
                    local.BimElements.Add(new BimElementObjective() { ObjectiveID = local.ID, BimElementID = bim.ID });
                }
            }
        }

        private async Task ItemSync()
        {
            // Добавим отсутвуюшие item
            if (local.Items == null) local.Items = new List<ObjectiveItem>();
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
                local.Items.Add(new ObjectiveItem() { ObjectiveID = local.ID, ItemID = item.ID });
            }
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(TableRevision.Objectives);
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            return Task.FromResult<List<ISynchroTable>>(null);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(TableRevision.Objectives, rev.ID).Rev = rev.Rev;
        }

        public async Task Special(SyncAction action)
        {
            //await GetRemote(action.ID);
            //await GetLocal(action.ID);
            //if (remote != null)
            //{
            //    if (local != null)
            //    {
            //        local.Title = remote.Title;
            //    }
            //    else
            //    {
            //        local = new Objective()
            //        {
            //            ID = (int)remote.ID,
            //            Title = remote.Title,
            //        };

            //        // TODO: Надо както сделать получше
            //        // Эта коллекция заполнится при синхронизации ItemDto
            //        local.Items = new List<ObjectiveItem>();
            //        context.Objectives.Add(local);
            //    }
            //}
            throw new NotImplementedException();
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            //if (action.TypeAction == TypeSyncAction.Download || action.TypeAction == TypeSyncAction.Upload)
            //    action.SpecialSynchronization = true;
            return action;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                remote = mapper.Map<ObjectiveDto>(local);

                //if (local.Items != null)
                //{
                //    remote.Items = local.Items?.Select(x => Convert(x.Item));
                //}

                //if (local.BimElements != null)
                //{
                //    remote.BimElements = local.BimElements?.Select(x => Convert(x.BimElement));
                //}

                //if (local.DynamicFields != null)
                //{
                //    remote.DynamicFields = local.DynamicFields?.Select(x => Convert(x));
                //}

                await disk.Push(remote, action.ID.ToString());
            }
        }

        private DynamicFieldDto Convert(DynamicField field)
        {
            return new DynamicFieldDto()
            {
                ID = (ID<DynamicFieldDto>)field.ID,
                Key = field.Key,
                Type = field.Type,
                Value = field.Value,
            };
        }

        private BimElementDto Convert(BimElement bimElement)
        {
            return new BimElementDto()
            {
                GlobalID = bimElement.GlobalID,
                ItemID = (ID<ItemDto>)bimElement.ItemID,
            };
        }

        private ObjectiveDto Convert(Objective obj)
        {
            return new ObjectiveDto()
            {
                ID = new ID<ObjectiveDto>(obj.ID),
                AuthorID = (ID<UserDto>)obj.AuthorID,
                CreationDate = obj.CreationDate,
                DueDate = obj.DueDate,
                ObjectiveTypeID = (ID<ObjectiveTypeDto>)obj.ObjectiveTypeID,
                ParentObjectiveID = (ID<ObjectiveDto>?)obj.ParentObjectiveID,
                ProjectID = (ID<ProjectDto>)obj.ProjectID,
                Status = (ObjectiveStatus)obj.Status,
                Title = obj.Title,
                Description = obj.Description,
            };
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

        private Item Convert(ItemDto item)
        {
            return new Item()
            {
                ID = (int)item.ID,
                ItemType = (int)item.ItemType,
                ExternalItemId = item.ExternalItemId,
                Name = item.Name,
            };
        }

        private async Task GetLocal(int id)
        {
            if (local?.ID != id)
                local = await context.Objectives.FindAsync(id);
        }

        private async Task GetRemote(int id)
        {
            if (remote?.ID != (ID<ObjectiveDto>)id)
                remote = await disk.Pull<ObjectiveDto>(id.ToString());
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Objectives.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(TableRevision.Objectives);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
        }
    }
}
