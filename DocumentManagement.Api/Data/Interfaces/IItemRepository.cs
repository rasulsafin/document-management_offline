using DocumentManagement.Models.Database;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public interface IItemRepository
    {
        Task<ItemDb> Add(ItemDb item);
        Task<bool> BreakLinkWithProject(int itemId, int projectId);
        Task<bool> BreakLinkWithTask(int itemId, int taskId);
        Task<bool> Delete(string path);
        Task<ItemDb> Get(string path);
        Task<bool> IsExists(string path);
        Task<bool> IsLinked(ItemDb item);
        Task<bool> LinkItemToProject(ItemDb item, ProjectDb project);
        Task<bool> LinkItemToTask(ItemDb item, TaskDmDb task);
    }
}