using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;

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
    }
}