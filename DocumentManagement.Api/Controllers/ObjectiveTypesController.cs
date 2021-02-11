﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using static MRS.DocumentManagement.Api.Validators.ServiceResponsesValidator;

namespace DocumentManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ObjectiveTypesController : ControllerBase
    {
        private IObjectiveTypeService service;

        public ObjectiveTypesController(IObjectiveTypeService objectiveTypeService) => service = objectiveTypeService;

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] string typeName)
        {
            try
            {
                var typeId = await service.Add(typeName);
                return ValidateId(typeId);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Find([FromRoute] int id)
        {
            var foundType = await service.Find(new ID<ObjectiveTypeDto>(id));
            return ValidateFoundObject(foundType);
        }

        [HttpGet]
        [Route("name")]
        public async Task<IActionResult> Find([FromQuery] string typename)
        {
            var foundType = await service.Find(typename);
            return ValidateFoundObject(foundType);
        }

        [HttpGet]
        [Route("list/{id}")]
        public async Task<IActionResult> GetAllObjectiveTypes([FromRoute] int connectionTypeId)
        {
            var allTypes = await service.GetObjectiveTypes(new ID<ConnectionTypeDto>(connectionTypeId));
            return ValidateCollection(allTypes);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Remove([FromRoute] int id)
        {
            try
            {
                var removed = await service.Remove(new ID<ObjectiveTypeDto>(id));
                return ValidateFoundRelatedResult(removed);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e);
            }
        }
    }
}
