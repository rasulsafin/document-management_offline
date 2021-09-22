using Brio.Docs.Connections.Utils.CloudBase.Synchronizers;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;

namespace Brio.Docs.Connections.GoogleDrive.Synchronization
{
    public class GoogleDriveConnectionContext : AConnectionContext
    {
        private GoogleDriveManager manager;

        private GoogleDriveConnectionContext()
        {
        }

        public static GoogleDriveConnectionContext CreateContext(GoogleDriveManager manager)
        {
            var context = new GoogleDriveConnectionContext { manager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new StorageObjectiveSynchronizer(manager);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new StorageProjectSynchronizer(manager);
    }
}
