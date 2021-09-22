using Brio.Docs.Connection.LementPro.Models;
using Brio.Docs.Connection.LementPro.Services;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class ProjectsServiceTests
    {
        private static ProjectsService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddLementPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<ProjectsService>();
            var connection = serviceProvider.GetService<LementProConnection>();

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

            await connection!.Connect(connectionInfo, CancellationToken.None);
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

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
            var dateFormat = "O";
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
