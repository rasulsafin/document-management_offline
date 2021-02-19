using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectToListDto>> GetAllProjects();
        Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID);

        Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate);
        Task<bool> Remove(ID<ProjectDto> projectID);
        Task<bool> Update(ProjectDto project);
        Task<ProjectDto> Find(ID<ProjectDto> projectID);

        Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID);
        Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);
        Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);
    }
}
