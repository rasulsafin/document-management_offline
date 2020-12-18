using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using System.Threading.Tasks;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private IItemService service;

        public ItemsController(IItemService itemService) => service = itemService;

        [HttpPost]
        [Route("project/{parentProject}")]
        public async Task<IActionResult> AddToProject([FromBody] ItemToCreateDto data, [FromRoute] int parentProject)
        {
            var itemID = await service.Add(data, new ID<ProjectDto>(parentProject));
            return ValidateId(itemID);
        }

        [HttpPost]
        [Route("objective/{parentObjective}")]
        public async Task<IActionResult> AddToObjective([FromBody] ItemToCreateDto data, [FromRoute] int parentObjective)
        {
            var itemID = await service.Add(data, new ID<ObjectiveDto>(parentObjective));
            return ValidateId(itemID);
        }

        [HttpPost]
        [Route("link/{itemId}/project/{projectID}")]
        public async Task<IActionResult> LinkToProject([FromRoute] int itemID, [FromRoute] int projectID)
        {
            var linked = await service.Link(new ID<ItemDto>(itemID), new ID<ProjectDto>(projectID));
            if (!linked)
                return Conflict();

            return Ok();
        }

        [HttpPost]
        [Route("link/{itemID}/objective/{objectiveID}")]
        public async Task<IActionResult> LinkToObjective([FromRoute] int itemID, [FromRoute] int objectiveID)
        {
            var linked = await service.Link(new ID<ItemDto>(itemID), new ID<ObjectiveDto>(objectiveID));
            if (!linked)
                return Conflict();

            return Ok();
        }

        [HttpPost]
        [Route("unlink/{itemID}/project/{projectID}/")]
        public async Task<IActionResult> UnlinkFromProject([FromRoute] int itemID, [FromRoute] int projectID)
        {
            var unlinked = await service.Unlink(new ID<ItemDto>(itemID), new ID<ProjectDto>(projectID));
            return ValidateFoundRelatedResult(unlinked);
        }

        [HttpPost]
        [Route("unlink/{itemID}/objective/{objectiveID}")]
        public async Task<IActionResult> UnlinkFromObjective([FromRoute] int itemID, [FromRoute] int objectiveID)
        {
            var unlinked = await service.Unlink(new ID<ItemDto>(itemID), new ID<ObjectiveDto>(objectiveID));
            return ValidateFoundRelatedResult(unlinked);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ItemDto item)
        {
            var updated = await service.Update(item);
            return ValidateFoundRelatedResult(updated);
        }

        [HttpGet]
        [Route("{itemID}")]
        public async Task<IActionResult> Find([FromRoute] int itemID)
        {
            var foundItem = await service.Find(new ID<ItemDto>(itemID));
            return ValidateFoundObject(foundItem);
        }

        [HttpGet]
        [Route("project/{projectID}")]
        public async Task<IActionResult> GetProjectItems([FromRoute] int projectID)
        {
            var items = await service.GetItems(new ID<ProjectDto>(projectID));
            return ValidateCollection(items);
        }

        [HttpGet]
        [Route("objective/{objectiveID}")]
        public async Task<IActionResult> GetObjectiveItems([FromRoute] int objectiveID)
        {
            var items = await service.GetItems(new ID<ObjectiveDto>(objectiveID));
            return ValidateCollection(items);
        }
    }
}
