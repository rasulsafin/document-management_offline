using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;
using static MRS.DocumentManagement.Connection.MrsPro.Tests.TestConstants;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class ProjectsServiceTests
    {
        private static readonly string PARENT_ID = "60b4d2719fbb9657cf2e0cbf";
        private static ProjectsService service;
        private static ServiceProvider serviceProvider;
        private static string existingProjectId;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<ProjectsService>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = TEST_EMAIL;
            var password = TEST_PASSWORD;
            var companyCode = TEST_COMPANY;
            var signInTask = authenticator.SignInAsync(email, password, companyCode);
            signInTask.Wait();
            if (signInTask.Result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var project = new Project()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                Name = "Name",
                Type = ELEMENT_TYPE,
            };
            var addProjectTask = service.TryPost(project);
            addProjectTask.Wait();
            existingProjectId = addProjectTask.Result.Id;
            if (existingProjectId == null)
                Assert.Fail("Issue creation failed. Cannot test non-existing issue.");
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (!string.IsNullOrEmpty(existingProjectId))
            {
                await Task.Delay(MILLISECONDS_TIME_DELAY);
                await service.TryDelete(existingProjectId);
            }

            serviceProvider.Dispose();
        }

        [TestInitialize]
        public async Task Setup()
         => await Task.Delay(MILLISECONDS_TIME_DELAY);

        [TestCleanup]
        public async Task Cleanup()
        {
            await Task.Delay(MILLISECONDS_TIME_DELAY);
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnsProjectsList()
        {
            var result = await service.GetAll();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(CheckRootProjects(result));
        }

        [TestMethod]
        public async Task TryGetAsync_ExistingProjectsByIds_ReturnsProjectsByIdsList()
        {
            var projects = await service.GetAll();
            var existingIds = projects.Take(5).Select(p => p.Id).ToList();
            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var result = await service.TryGetByIds(existingIds);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task TryGetAsync_NonExistingProjectsByIds_ReturnsEmptyList()
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
        public async Task TryGetAsync_ExistingProjectById_ReturnsProject()
        {
            var result = await service.TryGetById(existingProjectId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Ancestry != string.Empty);
            Assert.IsNotNull(result.Ancestry);
            Assert.IsTrue(result.Type != string.Empty);
            Assert.IsNotNull(result.Type);
            Assert.IsNotNull(result.Name);
            Assert.IsNotNull(result.Id);
            Assert.IsTrue(result.Id != string.Empty);
        }

        [TestMethod]
        public async Task TryGetAsync_NonExistingProjectsById_ReturnsNull()
        {
            var nonExistingId = $"nonExistingId{Guid.NewGuid()}";
            var result = await service.TryGetById(nonExistingId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TryPostAsync_ProjectWithParentProject_ReturnsAddedProject()
        {
            var project = new Project()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                Type = ELEMENT_TYPE,
                Name = "Test project name",
            };

            var result = await service.TryPost(project);

            await Task.Delay(MILLISECONDS_TIME_DELAY);
            await service.TryDelete(result.Id);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Id != string.Empty);
            Assert.AreEqual(result.ParentId, project.ParentId);
            Assert.AreEqual(result.ParentType, project.ParentType);
            Assert.AreEqual(result.Type, project.Type);
            Assert.AreEqual(result.Name, project.Name);
        }

        [TestMethod]
        public async Task TryPatchAsync_ProjectName_ReturnsProjectWithNewTitle()
        {
            var existingProject = await service.TryGetById(existingProjectId);
            var oldValue = existingProject.Name;
            var newValue = "[PATCHED:]" + existingProject.Name;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingProject.Id },
                Patch = new Patch[]
                {
                    new Patch()
                    {
                        Value = newValue,
                        Path = "/name",
                    },
                },
            };

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var updatedProject = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedProject);
            Assert.IsNotNull(updatedProject.Id);
            Assert.IsNotNull(updatedProject.Name);
            Assert.AreEqual(updatedProject.Name, newValue);
            Assert.AreNotEqual(updatedProject.Name, oldValue);
        }

        [TestMethod]
        public async Task TryPatchAsync_ProjectAncestry_ReturnsProjectWithNewParent()
        {
            var project = new Project()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                Type = ELEMENT_TYPE,
                Name = "Test project name",
            };

            var newProject = await service.TryPost(project);

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var existingProject = await service.TryGetById(existingProjectId);
            var oldValue = existingProject.Ancestry;
            var newValue = newProject.GetExternalId();

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingProject.Id },
                Patch = new Patch[]
                {
                    new Patch()
                    {
                        Value = newValue,
                        Path = "/ancestry",
                    },
                },
            };

            await Task.Delay(MILLISECONDS_TIME_DELAY);
            var updatedProject = await service.TryPatch(updatedValues);

            await Task.Delay(MILLISECONDS_TIME_DELAY);
            await service.TryDelete(newProject.Id);

            Assert.IsNotNull(updatedProject);
            Assert.IsNotNull(updatedProject.Id);
            Assert.IsNotNull(updatedProject.Name);
            Assert.AreEqual(updatedProject.Ancestry, newValue);
            Assert.AreNotEqual(updatedProject.Ancestry, oldValue);
            Assert.AreEqual(updatedProject.ParentId, newProject.Id);
        }

        [TestMethod]
        public async Task TryDeleteAsync_ExistingProject_ReturnsTrue()
        {
            var project = new Project()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                Type = ELEMENT_TYPE,
                Name = "Test project for deletion",
            };
            var newAddedProject = await service.TryPost(project);
            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var result = await service.TryDelete(newAddedProject.Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TryDeleteAsync_NonExistingProject_ReturnsFalse()
        {
            var nonExistingId = $"nonExistingId{Guid.NewGuid()}";
            var result = await service.TryDelete(nonExistingId);

            Assert.IsFalse(result);
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
