using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class ObjectiveService : IObjectiveService
    {
        public Task<ID<ObjectiveDto>> Add(ObjectiveToCreateDto data)
        {
            throw new System.NotImplementedException();
        }

        public Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Remove(ID<ObjectiveDto> objectiveID)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(ObjectiveDto objectiveData)
        {
            throw new System.NotImplementedException();
        }
    }
}
