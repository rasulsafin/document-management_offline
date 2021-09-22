using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Connections.LementPro.Properties;
using Brio.Docs.Connections.LementPro.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Brio.Docs.Connections.LementPro.LementProConstants;

namespace Brio.Docs.Connections.LementPro.Services
{
    public class ProjectsService
    {
        private readonly HttpRequestUtility requestUtility;
        private readonly ILogger<ProjectsService> logger;

        public ProjectsService(
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests,
            ILogger<ProjectsService> logger)
        {
            this.requestUtility = requestUtility;
            this.logger = logger;
            CommonRequests = commonRequests;
            logger.LogTrace("ProjectService created");
        }

        public CommonRequestsUtility CommonRequests { get; private set; }

        public async Task<IEnumerable<ObjectBase>> GetAllProjectsAsync()
        {
            logger.LogTrace("GetAllProjectsAsync started");
            return await CommonRequests.RetrieveObjectsListAsync(OBJECTTYPE_PROJECT);
        }

        public async Task<ObjectBase> GetProjectAsync(int projectId)
        {
            logger.LogTrace("GetProjectAsync started with projectId: {@ProjectID}", projectId);
            return await CommonRequests.GetObjectAsync(projectId);
        }

        public async Task<LementProType> GetDefaultProjectTypeAsync()
        {
            logger.LogTrace("GetDefaultProjectTypeAsync started");
            return (await CommonRequests.GetObjectsTypes(OBJECTTYPE_PROJECT)).FirstOrDefault();
        }

        public async Task<ObjectBaseCreateResult> CreateProjectAsync(ObjectBaseToCreate projectToCreate)
        {
            logger.LogTrace("CreateProjectAsync started with projectToCreate: {@Project}", projectToCreate);
            return await CommonRequests.CreateObjectAsync(projectToCreate);
        }

        public async Task<ObjectBase> UpdateProjectAsync(ObjectBaseToUpdate taskToUpdate)
        {
            logger.LogTrace("UpdateProjectAsync started with taskToUpdate: {@Objective}", taskToUpdate);
            var response = await requestUtility.GetResponseAsync(Resources.MethodObjectEdit, taskToUpdate);

            // Response contains some metadata and object
            var updatedTask = response[RESPONSE_OBJECT_NAME].ToObject<ObjectBase>();
            logger.LogDebug("Updated task: {@Objective}", updatedTask);
            return updatedTask;
        }

        public async Task<ObjectBase> DeleteProjectAsync(int projectId)
        {
            logger.LogTrace("DeleteProjectAsync started with projectId: {@ProjectID}", projectId);
            return await CommonRequests.ArchiveObjectAsync(projectId);
        }
    }
}
