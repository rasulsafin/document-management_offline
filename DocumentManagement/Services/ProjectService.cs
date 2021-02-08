using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Interface.SyncData;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ItemHelper itemHelper;
        private readonly ISyncService synchronizator;

        public ProjectService(DMContext context, IMapper mapper, ItemHelper itemHelper, ISyncService synchronizator)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
            this.synchronizator = synchronizator;
        }

        public async Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate)
        {
            var ownerID = (int)projectToCreate.AuthorID;

            var user = context.Users.Find(ownerID);
            if (user == null)
                return default;

            var projectToDb = mapper.Map<Project>(projectToCreate);
            projectToDb.Items = new List<ProjectItem>();
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
                            ProjectID = projectToDb.ID
                        }
                    };
            context.Update(projectToDb);
            await context.SaveChangesAsync();
            synchronizator.Update(NameTypeRevision.Projects, projectToDb.ID);
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
            var dbProjects = await context.Projects.ToListAsync();
            return dbProjects.Select(x => mapper.Map<ProjectToListDto>(x)).ToList();
        }

        public async Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID)
        {
            var iuserID = (int)userID;
            var dbProjects = await context.Users
                .Where(x => x.ID == iuserID)
                .SelectMany(x => x.Projects)
                .Select(x => x.Project)
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
                        UserID = (int)user
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
            synchronizator.Update(NameTypeRevision.Projects, (int)projectID, TypeChange.Delete);
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
            var projectFromDb = await context.Projects.FindAsync((int)projectID);
            if (projectFromDb == null)
                return false;

            projectFromDb = mapper.Map(project, projectFromDb);

            projectFromDb.Items = new List<ProjectItem>();
            var projectItems = context.ProjectItems.Where(i => i.ProjectID == projectFromDb.ID).ToList();
            var itemsToUnlink = projectItems.Where(o => project.Items.Any(i => (int)i.ID == o.ItemID));

            foreach (var item in project.Items)
            {
                await LinkItem(item, projectFromDb);
            }

            foreach (var item in itemsToUnlink)
            {
                await UnlinkItem(item.ItemID, projectFromDb.ID);
            }

            context.Projects.Update(projectFromDb);
            await context.SaveChangesAsync();
            synchronizator.Update(NameTypeRevision.Projects, projectFromDb.ID);
            return true;
        }

        private async Task LinkItem(ItemDto item, Project project)
        {
            var dbItem = await itemHelper.CheckItemToLink(context, mapper, item, project.GetType(), project.ID);
            if (dbItem == null)
                return;

            project.Items.Add(new ProjectItem
            {
                ProjectID = project.ID,
                ItemID = dbItem.ID
            });
            synchronizator.Update(NameTypeRevision.Projects, project.ID);
        }

        private async Task<bool> UnlinkItem(int itemID, int projectID)
        {
            var link = await context.ProjectItems
                .Where(x => x.ItemID == (int)itemID)
                .Where(x => x.ProjectID == (int)projectID)
                .FirstOrDefaultAsync();
            if (link == null)
                return false;
            context.ProjectItems.Remove(link);
            await context.SaveChangesAsync();
            synchronizator.Update(NameTypeRevision.Projects, projectID);
            return true;
        }
    }
}
