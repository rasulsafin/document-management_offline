using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Utility;

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

        public async Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate)
        {
            var ownerID = (int)projectToCreate.AuthorID;

            var user = context.Users.Find(ownerID);
            if (user == null)
                return default;

            var projectToDb = mapper.Map<Project>(projectToCreate);
            projectToDb.Items = new List<Item>();
            foreach (var item in projectToCreate.Items)
            {
                await LinkItem(item, projectToDb);
            }

            await context.Projects.AddAsync(projectToDb);
            await context.SaveChangesAsync();

            projectToDb.Users = new List<UserProject>
                    {
                        new UserProject
                        {
                            UserID = ownerID,
                            ProjectID = projectToDb.ID,
                        },
                    };
            context.Update(projectToDb);
            await context.SaveChangesAsync();
            var res = mapper.Map<ProjectToListDto>(projectToDb);

            return res;
        }

        public async Task<ProjectDto> Find(ID<ProjectDto> projectID)
        {
            var dbProject = await context.Projects.FindAsync((int)projectID);
            if (dbProject == null)
                return null;
            return mapper.Map<ProjectDto>(dbProject);
        }

        public async Task<IEnumerable<ProjectToListDto>> GetAllProjects()
        {
            var dbProjects = await context.Projects.Unsynchronized().ToListAsync();
            return dbProjects.Select(x => mapper.Map<ProjectToListDto>(x)).ToList();
        }

        public async Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID)
        {
            var iUserID = (int)userID;
            var dbProjects = await context.Users
               .Where(x => x.ID == iUserID)
               .SelectMany(x => x.Projects)
               .Select(x => x.Project)
               .Unsynchronized()
               .ToListAsync();

            var userProjects = dbProjects.Select(x => mapper.Map<ProjectToListDto>(x)).ToList();
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

        public async Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            var project = await context.Projects.Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);
            if (project == null)
                return false;
            if (project.Users == null)
                project.Users = new List<UserProject>();
            foreach (var user in users)
            {
                if (!project.Users.Any(x => x.ProjectID == project.ID && x.UserID == (int)user))
                {
                    project.Users.Add(new UserProject
                    {
                        ProjectID = project.ID,
                        UserID = (int)user,
                    });
                }
            }

            context.Update(project);
            await context.SaveChangesAsync();
            return true;
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
                .FirstOrDefaultAsync(x => x.ID == (int)projectID);
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

        public async Task<bool> Update(ProjectDto project)
        {
            var projectID = project.ID;
            var projectFromDb = await context.Projects
               .Include(x => x.Items)
               .FirstOrDefaultAsync(x => x.ID == (int)projectID);
            if (projectFromDb == null)
                return false;

            projectFromDb = mapper.Map(project, projectFromDb);

            var projectItems = projectFromDb.Items;
            projectFromDb.Items = new List<Item>();
            var itemsToUnlink = projectItems.Where(o => project.Items.Any(i => (int)i.ID == o.ID));

            foreach (var item in project.Items)
            {
                await LinkItem(item, projectFromDb);
            }

            foreach (var item in itemsToUnlink)
            {
                await UnlinkItem(item.ID, projectFromDb.ID);
            }

            context.Projects.Update(projectFromDb);
            await context.SaveChangesAsync();
            return true;
        }

        private async Task LinkItem(ItemDto item, Project project)
        {
            var dbItem = await itemHelper.CheckItemToLink(context, mapper, item, project.GetType(), project.ID);
            if (dbItem == null)
                return;

            dbItem.Project = project;
            context.Items.Update(dbItem);
            await context.SaveChangesAsync();
        }

        private async Task<bool> UnlinkItem(int itemID, int projectID)
        {
            var item = await context.Items
               .Include(x => x.Objectives)
               .Where(x => x.ID == (int)itemID)
               .Where(x => x.ProjectID == (int)projectID)
               .FirstOrDefaultAsync();
            if (item == null)
                return false;

            if (item.Objectives.Count == 0)
                context.Items.Remove(item);

            await context.SaveChangesAsync();
            return true;
        }
    }
}
