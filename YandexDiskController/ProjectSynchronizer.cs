﻿using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    public class ProjectSynchronizer : ISynchronizer
    {
        private YandexDiskManager yandex;
        private List<ProjectDto> projects;
        private ProjectDto remoteProject;
        private ProjectDto localProject;

        public ProjectSynchronizer(YandexDiskManager yandex)
        {
            this.yandex = yandex;
        }
        public List<Revision> GetRevision(Revisions revisions)
        {
            if (revisions.Projects == null) 
                revisions.Projects = new List<ProjectRevision>();
            return revisions.Projects.Select(x => (Revision)x).ToList();
        }
        public List<ISynchronizer> GetSubSynchronizes(int idProject)
        {
            List<ISynchronizer> subSynchronizes = new List<ISynchronizer>();

            var _id = (ID<ProjectDto>)idProject;
            if (localProject.ID != _id)
                localProject = projects.Find(x => x.ID == _id);
            subSynchronizes.Add(new ItemsSynchronizer(yandex, localProject));
            subSynchronizes.Add(new ObjectiveSynchronizer(yandex, localProject));
            return subSynchronizes;
        }
        public void LoadLocalCollect()
        {
            projects = ObjectModel.GetProjects();
        }
        public void SaveLocalCollect()
        {
            ObjectModel.SaveProjects(projects);
        }

        public async Task<bool> RemoteExistAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            remoteProject = await yandex.GetProjectAsync(_id);
            return remoteProject == null;
        }
        public void DeleteLocal(int id)
        {
            var _id = (ID<ProjectDto>)id;
            projects.RemoveAll(x => x.ID == _id);
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            if (remoteProject.ID == _id)
                remoteProject = await yandex.GetProjectAsync(_id);
            var index = projects.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                projects.Add(remoteProject);
            else
                projects[index] = remoteProject;
        }
        public bool LocalExist(int id)
        {
            var _id = (ID<ProjectDto>)id;
            localProject = projects.Find(x => x.ID == _id);
            return localProject != null;
        }

        public Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            throw new System.NotImplementedException();
        }
        public async Task UpdateRemoteAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            if (localProject.ID != _id)
                localProject = projects.Find(x => x.ID == _id);
            await yandex.UnloadProject(localProject);
        }
    }
}