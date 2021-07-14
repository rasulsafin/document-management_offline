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
    public class IssuesServiceTests
    {
        private static readonly string PARENT_ID = "60b4d2719fbb9657cf2e0cbf";
        private static IssuesService service;
        private static ServiceProvider serviceProvider;
        private static string existingIssueId;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<IssuesService>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = TEST_EMAIL;
            var password = TEST_PASSWORD;
            var companyCode = TEST_COMPANY;
            var signInTask = authenticator.SignInAsync(email, password, companyCode);
            signInTask.Wait();
            var result = signInTask.Result;
            if (result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var issue = new Issue()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                State = STATE_OPENED,
                Type = ISSUE_TYPE,
                Description = "Test description",
                Title = "Test title",
            };
            var addIssueTask = service.TryPost(issue);
            addIssueTask.Wait();
            existingIssueId = addIssueTask.Result.Id;
            if (existingIssueId == null)
                Assert.Fail("Issue creation failed. Cannot test non-existing issue.");
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (!string.IsNullOrEmpty(existingIssueId))
            {
                await Task.Delay(MILLISECONDS_TIME_DELAY);
                await service.TryDelete(existingIssueId);
            }

            serviceProvider.Dispose();
        }

        [TestInitialize]
        public async Task Setup()
            => await Task.Delay(MILLISECONDS_TIME_DELAY);

        [TestMethod]
        public async Task TryGetExistingIssuesByIdsAsync_ReturnsIssuesByIdsList()
        {
            var projects = await service.GetAll(DateTime.MinValue);
            var existingIds = projects.Take(5).Select(p => p.Id).ToList();
            var result = await service.TryGetByIds(existingIds);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task TryGetNonExistingIssuesByIdsAsync_ReturnsEmptyList()
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
        public async Task TryGetExistingIssueByIdAsync_ReturnsIssue()
        {
            var result = await service.TryGetById(existingIssueId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Ancestry != string.Empty);
            Assert.IsNotNull(result.Ancestry);
            Assert.IsTrue(result.Type != string.Empty);
            Assert.IsNotNull(result.Type);
        }

        [TestMethod]
        public async Task TryGetNonExistingIssueByIdAsync_ReturnsNull()
        {
            var nonExistingId = $"nonExistingId{Guid.NewGuid()}";
            var result = await service.TryGetById(nonExistingId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TryPostIssueAsync_ReturnsAddedIssue()
        {
            var issue = new Issue()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                State = STATE_OPENED,
                Type = ISSUE_TYPE,
                Description = "Test description",
                Title = "Test title",
            };

            var result = await service.TryPost(issue);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);

            await Task.Delay(MILLISECONDS_TIME_DELAY);
            await service.TryDelete(result.Id);
        }

        [TestMethod]
        public async Task TryPatchIssueTitleAsync_ReturnsIssueWithNewTitle()
        {
            var existingIssue = await service.TryGetById(existingIssueId);
            var oldValue = existingIssue.Title;
            var newValue = "[PATCHED:]" + existingIssue.Title;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingIssue.Id },
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

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.Title);
            Assert.AreEqual(updatedIssue.Title, newValue);
            Assert.AreNotEqual(updatedIssue.Title, oldValue);
        }

        [TestMethod]
        public async Task TryPatchIssueDescriptionAsync_ReturnsIssueWithNewDescription()
        {
            var existingIssue = await service.TryGetById(existingIssueId);
            var oldValue = existingIssue.Description;
            var newValue = "[PATCHED:]" + existingIssue.Description;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingIssue.Id },
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

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.Description);
            Assert.AreEqual(updatedIssue.Description, newValue);
            Assert.AreNotEqual(updatedIssue.Description, oldValue);
        }

        [TestMethod]
        public async Task TryPatchIssueDueDateAsync_ReturnsIssueWithNewDueDate()
        {
            var existingIssue = await service.TryGetById(existingIssueId);
            var oldValue = existingIssue.DueDate;
            var newValue = DateTime.Now.ToUnixTime();

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingIssue.Id },
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

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.DueDate);
            Assert.AreEqual(updatedIssue.DueDate, newValue);
            Assert.AreNotEqual(updatedIssue.DueDate, oldValue);
        }

        [TestMethod]
        public async Task TryPatchIssueStateAsync_ReturnsIssueWithNewState()
        {
            var existingIssue = await service.TryGetById(existingIssueId);
            var oldValue = existingIssue.State;
            var newValue = existingIssue.State == STATE_OPENED ?
                STATE_COMPLETED : STATE_VERIFIED;

            var updatedValues = new UpdatedValues
            {
                Ids = new[] { existingIssue.Id },
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

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.State);
            Assert.AreEqual(updatedIssue.State, newValue);
            Assert.AreNotEqual(updatedIssue.State, oldValue);
        }

        [TestMethod]
        public async Task TryDeleteExistingIssueAsync_ReturnsTrue()
        {
            var issue = new Issue()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = PARENT_ID,
                ParentType = ELEMENT_TYPE,
                State = STATE_OPENED,
                Type = ISSUE_TYPE,
                Description = "Test description",
                Title = "Test issue for deletion",
            };
            var newAddedIssue = await service.TryPost(issue);

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var result = await service.TryDelete(newAddedIssue.Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TryDeleteNonExistingIssueAsync_ReturnsFalse()
        {
            var nonExistingId = $"nonExistingId{Guid.NewGuid()}";
            var result = await service.TryDelete(nonExistingId);

            Assert.IsFalse(result);
        }
    }
}
