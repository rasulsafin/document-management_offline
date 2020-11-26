using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private IItemService service;

        public ItemsController(IItemService itemService) => service = itemService;

        [HttpPost]
        public async Task<ID<Item>> Add(ItemToCreate data, ID<Project> parentProject) => await service.Add(data, parentProject);

        [HttpPost]
        public async Task<ID<Item>> Add(ItemToCreate data, ID<Objective> parentObjective) => await service.Add(data, parentObjective);

        [HttpPost]
        public async Task Link(ID<Item> itemID, ID<Project> projectID) => await service.Link(itemID, projectID);

        [HttpPost]
        public async Task Link(ID<Item> itemID, ID<Objective> objectiveID) => await service.Link(itemID, objectiveID);

        [HttpPost]
        public async Task Unlink(ID<Item> itemID, ID<Project> projectID) => await service.Unlink(itemID, projectID);

        [HttpPost]
        public async Task Unlink(ID<Item> itemID, ID<Objective> objectiveID) => await service.Unlink(itemID, objectiveID);

        [HttpPut]
        public async Task Update(Item item) => await service.Update(item);

        [HttpGet]
        public async Task<Item> Find(ID<Item> itemID) => await service.Find(itemID);

        [HttpGet]
        public async Task<Item> Find(string path) => await service.Find(path);

        [HttpGet]
        public async Task<IEnumerable<Item>> GetItems(ID<Project> projectID) => await service.GetItems(projectID);

        [HttpGet]
        public async Task<IEnumerable<Item>> GetItems(ID<Objective> objectiveID) => await service.GetItems(objectiveID);
    }
}
