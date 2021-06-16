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
        private static string userId;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
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
            if (result.authStatus.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            userId = result.userId;
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
         => await Task.Delay(5000);

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
            var projects = await service.GetAll(DateTime.MinValue);
            var existingID = projects.First().Id;

            var result = await service.TryGetById(existingID);

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
        public async Task TryAddIssue_ReturnsAddedIssue()
        {
            var issue = new Issue()
            {
                CreatedDate = DateTime.Now.ToUnixTime(),
                Owner = userId,
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
    }
}
