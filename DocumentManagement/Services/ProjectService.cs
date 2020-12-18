﻿using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DMContext context;

        public ProjectService(DMContext context)
        {
            this.context = context;
        }

        public async Task<ID<ProjectDto>> AddToUser(ID<UserDto> owner, string title)
        {
            var userID = (int)owner;
            var user = context.Users.Find(userID);
            if (user == null)
                //throw new ArgumentException($"User with key {userID} not found");
                return ID<ProjectDto>.InvalidID;
            var project = new Database.Models.Project() { Title = title };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            project.Users = new List<Database.Models.UserProject>()
            {
                new Database.Models.UserProject(){ UserID = userID, ProjectID = project.ID }
            };
            context.Update(project);
            await context.SaveChangesAsync();
            return (ID<ProjectDto>)project.ID;
        }

        public async Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            var project = await context.Projects.Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);
            if (project == null)
                return false;
                //throw new ArgumentException($"Project with key {projectID} not found");
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
            return true;
        }

        public async Task<ProjectDto> Find(ID<ProjectDto> projectID)
        {
            var dbProject = await context.Projects.FindAsync((int)projectID);
            if (dbProject == null)
                return null;
            return new ProjectDto() { ID = projectID, Title = dbProject.Title };
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjects()
        {
            var dbProjects = await context.Projects.ToListAsync();
            return dbProjects.Select(x => new ProjectDto() 
            {
                ID = (ID<ProjectDto>)x.ID,
                Title = x.Title
            }).ToList();
        }

        public async Task<IEnumerable<ProjectDto>> GetUserProjects(ID<UserDto> userID)
        {
            var iuserID = (int)userID;
            var dbProjects = await context.Users
                .Where(x => x.ID == iuserID)
                .SelectMany(x => x.Projects)
                .Select(x => new { x.ProjectID, x.Project.Title })
                .ToListAsync();

            var userProjects = dbProjects.Select(x => new ProjectDto() 
            {
                ID = (ID<ProjectDto>)x.ProjectID,
                Title = x.Title
            }).ToList();
            return userProjects;
        }

        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
        {
            var usersDb = await context.UserProjects
                .Where(x => x.ProjectID == (int)projectID)
                .Select(x => x.User)
                .ToListAsync();
            return usersDb.Select(x => new UserDto((ID<UserDto>)x.ID, x.Login, x.Name)).ToList();
        }

        public async Task<bool> Remove(ID<ProjectDto> projectID)
        {
            var project = await context.Projects.FindAsync((int)projectID);
            if (project == null)
                return false;
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            var project = await context.Projects.FindAsync((int)projectID);
            if (project == null)
                return false;
                //throw new ArgumentException($"Project with key {projectID} not found");
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
            return true;
        }

        public async Task<bool> Update(ProjectDto projectData)
        {
            var projectID = projectData.ID;
            var project = await context.Projects.FindAsync((int)projectID);
            if (project == null)
                return false;
                //throw new ArgumentException($"Project with key {projectID} not found");
            project.Title = projectData.Title;
            context.Projects.Update(project);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ID<ProjectDto>> Add(string title)
        {
            var project = new Database.Models.Project() { Title = title };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            return (ID<ProjectDto>)project.ID;
        }

        private async Task LinkItem(ItemDto item, Database.Models.Project project)
        {
            var dbItem = await context.Items
                    .FirstOrDefaultAsync(i => i.ID == (int)item.ID);

            var alreadyLinked = await context.ProjectItems
                .AnyAsync(i => i.ItemID == (int)item.ID
                            && i.ProjectID == project.ID);

            if (alreadyLinked)
                return;

            if (dbItem == null)
            {
                dbItem = new Database.Models.Item
                {
                    ID = (int)item.ID,
                    ItemType = (int)item.ItemType,
                    ExternalItemId = item.ExternalItemId
                };
                context.Items.Add(dbItem);
            }
            project.Items.Add(new Database.Models.ProjectItem
            {
                ProjectID = project.ID,
                ItemID = dbItem.ID
            });
        }

        private async Task<bool> UnlinkItem(ID<ItemDto> itemID, ID<ProjectDto> projectID)
        {
            var link = await context.ProjectItems
                .Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ProjectID == (int)projectID)
                .FirstOrDefaultAsync();
            if (link == null)
                return false;
            context.ProjectItems.Remove(link);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
