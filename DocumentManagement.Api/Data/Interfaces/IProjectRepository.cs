using DocumentManagement.Models.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public interface IProjectRepository
    {
        Task<ProjectDb> Get(int id);
        Task<ProjectDb> Add(ProjectDb project, UserDb user);
        Task<ProjectDb> Update(ProjectDb project);
        Task<bool> Delete(int projectId);
        Task<bool> IsExists(int id);
        Task<IEnumerable<ProjectDb>> GetList(string login);
    }
}