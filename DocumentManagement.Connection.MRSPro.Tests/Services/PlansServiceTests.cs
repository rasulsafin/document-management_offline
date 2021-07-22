using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Tests.TestConstants;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
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
            var plans = await service.TryGetByParentId(PARENT_ID);

            Assert.IsNotNull(plans);
            Assert.IsTrue(plans.Any());
        }

        [TestMethod]
        public async Task TryPost_ExistingPlanToExistingFolderAsync_ReturnsListOfPlans()
        {
            //var filePath = @"C:\Users\yigurieva\Desktop\image.png";
            //var plan = await service.TryPost(filePath);

            //Assert.IsNotNull(plan);
        }

        [TestMethod]
        public async Task TryGetByProjectId_ExistingProject_ReturnsListOfPlans()
        {
            var plans = await service.TryGetByProjectId(PROJECT_ID);

            Assert.IsNotNull(plans);
            Assert.IsTrue(plans.Any());
        }
    }
}
