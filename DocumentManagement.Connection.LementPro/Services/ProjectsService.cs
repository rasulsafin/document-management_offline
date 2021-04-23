using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Properties;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class ProjectsService : IDisposable
    {
        private readonly HttpRequestUtility requestUtility;

        public ProjectsService(HttpRequestUtility requestUtility, CommonRequestsUtility commonRequests)
        {
            this.requestUtility = requestUtility;
            CommonRequests = commonRequests;
        }

        public CommonRequestsUtility CommonRequests { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task<IEnumerable<ObjectBase>> GetAllProjectsAsync()
            => await CommonRequests.RetriveObjectsListAsync(OBJECTTYPE_PROJECT);

        public async Task<ObjectBase> GetProjectAsync(int projectId)
            => await CommonRequests.GetObjectAsync(projectId);

        public async Task<LementProType> GetDefaultProjectTypeAsync()
            => (await CommonRequests.GetObjectsTypes(OBJECTTYPE_PROJECT)).FirstOrDefault();

        public async Task<ObjectBaseCreateResult> CreateProjectAsync(ObjectBaseToCreate projectToCreate)
            => await CommonRequests.CreateObjectAsync(projectToCreate);

        public async Task<ObjectBase> UpdateProjectAsync(ObjectBaseToUpdate taskToUpdate)
        {
            var response = await requestUtility.GetResponseAsync(Resources.MethodObjectEdit, taskToUpdate);

            // Response contains some metadata and object
            var updatedTask = response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
            return updatedTask;
        }

        public async Task<ObjectBase> DeleteProjectAsync(int projectId)
            => await CommonRequests.ArchiveObjectAsync(projectId);
    }
}
