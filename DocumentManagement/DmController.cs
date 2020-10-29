using AutoMapper;
using DocumentManagement.Data;
using DocumentManagement.Helpers;
using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.Bim.DocumentManagement
{
    public class DmController
    {
        private readonly IAuthRepository authRepo;
        private readonly IProjectRepository projectRepo;
        private readonly ITaskRepository taskRepo;
        private readonly ITaskTypeRepository typeRepo;
        private readonly IItemRepository itemRepo;

        private readonly IMapper mapper;

        public DmController(IAuthRepository authRepo, 
            IProjectRepository projectRepo, 
            ITaskRepository taskRepo, 
            ITaskTypeRepository typeRepo,
            IItemRepository itemRepo)
        {
            this.authRepo = authRepo;
            this.projectRepo = projectRepo;
            this.taskRepo = taskRepo;
            this.typeRepo = typeRepo;
            this.itemRepo = itemRepo;

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfiles>());
            mapper = new Mapper(config);
        }

        public async Task<bool> Register(User userForRegister)
        {
            if (await authRepo.IsExists(userForRegister.Login))
                return false;

            var userToCreate = new UserDb
            {
                Login = userForRegister.Login
            };

            await authRepo.Register(userToCreate, userForRegister.Password);

            return true;
        }

        public async Task<bool> Login(User userForLoginDto)
        {
            var userFromRepo = await authRepo.Login(userForLoginDto.Login, userForLoginDto.Password);

            if (userFromRepo == null)
                return false;

            return true;
        }

        public async Task<bool> DeleteUser(User userForDeletion) 
            => await authRepo.Delete(userForDeletion.Login);

        public async Task<User> GetUser(string login)
        {
            var user = await authRepo.Get(login);
            var userForReturn = mapper.Map<UserDb, User>(user);

            return userForReturn;
        }


        public async Task<bool> AddProject(Project projectForAddition, string login)
        {
            if (!await authRepo.IsExists(login))
                return false;

            if (await projectRepo.IsExists(projectForAddition.Id))
                return false;

            var userToLink = await authRepo.Get(login);
            var projectToCreate = mapper.Map<Project, ProjectDb>(projectForAddition);

            await projectRepo.Add(projectToCreate, userToLink);

            return true;
        }

        public async Task<List<Project>> GetProjects(string login)
        {
            var projects = await projectRepo.GetList(login);
            var projectsToReturn = mapper.Map<List<Project>>(projects);

            return projectsToReturn;
        }

        public async Task<bool> DeleteProject(int projectId) => await projectRepo.Delete(projectId);

        public async Task<bool> DeleteTask(int id)
        {
            if (!(await taskRepo.IsExists(id)))
                return false;

            return await taskRepo.Delete(id);
        }

        public async Task<bool> AddType(TaskType type)
        {
            type.Name = type.Name.ToLower();
            if (await typeRepo.IsExists(type.Name))
                return false;

            await typeRepo.Add(type);

            return true;
        }
        public async Task<bool> DeleteTypeById(TaskType type)
        {
            if (!(await typeRepo.IsExists(type.Name)))
                return false;

            return await typeRepo.Delete(type.Name);
        }

        public async Task<List<TaskDm>> GetTasks(int projectId)
        {
            var tasks = await taskRepo.GetList(projectId);
            var tasksToReturn = mapper.Map<List<TaskDm>>(tasks);

            return tasksToReturn;
        }
        public async Task<List<TaskDm>> GetTasks(int projectId, TaskType type)
        {
            var tasks = await taskRepo.GetList(projectId, type);
            var tasksToReturn = mapper.Map<List<TaskDm>>(tasks);

            return tasksToReturn;
        }

        public async Task<List<TaskType>> GetTypes() => await typeRepo.GetList();

        public async Task<bool> UpdateProject(Project project)
        {
            if (!(await projectRepo.IsExists(project.Id)))
                return false;

            var projectToUpdate = mapper.Map<ProjectDb>(project);
            var updatedProject = await projectRepo.Update(projectToUpdate);

            foreach (var item in project.Items)
            {
                ItemDb itemToLink = await GetItemToLink(item);
                await itemRepo.LinkItemToProject(itemToLink, updatedProject);
            }

            return true;
        }
        public async Task<TaskDm> GetTask(int tadkId) 
        { 
            var task = await taskRepo.Get(tadkId);
            var taskForReturn = mapper.Map<TaskDm>(task);

            return taskForReturn;
        }

        public async Task<bool> AddTask(TaskDm taskForAddition, int projectId)
        {
            string login = taskForAddition.Author.Login;

            if (await taskRepo.IsExists(taskForAddition.Id))
                return false;

            if (!await authRepo.IsExists(login))
                return false;

            if (!(await projectRepo.IsExists(projectId)))
                return false;

            if (!(await typeRepo.IsExists(taskForAddition.Type.Name)))
                return false;

            var projectToLink = await projectRepo.Get(projectId);
            var taskToCreate = mapper.Map<TaskDm, TaskDmDb>(taskForAddition);
            var createdTask = await taskRepo.Add(taskToCreate, projectToLink, login);

            foreach (var item in taskForAddition.Items)
            {
                ItemDb itemToLink = await GetItemToLink(item);
                await itemRepo.LinkItemToTask(itemToLink, createdTask);
            }

            return true;
        }

        public async Task<bool> UpdateTask(TaskDm task)
        {
            if (!(await taskRepo.IsExists(task.Id)))
                return false;

            var taskToUpdate = mapper.Map<TaskDmDb>(task);
            var updatedTask = await taskRepo.Update(taskToUpdate);

            foreach (var item in task?.Items)
            {
                if (item != null)
                {
                    ItemDb itemToLink = await GetItemToLink(item);
                    await itemRepo.LinkItemToTask(itemToLink, updatedTask);
                }
            }

            foreach (var subtask in task?.Tasks)
            {
                if (subtask != null)
                {
                    if (!(await taskRepo.IsExists(subtask.Id)))
                    {
                        var subtaskToCreate = mapper.Map<TaskDmDb>(subtask);
                        await taskRepo.AddSubtask(updatedTask, subtaskToCreate, subtask.Author.Login);
                    }
                }
            }

            return true;
        }

        public async Task<bool> DeleteItem(string path) => await itemRepo.Delete(path);

        public async Task<bool> DeleteItemFromTask(Item item, TaskDm task) 
            => await itemRepo.BreakLinkWithTask(item.Id, task.Id);

        public async Task<bool> DeleteItemFromProject(Item item, Project project) 
            => await itemRepo.BreakLinkWithProject(item.Id, project.Id);

        private async Task<ItemDb> GetItemToLink(Item item)
        {
            if (await itemRepo.IsExists(item.Path))
                return await itemRepo.Get(item.Path);
            else
            {
                var itemToAdd = mapper.Map<ItemDb>(item);
                return await itemRepo.Add(itemToAdd);
            };
        }
    }
}
