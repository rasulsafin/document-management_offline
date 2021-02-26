using System.Collections.Generic;
using System.IO;
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
        private static List<int> objectsIdsToDelete = new List<int>();
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

            subject = new Mock<CommonRequestsUtilityTests> { CallBase = true };
            subject.Setup(p => p.RequestUtility).Returns(requestsUtility);
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            foreach (var id in objectsIdsToDelete)
            {
                var deleteResult = await subject.Object.DeleteObjectAsync(id);
                if (!deleteResult.Any())
                {
                    await Task.Delay(5000);
                    await subject.Object.DeleteObjectAsync(id);
                }
            }
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
            var folderKey = "{\"id\":127472,\"subKeys\":[]}";

            var result = await subject.Object.GetObjectsListFromFolderAsync(objectType, folderKey);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetTypesAttributesDefinitionAsync_BimAttributesWithCorrectCredentials_ReturnsBimAttributesSubtypes()
        {
            var typeId = "40163";

            var result = await subject.Object.GetTypesAttributesDefinitionAsync(typeId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task UploadFileAsync_SmallFileLess2MB_UploadedSuccessful()
        {
            var bimTypeId = 40170;
            var filePath = "C:\\Users\\diismagilov\\Downloads\\HelloWallIfc4.ifc";
            var name = Path.GetFileName(filePath);

            var result = await subject.Object.UploadFileAsync(bimTypeId, name, filePath);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess.Value);
            objectsIdsToDelete.Add(result.ID.Value);
        }

        [TestMethod]
        public async Task UploadFileAsync_LargeFileMoreThan2MB_UploadedSuccessful()
        {
            var bimTypeId = 40170;
            var filePath = "C:\\Users\\diismagilov\\Downloads\\00_Gladilova_AC_(IFC2x3)_05062020.ifc";
            var name = Path.GetFileName(filePath);

            var result = await subject.Object.UploadFileAsync(bimTypeId, name, filePath);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess.Value);
            objectsIdsToDelete.Add(result.ID.Value);
        }

        [TestMethod]
        public async Task GetDefaultTemplate_BimCategoryAndBimType_ReturnsNotEmptyJToken()
        {
            var bimCategory = 40163;
            var bimTypeId = 40170;

            var result = await subject.Object.GetDefaultTemplate(bimCategory, bimTypeId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DeleteObjectAsync_ExistingObject_ReturnsNotEmptyList()
        {
            var bimTypeId = 40170;
            var filePath = "C:\\Users\\diismagilov\\Downloads\\HelloWallIfc4.ifc";
            var name = Path.GetFileName(filePath);
            var uploaded = await subject.Object.UploadFileAsync(bimTypeId, name, filePath);
            var bimId = uploaded.ID.Value;

            // Wait for creating (5 sec is enough usually)
            await Task.Delay(5000);

            var result = await subject.Object.DeleteObjectAsync(bimId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }
    }
}
