using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            
            FindLocalObjective(idObj);
            if (localObj != null)
                subSynchronizes.Add(new ItemsSynchronizer(yandex, project, localObj));
            //subSynchronizes.Add(new ObjectiveSynchronizer(yandex, localProject));
            return subSynchronizes;
        }

        private void FindLocalObjective(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            if (localObj?.ID != _id)
                localObj = objectives.Find(x => x.ID == _id);
        }

        public void LoadLocalCollect()
        {
            objectives = ObjectModel.GetObjectives(project);
        }
        public async Task SaveLocalCollectAsync()
        {
            ObjectModel.SaveObjectives(project, objectives);
        }



        public async Task<bool> RemoteExist(int id)
        {
            await FindRemoteObjective(id);
            return remoteObj != null;
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            await FindRemoteObjective(id);
            var index = objectives.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                objectives.Add(remoteObj);
            else
                objectives[index] = remoteObj;


        }
        private async Task FindRemoteObjective(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            if (remoteObj?.ID != _id)
                remoteObj = await yandex.GetObjectiveAsync(project, _id);
        }
        public async Task DeleteLocalAsync(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            objectives.RemoveAll(x => x.ID == _id);
        }


        public async Task<bool> LocalExist(int id)
        {
            
            FindLocalObjective(id);
            return localObj != null;
        }
        public async Task UpdateRemoteAsync(int id)
        {
            FindLocalObjective(id);            
            await yandex.UploadObjectiveAsync(project, localObj);
        }
        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            // TODO: Удалять items файлы? Сначало понять ссылаются ли другие item на него 
            await yandex.DeleteObjective(project, _id);
        }

        
    }
}