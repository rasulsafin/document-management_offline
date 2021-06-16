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

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class IssuesServiceTests
    {
        private static IssuesService service;
        private static ServiceProvider serviceProvider;
        private static string existingIssueId;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<IssuesService>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = "asidorov@briogroup.ru";
            var password = "GhundU72!c";
            var companyCode = "skprofitgroup";
            var signInTask = authenticator.SignInAsync(email, password, companyCode);
            signInTask.Wait();
            var result = signInTask.Result;
            if (result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            delay = Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var issue = new Issue()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = "60b4d2719fbb9657cf2e0cbf",
                ParentType = Constants.ELEMENT_TYPE,
                State = Constants.STATE_OPENED,
                Type = Constants.ISSUE_TYPE,
                Description = "Test description",
                Title = "Test title",
            };
            var addIssueTask = service.TryPost(issue);
            addIssueTask.Wait();
            existingIssueId = addIssueTask.Result.Id;
            if (existingIssueId == null)
                Assert.Fail("Issue creation failed. Cannot patch non-existing issue.");
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
        => await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

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
        public async Task TryPostIssue_ReturnsAddedIssue()
        {
            var issue = new Issue()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                ParentId = "60b4d2719fbb9657cf2e0cbf",
                ParentType = Constants.ELEMENT_TYPE,
                State = "opened",
                Type = Constants.ISSUE_TYPE,
                Description = "Test description",
                Title = "Test title",
            };

            var result = await service.TryPost(issue);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);
        }

        [TestMethod]
        public async Task TryPatchIssueTitle_ReturnsIssueWithNewTitle()
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

            await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.Title);
            Assert.AreEqual(updatedIssue.Title, newValue);
            Assert.AreNotEqual(updatedIssue.Title, oldValue);
        }

        [TestMethod]
        public async Task TryPatchIssueDescription_ReturnsIssueWithNewDescription()
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

            await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.Description);
            Assert.AreEqual(updatedIssue.Description, newValue);
            Assert.AreNotEqual(updatedIssue.Description, oldValue);
        }

        [TestMethod]
        public async Task TryPatchIssueDueDate_ReturnsIssueWithNewDueDate()
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

            await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.DueDate);
            Assert.AreEqual(updatedIssue.DueDate, newValue);
            Assert.AreNotEqual(updatedIssue.DueDate, oldValue);
        }

        [TestMethod]
        public async Task TryPatchIssueState_ReturnsIssueWithNewState()
        {
            var existingIssue = await service.TryGetById(existingIssueId);
            var oldValue = existingIssue.State;
            var newValue = existingIssue.State == Constants.STATE_OPENED ?
                Constants.STATE_COMPLETED : Constants.STATE_VERIFIED;

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

            await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

            var updatedIssue = await service.TryPatch(updatedValues);

            Assert.IsNotNull(updatedIssue);
            Assert.IsNotNull(updatedIssue.Id);
            Assert.IsNotNull(updatedIssue.State);
            Assert.AreEqual(updatedIssue.State, newValue);
            Assert.AreNotEqual(updatedIssue.State, oldValue);
        }

        [TestMethod]
        public async Task TryDeleteIssue_ReturnsTrue()
        {
            //var issue = new Issue()
            //{
            //    CreatedDate = DateTime.Now.ToUnixTime(),
            //    ParentId = "60b4d2719fbb9657cf2e0cbf",
            //    ParentType = Constants.ELEMENT_TYPE,
            //    State = "opened",
            //    Type = Constants.ISSUE_TYPE,
            //    Description = "Test description",
            //    Title = "Test title",
            //};

            //var result = await service.TryDelete(issue);

            //Assert.IsNotNull(result);
            //Assert.IsNotNull(result.Id);
        }
    }
}
