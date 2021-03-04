using System;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
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
            if (topFolder == default)
                return default;

            var folder = (await foldersService.GetFoldersAsync(projectId, topFolder.ID)).FirstOrDefault();

            return folder;
        }
    }
}
