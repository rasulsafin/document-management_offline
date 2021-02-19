using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IObjectiveService
    {
        Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data);
        Task<bool> Remove(ID<ObjectiveDto> objectiveID);
        Task<bool> Update(ObjectiveDto objectiveData);
        Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID);

        Task<IEnumerable<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID);

        Task<bool> GenerateReport(IEnumerable<ID<ObjectiveDto>> objectives, string path, int userID, string projectName);

        //Task<IEnumerable<DynamicFieldInfoDto>> GetRequiredDynamicFields(ID<ObjectiveDto> objectiveID);
    }
}
