﻿using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            var _id = (ID<ProjectDto>)idProject;
            if (localProject?.ID != _id)
                localProject = projects.Find(x => x.ID == _id);
            if (localProject != null) //throw new Exception($"Нету такого проекта! idProject={idProject}");
            {// проект не удален!!!
                subSynchronizes.Add(new ItemsSynchronizer(yandex, localProject));
                subSynchronizes.Add(new ObjectiveSynchronizer(yandex, localProject));
            }
            return subSynchronizes;
        }
        public void LoadLocalCollect()
        {
            projects = ObjectModel.GetProjects();
        }
        public async Task SaveLocalCollectAsync()
        {
            ObjectModel.SaveProjects(projects);
        }

        public async Task<bool> RemoteExist(int id)
        {
            var _id = (ID<ProjectDto>)id;
            remoteProject = await yandex.GetProjectAsync(_id);
            return remoteProject != null;
        }
        public async Task DeleteLocalAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            projects.RemoveAll(x => x.ID == _id);
        }
        public async Task DownloadAndUpdateAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            if (remoteProject.ID != _id)
                remoteProject = await yandex.GetProjectAsync(_id);
            var index = projects.FindIndex(x => (int)x.ID == id);
            if (index < 0)
                projects.Add(remoteProject);
            else
                projects[index] = remoteProject;

            var dirName = PathManager.GetProjectDir(remoteProject);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
        }
        public async Task<bool> LocalExist(int id)
        {
            var _id = (ID<ProjectDto>)id;
            localProject = projects.Find(x => x.ID == _id);
            return localProject != null;
        }

        public async Task DeleteRemoteAsync(int id)
        {
            var _id = (ID<ProjectDto>)id;
            // TODO: Удалять файлы item objective? 
            await yandex.DeleteProject( _id);
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