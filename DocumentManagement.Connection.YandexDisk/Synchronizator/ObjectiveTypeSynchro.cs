using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ObjectiveTypeSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private IMapper mapper;
        private ObjectiveType local;
        private ObjectiveTypeDto remote;

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
                context.ObjectiveTypes.Remove(local);
                await context.SaveChangesAsync();
            }

            action.IsComplete = true;
        }

        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<ObjectiveTypeDto>(action.ID.ToString());
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
                    local = mapper.Map<ObjectiveType>(remote);
                else
                    local = mapper.Map(remote, local);

                if (!exist)
                    context.ObjectiveTypes.Add(local);
                await context.SaveChangesAsync();
                action.IsComplete = true;
            }
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.GetRevisions(TableRevision.ObjectiveTypes);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetRevision(TableRevision.ObjectiveTypes, rev.ID).Rev = rev.Rev;
        }

        public async Task Upload(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
            {
                remote = mapper.Map<ObjectiveTypeDto>(local);
                await disk.Push(remote, action.ID.ToString());
            }

            action.IsComplete = true;
        }

        public void CheckDBRevision(RevisionCollection local)
        {
            var allId = context.ObjectiveTypes.Select(x => x.ID).ToList();

            var revCollect = local.GetRevisions(TableRevision.ObjectiveTypes);
            foreach (var id in allId)
            {
                if (!revCollect.Any(x => x.ID == id))
                {
                    revCollect.Add(new Revision(id));
                }
            }
        }

        private async Task GetLocal(int id)
        {
            if (local?.ID != id)
                local = await context.ObjectiveTypes.FindAsync(id);
        }

        private async Task GetRemote(int id)
        {
            if (remote?.ID != (ID<ObjectiveTypeDto>)id)
                remote = await disk.Pull<ObjectiveTypeDto>(id.ToString());
        }
    }
}
