using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface.Services
{
    public interface IProjectService
    {
        IEnumerable<Project> GetUserProjects();
        Task<ID<Project>> Add(string title);
        Task Remove(ID<Project> projectID);
        Task Update(Project projectData);
        Task<Project> Find(ID<Project> projectID);

        Task<IEnumerable<User>> GetUsers(ID<Project> projectID);
        Task AddUsers(ID<Project> projectID, IEnumerable<ID<User>> users);
        Task RemoveUsers(ID<Project> projectID, IEnumerable<ID<User>> users);
    }
}
