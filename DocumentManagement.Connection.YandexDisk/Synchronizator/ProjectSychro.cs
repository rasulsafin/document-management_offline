using MRS.DocumentManagement.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    internal class ProjectSychro : ISynchroTable
    {
        private DiskManager disk;
        private DMContext context;

        public ProjectSychro(DiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        Task ISynchroTable.DeleteLocal(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        Task ISynchroTable.DeleteRemote(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        Task ISynchroTable.Download(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        List<Revision> ISynchroTable.GetRevisions(RevisionCollection revisions)
        {
            throw new System.NotImplementedException();
        }

        Task<List<ISynchroTable>> ISynchroTable.GetSubSynchroList(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        void ISynchroTable.SetRevision(RevisionCollection revisions, Revision rev)
        {
            throw new System.NotImplementedException();
        }

        Task ISynchroTable.Special(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        SyncAction ISynchroTable.SpecialSynchronization(SyncAction action)
        {
            throw new System.NotImplementedException();
        }

        Task ISynchroTable.Upload(SyncAction action)
        {
            throw new System.NotImplementedException();
        }
    }
}