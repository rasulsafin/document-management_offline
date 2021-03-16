using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using static MRS.DocumentManagement.Connection.LementPro.LementProConstants;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class ProjectsService : IDisposable
    {
        private readonly HttpRequestUtility requestUtility;
        private readonly CommonRequestsUtility commonRequests;

        public ProjectsService(HttpRequestUtility requestUtility, CommonRequestsUtility commonRequests)
        {
            this.requestUtility = requestUtility;
            this.commonRequests = commonRequests;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task<IEnumerable<ObjectBase>> GetAllProjectsAsync()
            => await commonRequests.RetriveObjectsListAsync(OBJECTTYPE_PROJECTS);

        public async Task<ObjectBase> GetProjectAsync(int projectId)
            => await commonRequests.GetObjectAsync(projectId);
    }
}
