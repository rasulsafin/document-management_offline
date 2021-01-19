using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement
{
    public class ProjectSynchronizer : ISynchronizer
    {
        private DiskManager yandex;
        private List<ProjectDto> projects;
        private ProjectDto remoteProject;
        private ProjectDto localProject;

        public ProjectSynchronizer(DiskManager yandex)
        {
            this.yandex = yandex;
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
            Find(idProject);
            await Download(idProject);

            // if (localProject != null) // throw new Exception($"Нету такого проекта! idProject={idProject}");
            // {// проект не удален!!!
            subSynchronizes.Add(new ItemsSynchronizer(yandex, remoteProject, localProject));
            subSynchronizes.Add(new ObjectiveSynchronizer(yandex, localProject));

            // }
            return subSynchronizes;
        }

        public void LoadLocalCollect()
        {
            projects = ObjectModel.GetProjects();
        }

        public Task SaveLocalCollectAsync()
        {
            ObjectModel.SaveProjects(projects);
            return Task.CompletedTask;
        }

        public async Task<bool> RemoteExist(int id)
        {
            await Download(id);
            return remoteProject != null;
        }

        public Task DeleteLocalAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            projects.RemoveAll(x => x.ID == _id);
            return Task.CompletedTask;
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            await Download(id);
            var index = projects.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                projects.Add(remoteProject);
            else
                projects[index] = remoteProject;

            var dirName = PathManager.GetProjectDir(remoteProject);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
        }

        public Task<bool> LocalExist(int id)
        {
            Find(id);
            return Task.FromResult(localProject != null);
        }

        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;

            // TODO: Удалять файлы item objective?
            await yandex.DeleteProject(_id);
        }

        public async Task UpdateRemoteAsync(int id)
        {
            Find(id);
            await yandex.UnloadProject(localProject);
        }

        private void Find(int id)
        {
            var _id = (ID<ProjectDto>)id;
            if (localProject?.ID != _id)
                localProject = projects.Find(x => x.ID == _id);
        }

        private async Task Download(int id)
        {
            var _id = (ID<ProjectDto>)id;
            if (remoteProject?.ID != _id)
                remoteProject = await yandex.GetProjectAsync(_id);
        }
    }
}
