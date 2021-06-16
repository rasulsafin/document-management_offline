using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class ProjectsServiceTests
    {
        private static readonly DateTime DATE = DateTime.MinValue;

        private static ProjectsService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<ProjectsService>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = "asidorov@briogroup.ru";
            var password = "GhundU72!c";
            var companyCode = "skprofitgroup";
            var signInTask = authenticator.SignInAsync(email, password, companyCode);
            signInTask.Wait();
            if (signInTask.Result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
         => await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

        [TestMethod]
        public async Task GetAllProjectsAsync_ReturnsProjectsList()
        {
            var result = await service.GetAll(DATE);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetRootProjectsAsync_ReturnsRootProjectsList()
        {
            var result = await service.GetRootProjects();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(CheckRootProjects(result));
        }

        [TestMethod]
        public async Task TryGetExistingProjectsByIdsAsync_ReturnsProjectsByIdsList()
        {
            var projects = await service.GetAll(DATE);
            var existingIds = projects.Take(5).Select(p => p.Id).ToList();
            var result = await service.TryGetByIds(existingIds);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task TryGetNonExistingProjectsByIdsAsync_ReturnsEmptyList()
        {
            var nonExistingIds = new List<string>()
            {
                $"nonExistingId1{Guid.NewGuid()}",
                $"nonExistingId2{Guid.NewGuid()}",
                $"nonExistingId3{Guid.NewGuid()}",
            };

            var result = await service.TryGetByIds(nonExistingIds);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TryGetExistingProjectByIdAsync_ReturnsProject()
        {
            var projects = await service.GetAll(DATE);
            var existingID = projects.First().Id;

            var result = await service.TryGetById(existingID);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Ancestry != string.Empty);
            Assert.IsNotNull(result.Ancestry);
            Assert.IsTrue(result.Type != string.Empty);
            Assert.IsNotNull(result.Type);
        }

        [TestMethod]
        public async Task TryGetNonExistingProjectsByIdAsync_ReturnsNull()
        {
            var nonExistingId = $"nonExistingId{Guid.NewGuid()}";
            var result = await service.TryGetById(nonExistingId);

            Assert.IsNull(result);
        }

        private bool CheckRootProjects(IEnumerable<Project> projects)
        {
            foreach (var pj in projects)
            {
                if (!pj.Ancestry.EndsWith(Constants.ROOT))
                    return false;
            }

            return true;
        }
    }
}
