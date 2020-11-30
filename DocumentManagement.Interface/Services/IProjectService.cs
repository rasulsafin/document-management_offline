using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjects();
        Task<IEnumerable<ProjectDto>> GetUserProjects(ID<UserDto> userID);
        Task<ID<ProjectDto>> Add(ID<UserDto> owner, string title);
        Task<bool> Remove(ID<ProjectDto> projectID);
        Task Update(ProjectDto projectData);
        Task<ProjectDto> Find(ID<ProjectDto> projectID);

        Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID);
        Task AddUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);
        Task RemoveUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);
    }
}
