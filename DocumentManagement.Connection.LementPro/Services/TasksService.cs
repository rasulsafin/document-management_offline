using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class TasksService
    {
        private readonly ILogger<TasksService> logger;
        private readonly HttpRequestUtility requestUtility;

        public TasksService(
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests,
            ILogger<TasksService> logger)
        {
            this.requestUtility = requestUtility;
            CommonRequests = commonRequests;
            this.logger = logger;
            logger.LogTrace("TasksService created");
        }

        public CommonRequestsUtility CommonRequests { get; private set; }

        public async Task<IEnumerable<ObjectBase>> GetAllTasksAsync()
        {
            logger.LogTrace("GetAllTasksAsync started");
            return await CommonRequests.RetrieveObjectsListAsync(OBJECTTYPE_SINGLE_TASK);
        }

        public async Task<ObjectBase> GetTaskAsync(int taskId)
        {
            logger.LogTrace("GetTaskAsync started with taskId: {@ObjectiveID}", taskId);
            return await CommonRequests.GetObjectAsync(taskId);
        }

        public async Task<List<LementProType>> GetTasksTypesAsync()
        {
            logger.LogTrace("GetTasksTypesAsync started");
            return await CommonRequests.GetObjectsTypes(OBJECTTYPE_SINGLE_TASK);
        }

        public async Task<ObjectBaseCreateResult> CreateTaskAsync(ObjectBaseToCreate objectToCreate)
        {
            logger.LogTrace("CreateTaskAsync started with objectToCreate: {@Objective}", objectToCreate);
            return await CommonRequests.CreateObjectAsync(objectToCreate);
        }

        public async Task<ObjectBase> DeleteTaskAsync(int objectId)
        {
            logger.LogTrace("DeleteTaskAsync started with objectId: {@ObjectiveID}", objectId);
            return await CommonRequests.ArchiveObjectAsync(objectId);
        }

        public async Task<ObjectBase> UpdateTaskAsync(ObjectBaseToUpdate taskToUpdate)
        {
            logger.LogTrace("UpdateTaskAsync started with taskToUpdate: {@Objective}", taskToUpdate);
            var response = await requestUtility.GetResponseAsync(Resources.MethodTaskUpdate, taskToUpdate);

            // Response contains some metadata and object
            var updatedTask = response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
            logger.LogDebug("Updated task: {@Objective}", updatedTask);
            return updatedTask;
        }
    }
}
