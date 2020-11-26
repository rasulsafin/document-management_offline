using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private IProjectService service;

        public ProjectsController(IProjectService projectService) => service = projectService;

        [HttpGet]
        public async Task<IEnumerable<Project>> GetUserProjects(ID<User> userID) => await service.GetUserProjects(userID);

        [HttpPost]
        public async Task<ID<Project>> Add(ID<User> owner, string title) => await service.Add(owner, title);

        [HttpDelete]
        public async Task Remove(ID<Project> projectID) => await service.Remove(projectID);

        [HttpPut]
        public async Task Update(Project projectData) => await service.Update(projectData);

        [HttpGet]
        public async Task<Project> Find(ID<Project> projectID) => await service.Find(projectID);

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers(ID<Project> projectID) => await service.GetUsers(projectID);

        [HttpPost]
        public async Task AddUsers(ID<Project> projectID, IEnumerable<ID<User>> users) => await service.AddUsers(projectID, users);

        [HttpDelete]
        public async Task RemoveUsers(ID<Project> projectID, IEnumerable<ID<User>> users) => await service.RemoveUsers(projectID, users);
    }
}
