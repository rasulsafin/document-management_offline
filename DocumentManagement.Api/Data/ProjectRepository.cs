using DocumentManagement.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DocumentManagementContext context;
        public ProjectRepository(DocumentManagementContext context) 
            => this.context = context;

        public async Task<ProjectDb> Get(int id)
            => await context.Projects.Include(p => p.Tasks)
                                     .Include(p => p.Items)
                                     .Include(p => p.Users)
                                     .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<ProjectDb> Add(ProjectDb project, UserDb user)
        {
            ProjectUsers bridge = new ProjectUsers()
            {
                Project = project,
                User = user
            };

            project.Users = new List<ProjectUsers>() { bridge };

            var entityToReturn = await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            return entityToReturn.Entity;
        }

        public async Task<ProjectDb> Update(ProjectDb project)
        {
            var projectToUpdate = await Get(project.Id);
            projectToUpdate.Title = project.Title;
            await context.SaveChangesAsync();

            return await Get(project.Id);
        }

        public async Task<bool> Delete(int projectId)
        {
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            context.Projects.Remove(project);
            await context.SaveChangesAsync();

            return !(await IsExists(projectId));
        }

        public async Task<bool> IsExists(int id)
        {
            if (await context.Projects.AnyAsync(p => p.Id == id))
                return true;

            return false;
        }

        public async Task<IEnumerable<ProjectDb>> GetList(string login)
        {
            UserDb user = await context.Users.FirstOrDefaultAsync(u => u.Login == login);
            var projects = context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Items)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Item)
                .Include(p => p.Users)
                .Where(p => p.Users.Any(u => u.UserId == user.Id));

            return projects;
        }
    }
}
