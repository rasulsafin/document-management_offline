using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.LementPro.Models;
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

        public static async Task<LementProConnectionContext> CreateContext(ConnectionInfoDto info, DateTime lastSynchronizationDate)
        {
            var connection = new HttpConnection();
            var requestUtility = new HttpRequestUtility(connection);
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);

            var context = new LementProConnectionContext
            {
                TasksService = new TasksService(requestUtility, commonRequests),
                BimsService = new BimsService(requestUtility, commonRequests),
            };

            await authService.SignInAsync(info);

            return context;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            throw new NotImplementedException();
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IReadOnlyCollection<ObjectiveExternalDto>> GetObjectives()
        {
            var lementTasks = await TasksService.GetAllTasksAsync();
            var files = await BimsService.GetAllBimFilesAsync();
            objectives = new List<ObjectiveExternalDto>();

            foreach (var task in lementTasks)
            {
                var objective = task.ToObjectiveExternalDto();
                objective.Items = GetIssueFiles(task, files);
                objectives.Add(objective);
            }

            return objectives;
        }

        protected override async Task<IReadOnlyCollection<ProjectExternalDto>> GetProjects()
        {
            throw new NotImplementedException("Projects managaring is not implemented");
        }

        private ICollection<ItemExternalDto> GetIssueFiles(ObjectBase issue, IEnumerable<ObjectBase> allBims)
        {
            var files = new List<ItemExternalDto>();
            var bimRef = issue.Values.BimRef;
            var bimFile = allBims.FirstOrDefault(b => b.ID == bimRef.ID).ToItemExternalDto();
            if (bimFile != null)
                files.Add(bimFile);

            // TODO implement adding non-BIM files after implementing functionality for them
            return files;
        }
    }
}
