﻿using Microsoft.AspNetCore.Mvc;
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