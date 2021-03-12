using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.GoogleDrive.Synchronization
{
    public class GoogleDriveConnectionContext : AConnectionContext
    {
        private GoogleDriveConnectionContext()
        {
        }

        internal GoogleDriveManager GoogleManager { get; set; }

        public static GoogleDriveConnectionContext CreateContext(
            DateTime lastSynchronizationDate,
            GoogleDriveManager manager)
        {
            var context = new GoogleDriveConnectionContext { GoogleManager = manager };
            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            throw new NotImplementedException();
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            throw new NotImplementedException();
        }

        protected async override Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives()
        {
            throw new NotImplementedException();
        }

        protected async override Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects()
        {
            throw new NotImplementedException();
        }
    }
}
