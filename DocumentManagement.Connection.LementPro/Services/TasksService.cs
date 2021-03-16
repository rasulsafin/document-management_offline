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

        public TasksService()
        {
        }

        public TasksService(
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests)
        {
            this.requestUtility = requestUtility;
            CommonRequests = commonRequests;
        }

        public CommonRequestsUtility CommonRequests { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task<IEnumerable<ObjectBase>> GetAllTasksAsync()
            => await CommonRequests.RetriveObjectsListAsync(OBJECTTYPE_SINGLE_TASK);

        public async Task<ObjectBase> GetTaskAsync(int taskId)
            => await CommonRequests.GetObjectAsync(taskId);

        public async Task<List<LementProType>> GetTasksTypesAsync()
            => await CommonRequests.GetObjectsTypes(OBJECTTYPE_SINGLE_TASK);

        public async Task<ObjectBaseCreateResult> CreateTaskAsync(ObjectBaseToCreate objectToCreate)
            => await CommonRequests.CreateObjectAsync(objectToCreate);

        public async Task<ObjectBase> DeleteTaskAsync(int objectId)
            => await CommonRequests.ArchiveObjectAsync(objectId);

        public async Task<ObjectBase> UpdateTaskAsync(TaskToUpdate taskToUpdate)
        {
            var response = await requestUtility.GetResponseAsync(Resources.MethodTaskUpdate, taskToUpdate);

            // Response contains some metadata and object
            var updatedTask = response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
            return updatedTask;
        }
    }
}
