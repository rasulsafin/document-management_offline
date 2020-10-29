using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public class TaskTypeRepository : ITaskTypeRepository
    {
        private readonly DocumentManagementContext context;
        public TaskTypeRepository(DocumentManagementContext context)
            => this.context = context;

        public async Task<bool> Add(TaskType type)
        {
            await context.Types.AddAsync(type);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<TaskType>> GetList() => await context.Types.ToListAsync();

        public async Task<bool> IsExists(string name)
        {
            if (await context.Types.AnyAsync(t => t.Name == name))
                return true;

            return false;
        }

        public async Task<bool> Delete(string typeName)
        {
            var taskType = await context.Types.FirstOrDefaultAsync(t => t.Name == typeName);

            context.Types.Remove(taskType);
            await context.SaveChangesAsync();

            return !(await IsExists(taskType.Name));
        }
    }
}
