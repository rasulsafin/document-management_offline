using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Factories
{
    public class ObjectiveSynchronizerFactory
    {
        private readonly ItemsSyncHelper itemsSyncHelper;
        private readonly FoldersSyncHelper foldersSyncHelper;
        private readonly ProjectsHelper projectsHelper;
        private readonly HubsHelper hubsHelper;
        private readonly IssuesService issuesService;
        private readonly ItemsService itemsService;

        public ObjectiveSynchronizerFactory(
            ItemsSyncHelper itemsSyncHelper,
            FoldersSyncHelper foldersSyncHelper,
            ProjectsHelper projectsHelper,
            HubsHelper hubsHelper,
            IssuesService issuesService,
            ItemsService itemsService)
        {
            this.itemsSyncHelper = itemsSyncHelper;
            this.foldersSyncHelper = foldersSyncHelper;
            this.projectsHelper = projectsHelper;
            this.hubsHelper = hubsHelper;
            this.issuesService = issuesService;
            this.itemsService = itemsService;
        }

        public Bim360ObjectivesSynchronizer Create(Bim360ConnectionContext context)
            => new Bim360ObjectivesSynchronizer(
                context,
                itemsSyncHelper,
                foldersSyncHelper,
                projectsHelper,
                hubsHelper,
                issuesService,
                itemsService);
    }
}
