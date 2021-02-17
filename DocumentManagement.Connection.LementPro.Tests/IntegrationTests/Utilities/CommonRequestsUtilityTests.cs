using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Utilities
{
    [TestClass]
    public class CommonRequestsUtilityTests : CommonRequestsUtility
    {
        private static string token;
        private static Mock<CommonRequestsUtilityTests> subject;

        [ClassInitialize]
        public static async Task Initialize(TestContext unused)
        {
            var requestsUtility = new HttpRequestUtility(new NetConnector());
            var service = new AuthenticationService(requestsUtility);

            var connectionInfo = new ConnectionInfoDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", "diismagilov" },
                    { "password", "DYZDFMwZ" },
                },
            };

            var (_, updatedInfo) = await service.SignInAsync(connectionInfo);
            token = updatedInfo.AuthFieldValues["token"];

            subject = new Mock<CommonRequestsUtilityTests> { CallBase = true };
            subject.Setup(p => p.RequestUtility).Returns(requestsUtility);
        }

        [TestMethod]
        public async Task MyTestMethod()
        {
            var result = await subject.Object.GetMenuCategoriesAsync(token);

            Assert.IsNotNull(result);
        }
    }
}
