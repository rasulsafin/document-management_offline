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

        public async Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
        {
            if (localRev == null) localRev = new Revision(remoteRev.ID);
            if (remoteRev == null) remoteRev = new Revision(localRev.ID);
            if (localRev.IsDelete || remoteRev.IsDelete) return SyncAction.Delete;

            await Download(localRev.ID);
            FindLocal(localRev.ID);
            if (remoteProject == null) remoteRev.Rev = 0;
            if (localProject == null) localRev.Rev = 0;

            if (localRev < remoteRev) return SyncAction.Download;
            if (localRev > remoteRev) return SyncAction.Upload;
            return SyncAction.None;
        }

        public async Task<List<ISynchronizer>> GetSubSynchronizesAsync(int idProject)
        {
            List<ISynchronizer> subSynchronizes = new List<ISynchronizer>();
            await Download(idProject);
            FindLocal(idProject);

            subSynchronizes.Add(new ItemsSynchronizer(yandex, remoteProject, localProject));
            subSynchronizes.Add(new ObjectiveSynchronizer(yandex, localProject));
            return subSynchronizes;
        }

        public void LoadCollection()
        {
            projects = ObjectModel.GetProjects();
        }

        public Task SaveLocalCollectAsync()
        {
            ObjectModel.SaveProjects(projects);
            return Task.CompletedTask;
        }

        public Task DeleteLocal(int id)
        {
            var id1 = (ID<ProjectDto>)id;
            projects.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        public async Task DownloadRemote(int id)
        {
            await Download(id);
            var index = projects.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                projects.Add(remoteProject);
            else
                projects[index] = remoteProject;

            var dirName = PathManager.GetLocalProjectDir(remoteProject);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
        }

        public async Task DeleteRemote(int id)
        {
            var id1 = (ID<ProjectDto>)id;

            // TODO: Удалять файлы item objective?
            await yandex.DeleteProject(id1);
        }

        public async Task UploadLocal(int id)
        {
            FindLocal(id);
            await yandex.UnloadProject(localProject);
        }

        private void FindLocal(int id)
        {
            var id1 = (ID<ProjectDto>)id;
            if (localProject?.ID != id1)
                localProject = projects.Find(x => x.ID == id1);
        }

        private async Task Download(int id)
        {
            var id1 = (ID<ProjectDto>)id;
            if (remoteProject?.ID != id1)
                remoteProject = await yandex.GetProjectAsync(id1);
        }
    }
}
