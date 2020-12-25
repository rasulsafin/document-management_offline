using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class ProjectServise : IProjectService
    {
        YandexDiskManager yandex;

        public ProjectServise(YandexDiskManager yandex)
        {
            this.yandex = yandex;
        }

        public Task<ID<ProjectDto>> Add(string title)
        {
            throw new NotImplementedException();
        }

        public Task<ID<ProjectDto>> AddToUser(ID<UserDto> owner, string title)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectDto> Find(ID<ProjectDto> projectID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProjectDto>> GetAllProjects()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProjectDto>> GetUserProjects(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Remove(ID<ProjectDto> projectID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(ProjectDto projectData)
        {
            throw new NotImplementedException();
        }
    }
}
