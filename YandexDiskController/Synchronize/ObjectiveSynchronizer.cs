using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    internal class ObjectiveSynchronizer : ISynchronizer
    {
        private YandexDiskManager yandex;        
        private ProjectDto project;
        private List<ObjectiveDto> objectives;
        private ObjectiveDto remoteObj;
        private ObjectiveDto localObj;

        public ObjectiveSynchronizer(YandexDiskManager yandex, ProjectDto localProject)
        {
            this.yandex = yandex;
            this.project = localProject;
        }

        public List<Revision> GetRevision(Revisions revisions)
        {
            int idProj = (int)project.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);
            if (projectRev == null)
                return new List<Revision>();
            if (projectRev.Objectives == null)
                projectRev.Objectives = new List<ObjectiveRevision>();
            return projectRev.Objectives.Select(x => (Revision)x).ToList();
        }
        public void SetRevision(Revisions revisions, List<Revision> objectiveRevs)
        {
            int idProj = (int)project.ID;
            var projectRev = revisions.Projects.Find(x => x.ID == idProj);
            if (projectRev.Objectives == null)
                if (projectRev.Objectives == null)
                    projectRev.Objectives = new List<ObjectiveRevision>();
            foreach (var rev in objectiveRevs)
            {
                var index = projectRev.Objectives.FindIndex(x => x.ID == rev.ID);
                if (index < 0)
                    projectRev.Objectives.Add(new ObjectiveRevision(rev.ID, rev.Rev));
                else
                    projectRev.Objectives[index].Rev = rev.Rev;
            }
        }
        public List<ISynchronizer> GetSubSynchronizes(int idObj)
        {
            List<ISynchronizer> subSynchronizes = new List<ISynchronizer>();
            
            FindLocalObjective(idObj);
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
        public void SaveLocalCollect()
        {
            ObjectModel.SaveObjectives(project, objectives);
        }



        public async Task<bool> RemoteExistAsync(int id)
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
        public void DeleteLocal(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            objectives.RemoveAll(x => x.ID == _id);
        }


        public bool LocalExist(int id)
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