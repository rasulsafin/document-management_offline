using System;
using MRS.DocumentManagement.Connection.Utils.CloudBase.Synchronizers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.GoogleDrive.Synchronization
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
