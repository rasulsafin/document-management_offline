using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DocumentManagementContext context;
        public TaskRepository(DocumentManagementContext context)
            => this.context = context;

        public async Task<TaskDmDb> Get(int taskId)
        {
            return await context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.Tasks)
                .Include(t => t.Author)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(p => p.Id == taskId);
        }

        public async Task<TaskDmDb> Add(TaskDmDb task, ProjectDb project, string login)
        {
            var user = await context.Users
                .Include(u => u.Projects)
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Login == login);

            task.ProjectId = project.Id;
            task.Project = project;

            task.Author = user;
            task.UserId = user.Id;

            if (project.Tasks == null)
                project.Tasks = new List<TaskDmDb>() { task };
            else
                project.Tasks.Add(task);

            if (user.Tasks == null)
                user.Tasks = new List<TaskDmDb>() { task };
            else
                user.Tasks.Add(task);

            await context.SaveChangesAsync();

            return task;
        }

        public async Task<TaskDmDb> AddSubtask(TaskDmDb task, TaskDmDb subtask, string login)
        {
            subtask.ParentTask = task;
            subtask.ParentTaskId = task.Id;

            if (task.Tasks == null)
                task.Tasks = new List<TaskDmDb>() { subtask };
            else
                task.Tasks.Add(subtask);

            return await Add(subtask, task.Project, login);
        }

        public async Task<TaskDmDb> Update(TaskDmDb task)
        {
            var taskToUpdate = await Get(task.Id);
            taskToUpdate.Status = task.Status;
            taskToUpdate.Title = task.Title;
            taskToUpdate.Descriptions = task.Descriptions;
            taskToUpdate.Date = task.Date;

            await context.SaveChangesAsync();

            return await Get(task.Id);
        }

        public async Task<bool> Delete(int taskId)
        {
            var task = await Get(taskId);

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            return !(await IsExists(taskId));
        }

        public async Task<bool> IsExists(int taskId)
        {
            if (await context.Tasks.AnyAsync(t => t.Id == taskId))
                return true;

            return false;
        }

        public async Task<List<TaskDmDb>> GetList(int projectId)
        {
            return await context.Tasks
                .Include(t => t.Author)
                .Include(t => t.Type)
                .Where(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<List<TaskDmDb>> GetList(int projectId, TaskType type)
        {
            var tasks = await GetList(projectId);
            return tasks.Where(t => t.Type == type).ToList();
        }
    }
}