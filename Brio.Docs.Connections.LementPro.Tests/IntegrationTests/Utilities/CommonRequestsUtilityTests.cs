using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Connections.LementPro.Utilities;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.LementPro.Tests.IntegrationTests.Utilities
{
    [TestClass]
    public class CommonRequestsUtilityTests
    {
        private static List<int> objectsIdsToDelete = new List<int>();
        private static ServiceProvider serviceProvider;
        private static CommonRequestsUtilityTester utility;

        [ClassInitialize]
        public static async Task Initialize(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddLementPro();
            services.Remove(
                new ServiceDescriptor(
                    typeof(CommonRequestsUtility),
                    typeof(CommonRequestsUtility),
                    ServiceLifetime.Scoped));
            services.AddScoped<CommonRequestsUtility, CommonRequestsUtilityTester>();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            utility = (CommonRequestsUtilityTester)serviceProvider.GetService<CommonRequestsUtility>();
            var connection = serviceProvider.GetService<LementProConnection>();

            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            await connection!.Connect(connectionInfo, CancellationToken.None);
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            foreach (var id in objectsIdsToDelete)
            {
                var deleteResult = await utility.DeleteObjectAsync(id);
                if (!deleteResult.Any())
                {
                    await Task.Delay(5000);
                    await utility.DeleteObjectAsync(id);
                }
            }

            await serviceProvider.DisposeAsync();
        }

        [TestMethod]
        public async Task GetMenuCategories_WithCorrectCredentials_ReturnsTask()
        {
            var result = await utility.GetMenuCategoriesAsync();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetFoldersTree_ExistingCategoryWithCorrectCredentials_ReturnsFoldersTree()
        {
            var categoryObject = "Task";
            var categoryId = 40164;

            var result = await utility.GetFoldersTreeAsync(categoryObject, categoryId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetListTask_ExistingFolderWithCorrectCredentials_ReturnsTasksList()
        {
            var objectType = "Task";
            var folderKey = "{\"id\":127472,\"subKeys\":[]}";

            var result = await utility.GetObjectsListFromFolderAsync(objectType, folderKey);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetTypesAttributesDefinitionAsync_BimAttributesWithCorrectCredentials_ReturnsBimAttributesSubtypes()
        {
            var typeId = "40163";

            var result = await utility.GetTypesAttributesDefinitionAsync(typeId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task UploadFileAsync_SmallFileLess2MB_UploadedSuccessful()
        {
            var bimTypeId = 40170;
            var filePath = "C:\\Users\\diismagilov\\Downloads\\HelloWallIfc4.ifc";
            var name = Path.GetFileName(filePath);

            var result = await utility.UploadFileAsync(bimTypeId, name, filePath);

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

            var result = await utility.UploadFileAsync(bimTypeId, name, filePath);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess.Value);
            objectsIdsToDelete.Add(result.ID.Value);
        }

        [TestMethod]
        public async Task GetDefaultTemplate_BimCategoryAndBimType_ReturnsNotEmptyJToken()
        {
            var bimCategory = 40163;
            var bimTypeId = 40170;

            var result = await utility.GetDefaultTemplate(bimCategory, bimTypeId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DeleteObjectAsync_JustUploadedFile_ReturnsNotEmptyList()
        {
            var bimTypeId = 40170;
            var filePath = "C:\\Users\\diismagilov\\Downloads\\HelloWallIfc4.ifc";
            var name = Path.GetFileName(filePath);
            var uploaded = await utility.UploadFileAsync(bimTypeId, name, filePath);
            var bimId = uploaded.ID.Value;

            // Wait for creating (5 sec is enough usually)
            await Task.Delay(5000);

            var result = await utility.DeleteObjectAsync(bimId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }
    }
}
