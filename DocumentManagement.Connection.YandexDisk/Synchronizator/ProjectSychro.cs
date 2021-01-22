using MRS.DocumentManagement.Database;

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
    }
}