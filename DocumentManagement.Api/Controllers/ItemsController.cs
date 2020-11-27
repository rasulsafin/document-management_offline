using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Models;
using MRS.DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRS.DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private IItemService service;

        public ItemsController(IItemService itemService) => service = itemService;

        [HttpPost]
        [Route("Add/Project")]
        public async Task<ID<Item>> Add(ItemToCreate data, [FromHeader] ID<Project> parentProject) => await service.Add(data, parentProject);

        [HttpPost]
        [Route("Add/Objective")]
        public async Task<ID<Item>> Add(ItemToCreate data, [FromHeader] ID<Objective> parentObjective) => await service.Add(data, parentObjective);

        [HttpPost]
        [Route("Link/Project")]
        public async Task Link(ID<Item> itemID, [FromHeader] ID<Project> projectID) => await service.Link(itemID, projectID);

        [HttpPost]
        [Route("Link/Objective")]
        public async Task Link(ID<Item> itemID, [FromHeader] ID<Objective> objectiveID) => await service.Link(itemID, objectiveID);

        [HttpPost]
        [Route("Unlink/Project")]
        public async Task Unlink(ID<Item> itemID, [FromHeader] ID<Project> projectID) => await service.Unlink(itemID, projectID);

        [HttpPost]
        [Route("Unlink/Objective")]
        public async Task Unlink(ID<Item> itemID, [FromHeader] ID<Objective> objectiveID) => await service.Unlink(itemID, objectiveID);

        [HttpPut]
        [Route("Update/Item")]
        public async Task Update(Item item) => await service.Update(item);

        [HttpGet]
        [Route("Find/Id")]
        public async Task<Item> Find(ID<Item> itemID) => await service.Find(itemID);

        [HttpGet]
        [Route("Find/Path")]
        public async Task<Item> Find(string path) => await service.Find(path);

        [HttpGet]
        [Route("Get/Project")]
        public async Task<IEnumerable<Item>> GetItems(ID<Project> projectID) => await service.GetItems(projectID);

        [HttpGet]
        [Route("Get/Objective")]
        public async Task<IEnumerable<Item>> GetItems(ID<Objective> objectiveID) => await service.GetItems(objectiveID);
    }
}
