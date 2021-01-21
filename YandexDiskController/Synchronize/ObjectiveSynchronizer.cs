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
        private DiskManager disk;
        private ProjectDto project;
        private List<ObjectiveDto> objectives;
        private ObjectiveDto remoteObj;
        private ObjectiveDto localObj;

        public ObjectiveSynchronizer(DiskManager yandex, ProjectDto localProject)
        {
            this.disk = yandex;
            this.project = localProject;
        }


        public string NameElement { get; set; }

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
            var projectRev = revisions.GetProject(idProj);

            var objectiveRev = projectRev.FindObjetive(rev.ID);
            objectiveRev.Rev = rev.Rev;
        }

        public async Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
        {
            if (localRev == null) localRev = new Revision(remoteRev.ID);
            if (remoteRev == null) remoteRev = new Revision(localRev.ID);

            Find(localRev.ID);
            NameElement = $"proj({project.ID}) obj({localObj.ID})";
            if (localRev.IsDelete || remoteRev.IsDelete) return SyncAction.Delete;

            await Download(localRev.ID);
            if (remoteObj == null) remoteRev.Rev = 0;
            if (localObj == null) localRev.Rev = 0;

            if (localRev < remoteRev) return SyncAction.Download;
            if (localRev > remoteRev) return SyncAction.Upload;
            return SyncAction.None;
        }

        public async Task<List<ISynchronizer>> GetSubSynchronizesAsync(int idObj)
        {
            List<ISynchronizer> subSynchronizes = new List<ISynchronizer>();

            await Download(idObj);
            Find(idObj);
            if (remoteObj == null) remoteObj = new ObjectiveDto() { ID = new ID<ObjectiveDto>(idObj) };
            if (localObj == null) localObj = new ObjectiveDto() { ID = new ID<ObjectiveDto>(idObj) };

            if ((localObj.Items != null || remoteObj.Items != null)
                && (localObj.Items.Count() > 0 || remoteObj.Items.Count() > 0))
                subSynchronizes.Add(new ItemsSynchronizer(disk, project, remoteObj, localObj));

            return subSynchronizes;
        }

        public Task LoadCollection()
        {
            objectives = ObjectModel.GetObjectives(project);
            return Task.CompletedTask;
        }

        public Task SaveCollectionAsync()
        {
            ObjectModel.SaveObjectives(project, objectives);
            // ObjectModel.SaveObjectives(project, objectives);
            return Task.CompletedTask;
        }

        public async Task DownloadRemote(int id)
        {
            await Download(id);
            var index = objectives.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                objectives.Add(remoteObj);
            else
                objectives[index] = remoteObj;
        }

        public async Task UploadLocal(int id)
        {
            Find(id);
            await disk.UploadObjectiveAsync(project, localObj);
        }

        public Task DeleteLocal(int id)
        {
            var id1 = (ID<ObjectiveDto>)id;
            objectives.RemoveAll(x => x.ID == id1);
            return Task.CompletedTask;
        }

        public async Task DeleteRemote(int id)
        {
            var id1 = (ID<ObjectiveDto>)id;

            // TODO: Удалять items файлы? Сначало понять ссылаются ли другие item на него
            await disk.DeleteObjective(project, id1);
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
                remoteObj = await disk.GetObjectiveAsync(project, _id);
        }
    }
}
