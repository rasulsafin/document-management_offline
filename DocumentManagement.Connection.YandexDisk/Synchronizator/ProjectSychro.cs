﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    public class ProjectSychro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private Project local;
        private ProjectDto remote;

        public ProjectSychro(IDiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public async Task DeleteLocal(SyncAction action)
        {
            await GetLocal(action.ID);
            if (local != null)
                context.Projects.Remove(local);
        }


        public async Task DeleteRemote(SyncAction action)
        {
            await disk.Delete<ProjectDto>(action.ID.ToString());
        }

        public async Task Download(SyncAction action)
        {
            await GetRemote(action.ID);
            await GetLocal(action.ID);
            if (remote != null)
            {
                if (local != null)
                {
                    local.Title = remote.Title;
                }
                else
                {
                    local = new Project()
                    {
                        ID = (int)remote.ID,
                        Title = remote.Title,
                    };

                    // TODO: Надо както сделать получше
                    // Эта коллекция заполнится при синхронизации ItemDto
                    local.Items = new List<ProjectItem>();
                    context.Projects.Add(local);
                }
            }
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            return revisions.Projects.Select(pr => (Revision)pr).ToList();
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            List<ISynchroTable> synchros = new List<ISynchroTable>();
            synchros.Add(new ItemSynchro(disk, context, new ID<ProjectDto>(action.ID)));
            return Task.FromResult(synchros);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetProject(rev.ID).Rev = rev.Rev;
        }

        public Task Special(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public Task Upload(SyncAction action)
        {
            throw new System.NotImplementedException();
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
    }
}