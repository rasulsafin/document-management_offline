using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
    internal class ObjectiveSynchronizer : ISynchronizer
    {
        private DiskManager disk;
        private DMContext context;
        private ProjectDto project;
        private ObjectiveDto remoteObj;
        private Objective localObj;

        public ObjectiveSynchronizer(DiskManager disk, DMContext context, ProjectDto project)
        {
            this.disk = disk;
            this.context = context;
            this.project = project;
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
            if (await LocalExist(idObj))
                subSynchronizes.Add(new ItemsSynchronizer(disk, context, project, Convert(localObj)));
            return subSynchronizes;
        }

        public void LoadCollection()
        {
        }

        public async Task SaveLocalCollectAsync()
        {
            await context.SaveChangesAsync();
        }

        private async Task Download(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            if (remoteObj?.ID != _id)
                remoteObj = await disk.GetObjectiveAsync(project, _id);
        }

        private async Task Find(int id)
        {
            if (localObj?.ID != id)
            {
                int projectID = (int)project.ID;
                localObj = await context.Objectives.FirstAsync(o => o.ProjectID == projectID && o.ID == id);
            }
        }

        public async Task<bool> RemoteExist(int id)
        {
            await Download(id);
            return remoteObj != null;
        }

        public async Task DownloadRemote(int id)
        {
            await Download(id);
            if (await LocalExist(id))
            {
                localObj.ID = (int)remoteObj.ID;
                localObj.AuthorID = (int)remoteObj.AuthorID;
                localObj.ProjectID = (int)remoteObj.ProjectID;
                localObj.ObjectiveTypeID = (int)remoteObj.ObjectiveTypeID;
                localObj.ParentObjectiveID = (int)remoteObj.ParentObjectiveID;

                localObj.Status = (int)remoteObj.Status;

                localObj.Title = remoteObj.Title;
                localObj.Description = remoteObj.Description;

                localObj.CreationDate = remoteObj.CreationDate;
                localObj.DueDate = remoteObj.DueDate;

                // TODO : ???
                // localObj.Items = remoteObj.Items;
                // localObj.BimElements = remoteObj.BimElements;
                // localObj.DynamicFields = remoteObj.DynamicFields;
            }
            else
            {
                context.Objectives.Add(Convert(remoteObj));
            }
        }

        public async Task DeleteLocal(int id)
        {
            if (await LocalExist(id))
                context.Objectives.Remove(localObj);
        }

        public async Task<bool> LocalExist(int id)
        {
            await Find(id);
            return localObj != null;
        }

        public async Task UploadLocal(int id)
        {
            await Find(id);
            await disk.UploadObjectiveAsync(project, Convert(localObj));
        }

        public async Task DeleteRemote(int id)
        {
            var _id = (ID<ObjectiveDto>)id;
            // TODO: Удалять items файлы? Сначало понять ссылаются ли другие item на него
            await disk.DeleteObjective(project, _id);
        }

        private ObjectiveDto Convert(Objective objective)
        {
            return new ObjectiveDto()
            {
                ID = (ID<ObjectiveDto>)objective.ID,
                AuthorID = (ID<UserDto>)objective.AuthorID,
                ProjectID = (ID<ProjectDto>)objective.ProjectID,
                ObjectiveTypeID = (ID<ObjectiveTypeDto>)objective.ObjectiveTypeID,
                ParentObjectiveID = (ID<ObjectiveDto>)objective.ParentObjectiveID,

                Status = (ObjectiveStatus)objective.Status,

                Title = objective.Title,
                Description = objective.Description,

                CreationDate = objective.CreationDate,
                DueDate = objective.DueDate,
            };
        }

        private Objective Convert(ObjectiveDto objective)
        {
            return new Objective()
            {
                ID = (int)objective.ID,
                AuthorID = (int)objective.AuthorID,
                ProjectID = (int)objective.ProjectID,
                ObjectiveTypeID = (int)objective.ObjectiveTypeID,
                ParentObjectiveID = (int)objective.ParentObjectiveID,

                Status = (int)objective.Status,

                Title = objective.Title,
                Description = objective.Description,

                CreationDate = objective.CreationDate,
                DueDate = objective.DueDate,
            };
        }

        public Task<SyncAction> GetActoin(Revision localRev, Revision remoteRev)
        {
            throw new System.NotImplementedException();
        }
    }
}
