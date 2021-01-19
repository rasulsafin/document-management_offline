using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    internal class ProjectSynchronizer : ISynchronizer
    {
        private DiskManager disk;
        private DMContext context;
        // private DbSet<Project> projects;
        private ProjectDto remoteProject;
        private Project localProject;

        public ProjectSynchronizer(DiskManager disk, DMContext context)
        {
            this.disk = disk;
            this.context = context;
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            if (revisions.Projects == null)
                revisions.Projects = new List<ProjectRevision>();
            return revisions.Projects.Select(x => (Revision)x).ToList();
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            var index = revisions.Projects.FindIndex(x => x.ID == rev.ID);
            if (index < 0)
                revisions.Projects.Add(new ProjectRevision(rev.ID, rev.Rev));
            else
                revisions.Projects[index].Rev = rev.Rev;
        }

        public async Task<List<ISynchronizer>> GetSubSynchronizesAsync(int idProject)
        {
            List<ISynchronizer> subSynchronizes = new List<ISynchronizer>();

            if (!await LocalExist(idProject))
            {// проект не удален!!!
                ProjectDto project = Convert(localProject);
                subSynchronizes.Add(new ItemsSynchronizer(disk, context, project));
                subSynchronizes.Add(new ObjectiveSynchronizer(disk, context, project));
            }

            return subSynchronizes;
        }

        public void LoadLocalCollect()
        {
            // projects = context.Projects;
        }

        public async Task SaveLocalCollectAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task<bool> RemoteExist(int id)
        {
            await Download(id);
            return remoteProject != null;
        }

        private async Task Download(int id)
        {
            var _id = (ID<ProjectDto>)id;
            if (remoteProject?.ID != _id)
                remoteProject = await disk.GetProjectAsync(_id);
        }

        public async Task DeleteLocalAsync(int id)
        {
            if (await LocalExist(id))
                context.Projects.Remove(localProject);
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            await Download(id);
            if (await LocalExist(id))
            {
                localProject.Title = remoteProject.Title;
            }
            else
            {
                localProject = new Project
                {
                    ID = (int)remoteProject.ID,
                    Title = remoteProject.Title,
                };
            }
        }

        private async Task Find(int id)
        {
            if (localProject?.ID != id)
                localProject = await context.Projects.FindAsync(id);
        }

        public async Task<bool> LocalExist(int id)
        {
            await Find(id);
            return localProject != null;
        }

        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            // TODO: Удалять файлы item и objective?
            await disk.DeleteProject(_id);
        }

        public async Task UpdateRemoteAsync(int id)
        {
            if (await LocalExist(id))
            {

                await disk.UnloadProject(Convert(localProject));
            }
        }

        private ProjectDto Convert(Project project)
        {
            return new ProjectDto
            {
                ID = new ID<ProjectDto>(project.ID),
                Title = project.Title,
            };
        }
    }
}