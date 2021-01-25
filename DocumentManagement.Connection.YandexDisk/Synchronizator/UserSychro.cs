using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{


    public class UserSychro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;

        public UserSychro(IDiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public Task DeleteLocal(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRemote(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public Task Download(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public Task Special(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public Task Upload(SyncAction action)
        {
            throw new NotImplementedException();
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            if (revisions.Users == null)
                revisions.Users = new List<Revision>();
            return revisions.Users;
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            return Task.FromResult<List<ISynchroTable>>(null);
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            revisions.GetUser(rev.ID).Rev = rev.Rev;
        }

        public SyncAction SpecialSynchronization(SyncAction action)
        {
            return action;
        }
    }
}
