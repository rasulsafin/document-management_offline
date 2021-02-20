using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class TasksService : IDisposable
    {
        private readonly HttpRequestUtility requestUtility;
        private readonly CommonRequestsUtility commonRequests;

        public TasksService(
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests)
        {
            this.requestUtility = requestUtility;
            this.commonRequests = commonRequests;
        }

        public void Dispose()
        {
            requestUtility.Dispose();
            commonRequests.Dispose();
        }

        public async Task<IEnumerable<ObjectBase>> GetAllTasksAsync()
            => await commonRequests.RetriveObjectsListAsync(OBJECT_TYPE_SINGLE_TASK);

        public async Task<ObjectBase> GetTaskAsync(int taskId)
            => await commonRequests.GetObjectAsync(taskId);

        public async Task<List<LementProType>> GetTasksTypesAsync()
            => await commonRequests.GetObjectsTypes(OBJECT_TYPE_SINGLE_TASK);

        public async Task<ObjectBaseCreateResult> CreateTask(ObjectBaseToCreate objectToCreate)
            => await commonRequests.CreateObjectAsync(objectToCreate);

        public async Task<bool> DeleteTaskAsync(int objectId)
            => await commonRequests.DeleteObjectAsync(objectId);

        public async Task<ObjectBase> UpdateTaskAsync(TaskToUpdate taskToUpdate)
        {
            var response = await requestUtility.GetResponseAsync(Resources.MethodTaskUpdate, taskToUpdate);

            // Response contains some metadata and object
            var updatedTask = response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
            return updatedTask;
        }
    }
}
