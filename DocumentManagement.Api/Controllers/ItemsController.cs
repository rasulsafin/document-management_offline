using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private IItemService service;

        public ItemsController(IItemService itemService) => service = itemService;

        [HttpPost]
        [Route("project/{parentproject}")]
        public async Task<ID<ItemDto>> AddToProject(ItemToCreateDto data, int parentProject) => await service.Add(data, new ID<ProjectDto>(parentProject));

        [HttpPost]
        [Route("objective/{parentobjective}")]
        public async Task<ID<ItemDto>> AddToObjective(ItemToCreateDto data, int parentObjective) => await service.Add(data, new ID<ObjectiveDto>(parentObjective));

        [HttpPost]
        [Route("link/project")]
        public async Task Link(ID<ItemDto> itemID, [FromHeader] ID<ProjectDto> projectID) => await service.Link(itemID, projectID);

        [HttpPost]
        [Route("link/objective")]
        public async Task Link(ID<ItemDto> itemID, [FromHeader] ID<ObjectiveDto> objectiveID) => await service.Link(itemID, objectiveID);

        [HttpPost]
        [Route("unlink/project")]
        public async Task Unlink(ID<ItemDto> itemID, [FromHeader] ID<ProjectDto> projectID) => await service.Unlink(itemID, projectID);

        [HttpPost]
        [Route("unlink/objective")]
        public async Task Unlink(ID<ItemDto> itemID, [FromHeader] ID<ObjectiveDto> objectiveID) => await service.Unlink(itemID, objectiveID);

        [HttpPut]
        [Route("update/item")]
        public async Task Update(ItemDto item) => await service.Update(item);

        [HttpGet]
        [Route("id")]
        public async Task<ItemDto> Find(ID<ItemDto> itemID) => await service.Find(itemID);

        [HttpGet]
        [Route("path")]
        public async Task<ItemDto> Find(string path) => await service.Find(path);

        [HttpGet]
        [Route("project")]
        public async Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID) => await service.GetItems(projectID);

        [HttpGet]
        [Route("objective")]
        public async Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID) => await service.GetItems(objectiveID);
    }
}
