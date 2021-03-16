using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class ProjectsServiceTests
    {
        private static ProjectsService service;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var requestUtility = new HttpRequestUtility(new HttpConnection());
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);
            service = new ProjectsService(requestUtility, commonRequests);

            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (_, _) = await authService.SignInAsync(connectionInfo);
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => service.Dispose();

        [TestMethod]
        public async Task GetAllProjectsAsync_ProjectsDefaultFolderNotEmpty_ReturnsProjectsList()
        {
            var result = await service.GetAllProjectsAsync();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetProjectAsync_ExistingProject_ReturnsProject()
        {
            var projectId = 402014;

            var result = await service.GetProjectAsync(projectId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetDefaultProjectTypeAsync_AtLeasOneProjectTypeExists_ReturnsFirstProjectType()
        {
            var result = await service.GetDefaultProjectTypeAsync();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CreateProjecAsync_NewProjectWithCorrectFields_ReturnsTrue()
        {
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newProjectValue = new ObjectBaseValueToCreate
            {
                Type = 40178,
                Name = $"CreatedByIntegrTest {DateTime.Now.ToShortTimeString()}",
                StartDate = DateTime.Now.ToString(dateFormat),
            };

            var newProject = new ObjectBaseToCreate
            {
                Values = newProjectValue,
                FileIds = new List<int>(),
            };

            var result = await service.CreateProjectAsync(newProject);

            Assert.IsTrue(result.IsSuccess.GetValueOrDefault());
        }

        [TestMethod]
        public async Task UpdateProjectAsync_ExistingProject_ReturnsUpdatedObject()
        {
            var projectId = 402014;
            var existingProject = await service.GetProjectAsync(projectId);
            var updatedLabel = $"UPDATED: {existingProject.Values.Name}";

            var updatedProjectValue = new ObjectBaseValueToUpdate
            {
                ID = existingProject.ID,
                CreationDate = existingProject.Values.CreationDate,
                Name = updatedLabel,
                Type = existingProject.Values.Type.ID,
                StartDate = existingProject.Values.StartDate,
            };
            var projectToUpdate = new ObjectBaseToUpdate
            {
                ID = existingProject.ID,
                Values = updatedProjectValue,
            };

            var result = await service.UpdateProjectAsync(projectToUpdate);

            Assert.IsNotNull(result);
            Assert.AreEqual(updatedLabel, result.Values.Name);
        }

        [TestMethod]
        public async Task DeleteTaskAsync_CreatedNewTask_ReturnsTrue()
        {
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newProjectValue = new ObjectBaseValueToCreate
            {
                Type = 40178,
                Name = $"CreatedByIntegrTest {DateTime.Now.ToShortTimeString()}",
                StartDate = DateTime.Now.ToString(dateFormat),
            };

            var newProject = new ObjectBaseToCreate
            {
                Values = newProjectValue,
                FileIds = new List<int>(),
            };
            var created = await service.CreateProjectAsync(newProject);

            // Wait for creating (2 sec is enough usually)
            await Task.Delay(3000);

            var result = await service.DeleteProjectAsync(created.ID.Value);

            Assert.IsNotNull(result);
        }
    }
}
