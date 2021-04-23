using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DocumentManagement.General.Utils.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProjectService> logger;

        public ProjectService(DMContext context,
            IMapper mapper,
            ItemHelper itemHelper,
            ILogger<ProjectService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.itemHelper = itemHelper;
            this.logger = logger;
            logger.LogTrace("ProjectService created");
        }

        public async Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Add started with projectToCreate: {@ProjectToCreate}", projectToCreate);

            try
            {
                var projectToDb = mapper.Map<Project>(projectToCreate);
                logger.LogDebug("Mapped project: {@ProjectToDb}", projectToDb);
                await context.Projects.AddAsync(projectToDb);
                await context.SaveChangesAsync();

                projectToDb.Items = new List<Item>();
                foreach (var item in projectToCreate.Items)
                {
                    await LinkItem(item, projectToDb);
                }

                var ownerID = (int)projectToCreate.AuthorID;
                var user = await context.Users.FindAsync(ownerID);
                logger.LogDebug("Found user: {@User}", user);

                if (user != null)
                {
                    // TODO: Link project to user only with the unique name + externalId;
                    projectToDb.Users = new List<UserProject>
                    {
                        new UserProject
                        {
                            UserID = ownerID,
                            ProjectID = projectToDb.ID,
                        },
                    };
                }

                context.Update(projectToDb);
                await context.SaveChangesAsync();
                var res = mapper.Map<ProjectToListDto>(projectToDb);
                logger.LogDebug("Adding result: {@Result}", res);

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't add project {@ProjectToCreate}", projectToCreate);
                throw;
            }
        }

        public async Task<ProjectDto> Find(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Find started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(p => p.ID == (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);
                if (dbProject == null)
                    throw new ArgumentNullException($"Project with key {projectID} was not found");
                return mapper.Map<ProjectDto>(dbProject);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get project {ProjectID}", projectID);
                throw;
            }
        }

        public async Task<IEnumerable<ProjectToListDto>> GetAllProjects()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetAllProjects started");
            try
            {
                var dbProjects = await context.Projects.Unsynchronized().ToListAsync();
                logger.LogDebug("Found projects: {@DBProjects}", dbProjects);
                return dbProjects.Select(x => mapper.Map<ProjectToListDto>(x)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of all projects");
                throw;
            }
        }

        public async Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetUserProjects started with userID: {@UserID}", userID);
            try
            {
                var userFromDb = await context.FindAsync<User>((int)userID);
                if (userFromDb == null)
                    throw new ArgumentNullException($"User with key {userID} was not found");

                var iUserID = userFromDb.ID;
                var dbProjects = await context.Users
                   .Where(x => x.ID == iUserID)
                   .SelectMany(x => x.Projects)
                   .Select(x => x.Project)
                   .Unsynchronized()
                   .ToListAsync();

            logger.LogDebug("Found projects: {@DBProjects}", dbProjects);
                var userProjects = dbProjects.Select(x => mapper.Map<ProjectToListDto>(x)).ToList();
            logger.LogDebug("Mapped projects: {@UserProjects}", userProjects);
                return userProjects;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of projects");
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetUsers started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.FindAsync<Project>((int)projectID);
                if (dbProject == null)
                    throw new ArgumentNullException($"Project with key {projectID} was not found");

                var usersDb = await context.UserProjects
                    .Where(x => x.ProjectID == dbProject.ID)
                    .Select(x => x.User)
                    .ToListAsync();
                logger.LogDebug("Found users: {@UsersDb}", usersDb);
                return usersDb.Select(x => mapper.Map<UserDto>(x)).ToList();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't get list of users from project {ProjectID}", projectID);
                throw;
            }
        }

        public async Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("LinkToUsers started for project {@ProjectID} with users: {@Users}", projectID, users);
            try
            {
                var project = await context.Projects.Include(x => x.Users)
                    .FirstOrDefaultAsync(x => x.ID == (int)projectID);
                logger.LogDebug("Found project: {@Project}", project);
                if (project == null)
                    throw new ArgumentNullException($"Project with key {projectID} was not found");

                project.Users ??= new List<UserProject>();
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
            catch (Exception e)
            {
                logger.LogError(e, "Can't link project {ProjectID} to list of users {@Users}", projectID, users);
                throw;
            }
        }

        public async Task<bool> Remove(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Remove started with projectID: {@ProjectID}", projectID);
            try
            {
                var project = await context.Projects.FindAsync((int)projectID);
                logger.LogDebug("Found project: {@Project}", project);
                if (project == null)
                    throw new ArgumentNullException($"Project with key {projectID} was not found");

                context.Projects.Remove(project);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't delete project {ProjectID}", projectID);
                throw;
            }
        }

        public async Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("UnlinkFromUsers started for project {@ProjectID} with users: {@Users}", projectID, users);
            try
            {
                var project = await context.Projects.Include(x => x.Users)
                    .FirstOrDefaultAsync(x => x.ID == (int)projectID);
                logger.LogDebug("Found project: {@Project}", project);
                if (project == null)
                    throw new ArgumentNullException($"Project with key {projectID} was not found");

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
            catch (Exception e)
            {
                logger.LogError(e, "Can't unlink project {ProjectID} from list of users {@Users}", projectID, users);
                throw;
            }
        }

        public async Task<bool> Update(ProjectDto project)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Update started with project: {@Project}", project);
            try
            {
                var projectID = project.ID;
                var projectFromDb = await context.Projects
                   .Include(x => x.Items)
                   .FirstOrDefaultAsync(x => x.ID == (int)projectID);
                logger.LogDebug("Found project: {@ProjectFromDB}", projectFromDb);
                if (projectFromDb == null)
                    throw new ArgumentNullException($"Project with key {projectID} was not found");

                projectFromDb = mapper.Map(project, projectFromDb);
                logger.LogDebug("Mapped project: {@ProjectFromDB}", projectFromDb);

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
            catch (Exception e)
            {
                logger.LogError(e, "Can't update project {@Project}", project);
                throw;
            }
        }

        private async Task LinkItem(ItemDto item, Project project)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("LinkItem started with project: {@Project} and item: {@Item}", project, item);
            try
            {
                var dbItem = await itemHelper.CheckItemToLink(context, mapper, item, project.GetType(), project.ID);
                logger.LogDebug("Found item: {@DBItem}", dbItem);
                if (dbItem == null)
                    return;

                dbItem.Project = project;
                context.Items.Update(dbItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't link item {@Item} to project {@Project}", item, project);
                throw;
            }
        }

        private async Task<bool> UnlinkItem(int itemID, int projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("UnlinkItem started for project {@ProjectID} with itemID: {@ItemID}", projectID, itemID);

            try
            {
                var item = await context.Items
                   .Include(x => x.Objectives)
                   .Where(x => x.ID == itemID)
                   .Where(x => x.ProjectID == projectID)
                   .FirstOrDefaultAsync();
                logger.LogDebug("Found item: {@Item}", item);
                if (item == null)
                    return false;

                if (item.Objectives.Count == 0)
                {
                    context.Items.Remove(item);
                    logger.LogDebug("{@ItemID} removed", itemID);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't link item {@ItemID} to project {@ProjectID}", itemID, projectID);
                throw;
            }
        }
    }
}
