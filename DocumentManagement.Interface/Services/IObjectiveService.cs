﻿using MRS.DocumentManagement.Interface.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IObjectiveService
    {
        Task<IEnumerable<ObjectiveToListDto>> GetAllObjectives();
        Task<ID<ObjectiveDto>> Add(ObjectiveToCreateDto data);
        Task<bool> Remove(ID<ObjectiveDto> objectiveID);
        Task<bool> Update(ObjectiveDto objectiveData);
        Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID);

        Task<IEnumerable<ObjectiveDto>> GetObjectives(ID<ProjectDto> projectID);

        //Task<IEnumerable<DynamicFieldInfoDto>> GetRequiredDynamicFields(ID<ObjectiveDto> objectiveID);
    }
}
