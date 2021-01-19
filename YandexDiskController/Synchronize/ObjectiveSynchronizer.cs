using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement
{
    internal class ObjectiveSynchronizer : ISynchronizer
    {
        private DiskManager yandex;
        private ProjectDto project;
        private List<ObjectiveDto> objectives;
        private ObjectiveDto remoteObj;
        private ObjectiveDto localObj;

        public ObjectiveSynchronizer(DiskManager yandex, ProjectDto localProject)
        {
            this.yandex = yandex;
            this.project = localProject;
        }

        public List<Revision> GetRevisions(RevisionCollection revisions)
        {
            int idProj = (int)project.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);
            if (projectRev == null)
                return new List<Revision>();

            if (projectRev.Objectives == null)
                projectRev.Objectives = new List<ObjectiveRevision>();
            return projectRev.Objectives.Select(x => (Revision)x).ToList();
        }

        public void SetRevision(RevisionCollection revisions, Revision rev)
        {
            int idProj = (int)project.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);
            if (projectRev.Objectives == null)
                projectRev.Objectives = new List<ObjectiveRevision>();

            var index = projectRev.Objectives.FindIndex(x => x.ID == rev.ID);
            if (index < 0)
                projectRev.Objectives.Add(new ObjectiveRevision(rev.ID, rev.Rev));
            else
                projectRev.Objectives[index].Rev = rev.Rev;
        }

        public async Task<List<ISynchronizer>> GetSubSynchronizesAsync(int idObj)
        {
            List<ISynchronizer> subSynchronizes = new List<ISynchronizer>();

            await Download(idObj);
            Find(idObj);
            subSynchronizes.Add(new ItemsSynchronizer(yandex, project, remoteObj, localObj));

            return subSynchronizes;
        }

        public void LoadLocalCollect()
        {
            objectives = ObjectModel.GetObjectives(project);
        }

        public Task SaveLocalCollectAsync()
        {
            ObjectModel.SaveObjectives(project, objectives);
            return Task.CompletedTask;
        }

        public async Task<bool> RemoteExist(int id)
        {
            await Download(id);
            return remoteObj != null;
        }

        public async Task DownloadAndUpdateAsync(int id)
        {
            await Download(id);
            var index = objectives.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                objectives.Add(remoteObj);
            else
                objectives[index] = remoteObj;
        }

        public Task DeleteLocalAsync(int id)
        {
            var id1 = (ID<ObjectiveDto>)id;
            objectives.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        public Task<bool> LocalExist(int id)
        {
            Find(id);
            return Task.FromResult(localObj != null);
        }

        public async Task UpdateRemoteAsync(int id)
        {
            Find(id);
            await yandex.UploadObjectiveAsync(project, localObj);
        }

        public async Task DeleteRemoteAsync(int id)
        {
            var id1 = (ID<ObjectiveDto>)id;

            // TODO: Удалять items файлы? Сначало понять ссылаются ли другие item на него
            await yandex.DeleteObjective(project, id1);
        }

        private void Find(int id)
        {
            var id1 = (ID<ObjectiveDto>)id;
            if (localObj?.ID != id1)
                localObj = objectives.Find(x => x.ID == id1);
        }

        private async Task Download(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            if (remoteObj?.ID != _id)
                remoteObj = await yandex.GetObjectiveAsync(project, _id);
        }
    }
}
