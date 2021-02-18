using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class TasksService : IDisposable
    {
        private readonly AuthenticationService authenticationService;
        private readonly HttpRequestUtility requestUtility;
        private readonly CommonRequestsUtility commonRequests;

        public TasksService(
            AuthenticationService authenticationService,
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests)
        {
            this.authenticationService = authenticationService;
            this.requestUtility = requestUtility;
            this.commonRequests = commonRequests;
        }

        public void Dispose()
        {
            authenticationService.Dispose();
            requestUtility.Dispose();
            commonRequests.Dispose();
        }

        public async Task<IEnumerable<ObjectBase>> GetAllTasksAsync()
        {
            var tasks = await commonRequests.RetriveObjectsListAsync(OBJECT_TYPE_SINGLE_TASK);
            return tasks;
        }

        public async Task<ObjectBase> GetTaskAsync(int taskId)
        {
            var task = await commonRequests.GetObjectAsync(taskId);
            return task;
        }

        public async Task<List<LementProType>> GetTasksTypesAsync()
            => await commonRequests.GetObjectsTypes(OBJECT_TYPE_SINGLE_TASK);

        public async Task<bool> CreateTask(ObjectBaseToCreate objectToCreate)
            => await commonRequests.CreateObjectAsync(objectToCreate);

        // To delete use this:
        //https://briogroup.lement.pro/Services/ObjectBase/Archive.do
        //with body id=403690
    }
}
