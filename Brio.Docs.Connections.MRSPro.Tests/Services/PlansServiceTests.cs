using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Common.Dtos;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brio.Docs.Connections.MrsPro.Tests.TestConstants;

namespace Brio.Docs.Connections.MrsPro.Tests.Services
{
    [TestClass]
    public class PlansServiceTests
    {
        private static readonly string PARENT_ID = "/5ebb7cb7782f96000146e7f3:ORGANIZATION/60b4d2719fbb9657cf2e0cbf:PROJECT";
        private static readonly string PROJECT_ID = "60b4d2719fbb9657cf2e0cbf";
        private static PlansService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<PlansService>();
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
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
        => await Task.Delay(MILLISECONDS_TIME_DELAY);

        [TestMethod]
        public async Task TryGetByParentIdAsync_ExistingParentId_ReturnsListOfPlans()
        {
            var plans = await service.TryGetByParentIdAsync(PARENT_ID);

            Assert.IsNotNull(plans);
            Assert.IsTrue(plans.Any());
        }

        [TestMethod]
        public async Task TryPostAsync_ExistingPlanToExistingFolder_ReturnsListOfPlans()
        {
            //var filePath = @"C:\Users\yigurieva\Desktop\image.png";
            //var plan = await service.TryPost(filePath);

            //Assert.IsNotNull(plan);
        }

        [TestMethod]
        public async Task TryGetByProjectIdAsync_ExistingProject_ReturnsListOfPlans()
        {
            var plans = await service.TryGetByProjectIdAsync(PROJECT_ID);

            Assert.IsNotNull(plans);
            Assert.IsTrue(plans.Any());
        }

        [TestMethod]
        [DataRow(
            "60f1750a546732672f28eed9",
            "60b4d2719fbb9657cf2e0cbf")]
        public async Task GetPlanUriAsync_GetingPlanUri_ReturnsPlanUri(string id,
            string parentId)
        {
            var uri = await service.GetUriAsync(id, parentId);

            Assert.IsNotNull(uri);
        }

        [TestMethod]
        [DataRow(
            "C:\\Users\\Admin\\Downloads\\Big_Flopa.jpg",
            "Big_Flopa.jpg.jpg",
            "60fabf44bcc3334b8b9377a6")]
        public async Task TryUploadPlanAsync_UploadingPlan_ReturnsTrue(string path, string originalName, string parentId)
        {
            var file = File.ReadAllBytes(path);
            var plan = new Plan()
            {
                OriginalFileName = originalName,
                ParentId = parentId,
            };

            var result = await service.TryUploadAsync(plan, originalName, file, parentId);

            Assert.IsTrue(result);
        }
    }
}
