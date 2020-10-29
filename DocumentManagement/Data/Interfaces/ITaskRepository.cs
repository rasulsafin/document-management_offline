using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public interface ITaskRepository
    {
        Task<TaskDmDb> Add(TaskDmDb task, ProjectDb project, string login);
        Task<TaskDmDb> AddSubtask(TaskDmDb task, TaskDmDb subtask, string login);
        Task<bool> Delete(int taskId);
        Task<TaskDmDb> Get(int taskId);
        Task<List<TaskDmDb>> GetList(int projectId);
        Task<List<TaskDmDb>> GetList(int projectId, TaskType type);
        Task<bool> IsExists(int taskId);
        Task<TaskDmDb> Update(TaskDmDb task);
    }
}