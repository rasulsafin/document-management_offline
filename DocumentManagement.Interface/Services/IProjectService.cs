using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjects();
        Task<IEnumerable<Project>> GetUserProjects(ID<User> userID);
        Task<ID<Project>> Add(ID<User> owner, string title);
        Task<bool> Remove(ID<Project> projectID);
        Task Update(Project projectData);
        Task<Project> Find(ID<Project> projectID);

        Task<IEnumerable<User>> GetUsers(ID<Project> projectID);
        Task AddUsers(ID<Project> projectID, IEnumerable<ID<User>> users);
        Task RemoveUsers(ID<Project> projectID, IEnumerable<ID<User>> users);
    }
}
