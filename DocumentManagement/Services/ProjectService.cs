using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using System.Linq;

namespace MRS.DocumentManagement.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DMContext context;

        public ProjectService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ID<Project>> Add(ID<User> owner, string title)
        {
            var userID = (int)owner;
            var user = context.Users.Find(userID);
            if (user == null)
                throw new ArgumentException($"User with key {userID} not found");

            var project = new Database.Models.Project() { Title = title };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            project.Users = new List<Database.Models.UserProject>()
            {
                new Database.Models.UserProject(){ UserID = userID, ProjectID = project.ID }
            };
            context.Update(project);
            await context.SaveChangesAsync();
            return (ID<Project>)project.ID;
        }

        public async Task AddUsers(ID<Project> projectID, IEnumerable<ID<User>> users)
        {
            var project = await context.Projects.Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);
            if(project == null)
                throw new ArgumentException($"Project with key {projectID} not found");
            if (project.Users == null)
                project.Users = new List<Database.Models.UserProject>();
            foreach (var user in users)
            {
                if (!project.Users.Any(x => x.ProjectID == project.ID && x.UserID == (int)user))
                {
                    project.Users.Add(new Database.Models.UserProject() 
                    {
                        ProjectID = project.ID,
                        UserID = (int)user
                    });
                }
            }
            context.Update(project);
            await context.SaveChangesAsync();
        }

        public async Task<Project> Find(ID<Project> projectID)
        {
            var dbProject = await context.Projects.FindAsync((int)projectID);
            if (dbProject == null)
                return null;
            return new Project() { ID = projectID, Title = dbProject.Title };
        }

        public async Task<IEnumerable<Project>> GetAllProjects()
        {
            var dbProjects = await context.Projects.ToListAsync();
            return dbProjects.Select(x => new Project() 
            {
                ID = (ID<Project>)x.ID,
                Title = x.Title
            }).ToList();
        }

        public async Task<IEnumerable<Project>> GetUserProjects(ID<User> userID)
        {
            var iuserID = (int)userID;
            var dbProjects = await context.Users
                .Where(x => x.ID == iuserID)
                .SelectMany(x => x.Projects)
                .Select(x => new { x.ProjectID, x.Project.Title })
                .ToListAsync();

            var userProjects = dbProjects.Select(x => new Project() 
            {
                ID = (ID<Project>)x.ProjectID,
                Title = x.Title
            }).ToList();
            return userProjects;
        }

        public async Task<IEnumerable<User>> GetUsers(ID<Project> projectID)
        {
            var usersDb = await context.UserProjects
                .Where(x => x.ProjectID == (int)projectID)
                .Select(x => x.User)
                .ToListAsync();
            return usersDb.Select(x => new User((ID<User>)x.ID, x.Login, x.Name)).ToList();
        }

        public async Task<bool> Remove(ID<Project> projectID)
        {
            var project = await context.Projects.FindAsync((int)projectID);
            if (project == null)
                return false;
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task RemoveUsers(ID<Project> projectID, IEnumerable<ID<User>> users)
        {
            var project = await context.Projects.FindAsync((int)projectID);
            if (project == null)
                throw new ArgumentException($"Project with key {projectID} not found");
            foreach (var user in users)
            {
                var link = project.Users.FirstOrDefault(x => x.UserID == (int)user);
                if (link != null)
                {
                    project.Users.Remove(link);
                }
            }
            context.Projects.Update(project);
            await context.SaveChangesAsync();
        }

        public async Task Update(Project projectData)
        {
            var projectID = projectData.ID;
            var project = await context.Projects.FindAsync((int)projectID);
            if (project == null)
                throw new ArgumentException($"Project with key {projectID} not found");
            project.Title = projectData.Title;
            context.Projects.Update(project);
            await context.SaveChangesAsync();
        }
    }
}
