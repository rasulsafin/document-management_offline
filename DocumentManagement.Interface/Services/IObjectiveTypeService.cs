using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;

namespace DocumentManagement.Interface.Services
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
