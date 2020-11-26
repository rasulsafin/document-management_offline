using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;

namespace MRS.DocumentManagement.Interface.Services
{
    public interface IObjectiveTypeService
    {
        Task<IEnumerable<ObjectiveType>> GetAllObjectiveTypes();
        Task<ID<ObjectiveType>> Add(string typeName);
        Task<bool> Remove(ID<ObjectiveType> id);
        Task<ObjectiveType> Find(ID<ObjectiveType> id);
        Task<ObjectiveType> Find(string typename);
    }
}
