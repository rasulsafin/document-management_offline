using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class IssuesServiceTests
    {
        private static IssuesService service;
        private static ServiceProvider serviceProvider;

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
            if (signInTask.Result.authStatus.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");
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

        //[TestMethod]
        //public async Task TryAddIssue_ReturnsAddedIssue()
        //{
        //    var Issue = new Issue()
        //    {
        //        CreatedDate = 1623242719575,
        //        Owner = "60b5c2d69fbb9657cf2e0d5f",
        //        ParentId = "60be1809d90f8f6dc96f8345",
        //        ParentType = "project",
        //        ProjectId = "60be1809d90f8f6dc96f8345",
        //        State = "opened",
        //        Type = "task",
        //        Description = "Test description",
        //        Title = "Test title",
        //    };

        //    var result = await service.PostObjective(objective);

        //    Assert.IsNotNull(result);
        //}
    }
}
