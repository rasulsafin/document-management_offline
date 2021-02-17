using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models.ObjectBase;
using MRS.DocumentManagement.Connection.LementPro.Utilities;

namespace MRS.DocumentManagement.Connection.LementPro.Services
{
    public class TasksService
    {
        private AuthenticationService authenticationService;
        private HttpRequestUtility requestUtility;
        private CommonRequestsUtility commonRequests;

        public TasksService(
            AuthenticationService authenticationService,
            HttpRequestUtility requestUtility,
            CommonRequestsUtility commonRequests)
        {
            this.authenticationService = authenticationService;
            this.requestUtility = requestUtility;
            this.commonRequests = commonRequests;
        }

        //public async Task<IEnumerable<ObjectBase>> GetTasksAsync()
        //{
        //    var token = await authenticationService.EnsureAccessValidAsync();

        //}
    }
}
