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
        public async Task<ID<ItemDto>> AddToProject([FromBody] ItemToCreateDto data, [FromRoute] int parentProject) => await service.Add(data, new ID<ProjectDto>(parentProject));

        [HttpPost]
        [Route("objective/{parentobjective}")]
        public async Task<ID<ItemDto>> AddToObjective([FromBody] ItemToCreateDto data, [FromRoute] int parentObjective) => await service.Add(data, new ID<ObjectiveDto>(parentObjective));

        [HttpPost]
        [Route("link/project/{projectID}/item/{itemId}")]
        public async Task LinkToProject([FromRoute] int itemID, [FromRoute] int projectID) => await service.Link(new ID<ItemDto>(itemID), new ID<ProjectDto>(projectID));

        [HttpPost]
        [Route("link/objective/{objectiveID}/item/{itemId}")]
        public async Task LinkToObjective([FromRoute] int itemID, [FromRoute] int objectiveID) => await service.Link(new ID<ItemDto>(itemID), new ID<ObjectiveDto>(objectiveID));

        [HttpPost]
        [Route("unlink/project/{projectID}/item/{itemId}")]
        public async Task UnlinkFromProject([FromRoute] int itemID, [FromRoute] int projectID) => await service.Unlink(new ID<ItemDto>(itemID), new ID<ProjectDto>(projectID));

        [HttpPost]
        [Route("unlink/objective/{objectiveID}/item/{itemId}")]
        public async Task UnlinkFromObjective([FromRoute] int itemID, [FromRoute] int objectiveID) => await service.Unlink(new ID<ItemDto>(itemID), new ID<ObjectiveDto>(objectiveID));

        [HttpPut]
        public async Task Update([FromBody] ItemDto item) => await service.Update(item);

        [HttpGet]
        [Route("{itemID}")]
        public async Task<ItemDto> Find([FromRoute] int itemID) => await service.Find(new ID<ItemDto>(itemID));

        [HttpGet]
        public async Task<ItemDto> Find([FromQuery] string path) => await service.Find(path);

        [HttpGet]
        [Route("project/{projectID}")]
        public async Task<IEnumerable<ItemDto>> GetProjectItems([FromRoute] int projectID) => await service.GetItems(new ID<ProjectDto>(projectID));

        [HttpGet]
        [Route("objective/{objectiveID}")]
        public async Task<IEnumerable<ItemDto>> GetObjectiveItems([FromQuery] int objectiveID) => await service.GetItems(new ID<ObjectiveDto>(objectiveID));
    }
}
