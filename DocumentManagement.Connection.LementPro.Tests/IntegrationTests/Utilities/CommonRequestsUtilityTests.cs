using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
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
            var requestsUtility = new HttpRequestUtility(new HttpConnection());
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
        public async Task GetMenuCategories_WithCorrectCredentials_ReturnsTask()
        {
            var result = await subject.Object.GetMenuCategoriesAsync();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetFoldersTree_ExistingCategoryWithCorrectCredentials_ReturnsFoldersTree()
        {
            var categoryObject = "Task";
            var categoryId = 40164;

            var result = await subject.Object.GetFoldersTreeAsync(categoryObject, categoryId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetListTask_ExistingFolderWithCorrectCredentials_ReturnsTasksList()
        {
            var objectType = "Task";
            string folderKey = "{\"id\":127472,\"subKeys\":[]}";

            var result = await subject.Object.GetObjectsListFromFolderAsync(objectType, folderKey);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }
    }
}
