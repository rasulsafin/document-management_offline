using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Factories
{
    public class ProjectSynchronizerFactory
    {
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly FoldersService foldersService;

        public ProjectSynchronizerFactory(ItemsSyncHelper itemsSyncHelper, FoldersService foldersService)
        {
            this.itemsSyncHelper = itemsSyncHelper;
            this.foldersService = foldersService;
        }

        public Bim360ProjectsSynchronizer Create(Bim360ConnectionContext context)
            => new Bim360ProjectsSynchronizer(context, itemsSyncHelper, foldersService);
    }
}
