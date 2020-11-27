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
        [Route("Get/Projects/User")]
        public async Task<IEnumerable<Project>> GetUserProjects(ID<User> userID) => await service.GetUserProjects(userID);

        [HttpPost]
        [Route("Add/Project/User")]
        public async Task<ID<Project>> Add(ID<User> owner, string title) => await service.Add(owner, title);

        [HttpDelete]
        [Route("Remove/Project")]
        public async Task Remove(ID<Project> projectID) => await service.Remove(projectID);

        [HttpPut]
        [Route("Update/Project")]
        public async Task Update(Project projectData) => await service.Update(projectData);

        [HttpGet]
        [Route("Find/Project")]
        public async Task<Project> Find(ID<Project> projectID) => await service.Find(projectID);

        [HttpGet]
        [Route("Get/Users/Project")]
        public async Task<IEnumerable<User>> GetUsers(ID<Project> projectID) => await service.GetUsers(projectID);

        [HttpPost]
        [Route("Add/Users/Project")]
        public async Task AddUsers([FromHeader] ID<Project> projectID, IEnumerable<ID<User>> users) => await service.AddUsers(projectID, users);

        [HttpDelete]
        [Route("Remove/Users/Project")]
        public async Task RemoveUsers([FromHeader] ID<Project> projectID, IEnumerable<ID<User>> users) => await service.RemoveUsers(projectID, users);
    }
}
