using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers
{
    public class FoldersSyncHelper
    {
        private readonly FoldersService foldersService;
        private readonly ProjectsService projectsService;

        public FoldersSyncHelper(FoldersService foldersService, ProjectsService projectsService)
        {
            this.foldersService = foldersService;
            this.projectsService = projectsService;
        }

        public async Task<Folder> GetDefaultFolderAsync(string hubId, string projectId, Func<Folder, bool> topFolderSelector = null)
        {
            var topFolders = await projectsService.GetTopFoldersAsync(hubId, projectId);
            var topFolder = topFolderSelector == null ? topFolders.LastOrDefault() : topFolders.LastOrDefault(topFolderSelector);
            return topFolder;
        }

        public async Task<IEnumerable<Item>> GetFolderItemsAsync(string projectId, string folderId)
        {
            var fileTuples = await foldersService.SearchAsync(projectId, folderId);
            return fileTuples.Select(t => t.item);
        }
    }
}
