﻿using System;
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
    public class ObjectiveTypeSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private IMapper mapper;
        private Objective local;
        private ObjectiveDto remote;

        public ObjectiveTypeSynchro(IDiskManager disk, DMContext context, IMapper mapper)
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
                action.IsComplete = true;
                await VerifyPrimaryKey(action);
                if (!action.IsComplete)
                    return;

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
                await context.SaveChangesAsync();

            }
        }

        private async Task VerifyPrimaryKey(SyncAction action)
        {
            bool projExist = await context.Projects.AnyAsync(x => x.ID == (int)remote.ProjectID);
            if (!projExist)
            {
                action.IsComplete = false;
                action.Data = remote;
                return;
            }

            bool userExist = await context.Users.AnyAsync(x => x.ID == (int)remote.AuthorID);
            if (!userExist)
            {
                action.IsComplete = false;
                action.Data = remote;
                return;
            }

            bool objectiveTypeExist = await context.ObjectiveTypes.AnyAsync(x => x.ID == (int)remote.ObjectiveTypeID);
            if (!objectiveTypeExist)
            {
                action.IsComplete = false;
                action.Data = remote;
                return;
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
            throw new NotImplementedException();
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            return action;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                remote = mapper.Map<ObjectiveDto>(local);
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
