using DocumentManagement.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public class ItemRepository : IItemRepository
    {
        private readonly DocumentManagementContext context;
        public ItemRepository(DocumentManagementContext context)
        {
            this.context = context;
        }

        public async Task<ItemDb> Get(string path)
            => await context.Items.Include(i => i.Projects)
                                  .FirstOrDefaultAsync(i => i.Path == path);

        public async Task<ItemDb> Add(ItemDb item)
        {
            var entityToReturn = await context.Items.AddAsync(item);
            await context.SaveChangesAsync();

            return entityToReturn.Entity;
        }

        public async Task<bool> Delete(string path)
        {
            var itemForDeletion = await Get(path);
            context.Items.Remove(itemForDeletion);
            await context.SaveChangesAsync();

            return !await IsExists(path);
        }

        public async Task<bool> IsLinked(ItemDb item)
        {
            if (await context.TaskItems.AnyAsync(i => i.Item == item)
                || await context.ProjectItems.AnyAsync(i => i.Item == item))
                return true;

            return false;
        }

        public async Task<bool> IsExists(string path)
        {
            if (await context.Items.AnyAsync(i => i.Path == path))
                return true;

            return false;
        }

        public async Task<bool> LinkItemToProject(ItemDb item, ProjectDb project)
        {
            ProjectItems bridge = new ProjectItems()
            {
                ProjectId = project.Id,
                Project = project,
                ItemId = item.Id,
                Item = item
            };

            item.Projects = new List<ProjectItems>() { bridge };
            await context.SaveChangesAsync();

            ///TODO: check if it works

            return true;
        }

        public async Task<bool> LinkItemToTask(ItemDb item, TaskDmDb task)
        {
            if (await context.TaskItems.AnyAsync(t => t.ItemId == item.Id && t.TaskId == task.Id))
                return true;

            TaskItems bridge = new TaskItems()
            {
                TaskId = task.Id,
                Task = task,
                ItemId = item.Id,
                Item = item
            };

            item.Tasks = new List<TaskItems>() { bridge };
            await context.SaveChangesAsync();

            ///TODO: check if it works

            return true;
        }

        public async Task<bool> BreakLinkWithProject(int itemId, int projectId)
        {
            var link = await context.ProjectItems
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.ProjectId == projectId);
            context.ProjectItems.Remove(link);
            await context.SaveChangesAsync();

            ///TODO: check if it works

            return true;
        }

        public async Task<bool> BreakLinkWithTask(int itemId, int taskId)
        {
            var link = await context.TaskItems
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.TaskId == taskId);
            context.TaskItems.Remove(link);
            await context.SaveChangesAsync();

            ///TODO: check if it works

            return true;
        }
    }
}
