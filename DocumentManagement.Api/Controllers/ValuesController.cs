using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DocumentManagement.Data;
using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TDMS;

namespace DocumentManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ITaskRepository repo;
        private readonly IMapper map;
        
        public ValuesController(ITaskRepository repo, IMapper map)
        {
            this.repo = repo;
            this.map = map;

        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetTask(int id)
        //{
        //    var task = await repo.Get(id);
        //    var taskForReturn = map.Map<TaskDm>(task);

        //    return Ok(taskForReturn);
        //}

        [HttpGet("tdms")]
        public ActionResult<TaskDm> GetTask()
        {
            TDMSApplication tdms = new TDMSApplication();
            TDMSObject obj = tdms.GetObjectByGUID("{C2E3DF6D-A006-4380-8D48-E08A39203999}");

            var taskForReturn = new TaskDm() {
                Index = obj.GUID,
                Descriptions = obj.Description
            };

            return Ok(taskForReturn);
        }

    }
}
