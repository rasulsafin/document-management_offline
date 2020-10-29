using DocumentManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public interface ITaskTypeRepository
    {
        Task<bool> Add(TaskType type);
        Task<bool> Delete(string typeName);
        Task<List<TaskType>> GetList();
        Task<bool> IsExists(string name);
    }
}