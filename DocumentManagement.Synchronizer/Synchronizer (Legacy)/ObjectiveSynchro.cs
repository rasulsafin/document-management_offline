using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Synchronizer.Legacy
{
    public class ObjectiveSynchro : ISynchroTable
    {
        private ICloudManager disk;
        private DMContext context;
        private IMapper mapper;
        private Objective local;
        private ObjectiveDto remote;

        public ObjectiveSynchro(ICloudManager disk, DMContext context, IMapper mapper)
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
                action.IsComplete = true;
                context.Objectives.Unsynchronized().Remove(local);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteRemote(SyncAction action)
        {
            // await disk.Delete<ObjectiveDto>(action.ID.ToString());
            action.IsComplete = true;
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
                await BimElementSync();
                await DynamicFieldSync();

                if (!exist)
                    context.Objectives.Add(local);
                await context.SaveChangesAsync();
            }
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(NameTypeRevision.Objectives);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(NameTypeRevision.Objectives, rev.ID).Rev = rev.Rev;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                action.IsComplete = true;
                remote = mapper.Map<ObjectiveDto>(local);
                // await disk.Push(remote, action.ID.ToString());
            }
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.Objectives.Unsynchronized().Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(NameTypeRevision.Objectives);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
        }

        private async Task VerifyPrimaryKey(SyncAction action)
        {
            bool projExist = await context.Projects.Unsynchronized().AnyAsync(x => x.ID == (int)remote.ProjectID);
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
                var dynamic = await context.DynamicFields.Unsynchronized().FindAsync((int)dynamicDto.ID);
                var existItem = dynamic != null;
                dynamic = mapper.Map<DynamicField>(dynamicDto);
                dynamic.ObjectiveID = local.ID;

                if (!existItem)
                    context.DynamicFields.Unsynchronized().Add(dynamic);
                else
                    context.DynamicFields.Unsynchronized().Update(dynamic);
                await context.SaveChangesAsync();
            }
        }

        private async Task BimElementSync()
        {
            // Добавим отсутвуюшие BimElement
            if (local.BimElements == null) local.BimElements = new List<BimElementObjective>();
            local.BimElements.Clear();

            foreach (var bimDto in remote.BimElements)
            {
                var bim = await context.BimElements.FirstOrDefaultAsync(x => x.GlobalID == bimDto.GlobalID);
                var existBim = bim != null;

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

        private async Task ItemSync()
        {
            // Добавим отсутвуюшие item
            if (local.Items == null) local.Items = new List<ObjectiveItem>();
            local.Items.Clear();

            foreach (var itemDto in remote.Items)
            {
                var item = await context.Items.Unsynchronized().FindAsync((int)itemDto.ID);
                var existItem = item != null;
                if (!existItem)
                {
                    item = mapper.Map<Item>(itemDto);
                    item.ItemType = (int)itemDto.ItemType;
                    context.Items.Unsynchronized().Add(item);
                }
                else
                {
                    item = mapper.Map(itemDto, item);
                    item.ItemType = (int)itemDto.ItemType;
                    context.Items.Unsynchronized().Update(item);
                }

                await context.SaveChangesAsync();
                local.Items.Add(new ObjectiveItem() { ObjectiveID = local.ID, ItemID = item.ID });
            }
        }

        private async Task GetLocal(int id)
        {
            if (local?.ID != id)
                local = await context.Objectives.Unsynchronized().FindAsync(id);
        }

        private async Task GetRemote(int id)
        {
            //  if (remote?.ID != (ID<ObjectiveDto>)id)
            //   remote = await disk.Pull<ObjectiveDto>(id.ToString());
        }
    }
}
