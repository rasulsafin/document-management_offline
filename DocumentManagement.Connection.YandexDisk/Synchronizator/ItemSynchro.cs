using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    internal class ItemSynchro : ISynchroTable
    {
        private IDiskManager disk;
        private DMContext context;
        private ID<ProjectDto> iD;

        public ItemSynchro(IDiskManager disk, DMContext context, ID<ProjectDto> iD)
        {
            this.disk = disk;
            this.context = context;
            this.iD = iD;
        }

        public Task DeleteLocal(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteRemote(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public Task Download(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<ISynchroTable>> GetSubSynchroList(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            throw new System.NotImplementedException();
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
    }
}