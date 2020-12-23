using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace MRS.DocumentManagement.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemHelper itemHelper;
        
        public ProjectService(DMContext context, IMapper mapper, ItemHelper itemHelper)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
        }

        public async Task<ID<ProjectDto>> AddToUser(ID<UserDto> owner, string title)
        {
            var userID = (int)owner;
            var user = context.Users.Find(userID);
            if (user == null)
                return ID<ProjectDto>.InvalidID;
            var project = new Database.Models.Project { Title = title };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            project.Users = new List<Database.Models.UserProject>
            {
                new Database.Models.UserProject
                {
                    UserID = userID,
                    ProjectID = project.ID
                }
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
            if (project.Users == null)
                project.Users = new List<Database.Models.UserProject>();
            foreach (var user in users)
            {
                if (!project.Users.Any(x => x.ProjectID == project.ID && x.UserID == (int)user))
                {
                    project.Users.Add(new Database.Models.UserProject
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
            return mapper.Map<ProjectDto>(dbProject);
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjects()
        {
            var dbProjects = await context.Projects.ToListAsync();
            return dbProjects.Select(x => mapper.Map<ProjectDto>(x)).ToList();
        }

        public async Task<IEnumerable<ProjectDto>> GetUserProjects(ID<UserDto> userID)
        {
            var iuserID = (int)userID;
            var dbProjects = await context.Users
                .Where(x => x.ID == iuserID)
                .SelectMany(x => x.Projects)
                .Select(x => x.Project)
                .ToListAsync();

            var userProjects = dbProjects.Select(x => mapper.Map<ProjectDto>(x)).ToList();
            return userProjects;
        }

        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
        {
            var usersDb = await context.UserProjects
                .Where(x => x.ProjectID == (int)projectID)
                .Select(x => x.User)
                .ToListAsync();
            return usersDb.Select(x => mapper.Map<UserDto>(x)).ToList();
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
            var project = await context.Projects.Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == (int) projectID);
            if (project == null)
                return false;
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
            project.Title = projectData.Title;
            context.Projects.Update(project);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ID<ProjectDto>> Add(string title)
        {
            var project = new Database.Models.Project { Title = title };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            return (ID<ProjectDto>)project.ID;
        }

        private async Task LinkItem(ItemDto item, Database.Models.Project project)
        {
            var dbItem = await itemHelper.CheckItemToLink(context, item, project.GetType(), project.ID);
            if (dbItem == null)
                return;

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
