﻿using System;
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
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
         => await Task.Delay(MILLISECONDS_TIME_DELAY);

        [TestMethod]
        public async Task GetAllProjectsAsync_ReturnsProjectsList()
        {
            var result = await service.GetAll();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(CheckRootProjects(result));
        }

        //[TestMethod]
        //public async Task GetRootProjectsAsync_ReturnsRootProjectsList()
        //{
        //    var result = await service.GetAll();

        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Any());
        //    Assert.IsTrue(CheckRootProjects(result));
        //}

        [TestMethod]
        public async Task TryGetExistingProjectsByIdsAsync_ReturnsProjectsByIdsList()
        {
            var projects = await service.GetAll();
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
            var projects = await service.GetAll();
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

        [TestMethod]
        public async Task TryPostProjectAsync_ReturnsAddedProject()
        {
            var project = new Project()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                State = STATE_OPENED,
                Type = ELEMENT_TYPE,
                Description = "Test description",
                Title = "Test title",
            };

            var result = await service.TryPost(project);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);

            await Task.Delay(MILLISECONDS_TIME_DELAY);
            await service.TryDelete(result.Id);
        }

        [TestMethod]
        public async Task TryPatchProjectTitleAsync_ReturnsProjectWithNewTitle()
        {
            var existingProject = await service.TryGetById(existingProjectId);
            var oldValue = existingProject.Title;
            var newValue = "[PATCHED:]" + existingProject.Title;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingProject.Id },
                Patch = new Patch[]
                {
                    new Patch()
                    {
                        Value = newValue,
                        Path = "/title",
                    },
                },
            };

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var updatedProject = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedProject);
            Assert.IsNotNull(updatedProject.Id);
            Assert.IsNotNull(updatedProject.Title);
            Assert.AreEqual(updatedProject.Title, newValue);
            Assert.AreNotEqual(updatedProject.Title, oldValue);
        }

        [TestMethod]
        public async Task TryPatchProjectDescriptionAsync_ReturnsProjectWithNewDescription()
        {
            var existingProject = await service.TryGetById(existingProjectId);
            var oldValue = existingProject.Description;
            var newValue = "[PATCHED:]" + existingProject.Description;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingProject.Id },
                Patch = new Patch[]
                {
                    new Patch()
                    {
                        Value = newValue,
                        Path = "/description",
                    },
                },
            };

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var updatedProject = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedProject);
            Assert.IsNotNull(updatedProject.Id);
            Assert.IsNotNull(updatedProject.Description);
            Assert.AreEqual(updatedProject.Description, newValue);
            Assert.AreNotEqual(updatedProject.Description, oldValue);
        }

        [TestMethod]
        public async Task TryPatchProjectDueDateAsync_ReturnsProjectWithNewDueDate()
        {
            var existingProject = await service.TryGetById(existingProjectId);
            var oldValue = existingProject.DueDate;
            var newValue = DateTime.Now.ToUnixTime();

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingProject.Id },
                Patch = new Patch[]
                {
                    new Patch()
                    {
                        Value = newValue,
                        Path = "/dueDate",
                    },
                },
            };

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var updatedProject = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedProject);
            Assert.IsNotNull(updatedProject.Id);
            Assert.IsNotNull(updatedProject.DueDate);
            Assert.AreEqual(updatedProject.DueDate, newValue);
            Assert.AreNotEqual(updatedProject.DueDate, oldValue);
        }

        [TestMethod]
        public async Task TryPatchProjectStateAsync_ReturnsProjectWithNewState()
        {
            var existingProject = await service.TryGetById(existingProjectId);
            var oldValue = existingProject.State;
            var newValue = existingProject.State == STATE_OPENED ?
                STATE_COMPLETED : STATE_VERIFIED;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingProject.Id },
                Patch = new Patch[]
                {
                    new Patch()
                    {
                        Value = newValue,
                        Path = "/state",
                    },
                },
            };

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var updatedProject = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedProject);
            Assert.IsNotNull(updatedProject.Id);
            Assert.IsNotNull(updatedProject.State);
            Assert.AreEqual(updatedProject.State, newValue);
            Assert.AreNotEqual(updatedProject.State, oldValue);
        }

        [TestMethod]
        public async Task TryDeleteExistingProjectAsync_ReturnsTrue()
        {
            var project = new Project()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                State = STATE_OPENED,
                Type = ELEMENT_TYPE,
                Description = "Test description",
                Title = "Test project for deletion",
            };
            var newAddedProject = await service.TryPost(project);

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var result = await service.TryDelete(newAddedProject.Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TryDeleteNonExistingProjectAsync_ReturnsFalse()
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
