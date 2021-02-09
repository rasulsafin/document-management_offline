using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IObjectiveTypeService
    {
        Task<IEnumerable<ObjectiveTypeDto>> GetAllObjectiveTypes();
        Task<ID<ObjectiveTypeDto>> Add(string typeName);
        Task<bool> Remove(ID<ObjectiveTypeDto> id);
        Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id);
        Task<ObjectiveTypeDto> Find(string typename);
    }
}
