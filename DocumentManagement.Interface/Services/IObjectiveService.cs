using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IObjectiveService
    {
        Task<IEnumerable<Objective>> GetAllObjectives();
        Task<ID<Objective>> Add(ObjectiveToCreate data);
        Task<bool> Remove(ID<Objective> objectiveID);
        Task Update(Objective projectData);
        Task<Objective> Find(ID<Objective> objectiveID);

        Task<IEnumerable<Objective>> GetObjectives(ID<Project> projectID);

        Task<IEnumerable<DynamicFieldInfo>> GetRequiredDynamicFields();
    }
}
