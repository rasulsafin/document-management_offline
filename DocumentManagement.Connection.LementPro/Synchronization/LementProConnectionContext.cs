using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Synchronization
{
    public class LementProConnectionContext : AConnectionContext
    {
        private LementProConnectionContext()
        {
        }

        internal TasksService TasksService { get; private set; }

        internal BimsService BimsService { get; private set; }

        internal ProjectsService ProjectsService { get; private set; }

        public static async Task<LementProConnectionContext> CreateContext(ConnectionInfoExternalDto info)
        {
            var connection = new HttpConnection();
            var requestUtility = new HttpRequestUtility(connection);
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);

            var context = new LementProConnectionContext
            {
                TasksService = new TasksService(requestUtility, commonRequests),
                BimsService = new BimsService(requestUtility, commonRequests),
                ProjectsService = new ProjectsService(requestUtility, commonRequests),
            };

            await authService.SignInAsync(info);

            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
            => new LementProObjectivesSynchronizer(this);

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
            => new LementProProjectsSynchronizer(this);
    }
}
