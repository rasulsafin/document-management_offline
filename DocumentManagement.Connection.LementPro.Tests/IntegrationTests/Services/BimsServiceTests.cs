using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Interface.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class BimsServiceTests
    {
        private static BimsService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddLementPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<BimsService>();
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
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestMethod]
        public async Task GetAllBimFilesAsync_BimDefaultFolderNotEmpty_ReturnsBimFilesList()
        {
            var result = await service.GetAllBimFilesAsync();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetBimAttributesAsync_ExistingBimElementAndFolder_ReturnsBimsAttributesList()
        {
            var folderId = "127482";
            var bimId = "402297";

            var result = await service.GetBimAttributesAsync(bimId, folderId);

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.Any(a => a.ParentId == bimId));
        }

        [TestMethod]
        public async Task GetBimsChildObjectsByAttributeAsync_ExistingBimElement_ReturnsBimFikeChildren()
        {
            var folderKey = "{\"id\":127482,\"subKeys\":[]}";
            var bimId = "402297";
            var attributeId = "59218";

            var result = await service.GetBimsChildObjectsByAttributeAsync(bimId, folderKey, attributeId);

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task DownloadBimsLastVersion_Bim402297_DownloadAndSaveBimToThePath()
        {
            string path = "C:\\Users\\diismagilov\\Downloads\\";
            string bimId = "402297";

            var result = await service.DownloadLastVersionAsync(bimId, path);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateBimVersion_SmallFile_UpdatedSuccessful()
        {
            // Update is not working for the moment

            //string path = "C:\\Users\\diismagilov\\Downloads\\HelloWallIfc4.ifc";
            //string bimId = "436782";

            //var result = await service.UpdateBimVersion(bimId, path);

            //Assert.IsNotNull(result?.IsSuccess);
            //Assert.IsTrue(result.IsSuccess.Value);
        }

        [TestMethod]
        public async Task UpdateBimVersion_LargeFile_UpdatedSuccessful()
        {
            // Update is not working for the moment

            //string path = "C:\\Users\\diismagilov\\Downloads\\00_Gladilova_AC_(IFC2x3)_05062020.ifc";
            //string bimId = "436770";

            //var result = await service.UpdateBimVersion(bimId, path);

            //Assert.IsNotNull(result?.IsSuccess);
            //Assert.IsTrue(result.IsSuccess.Value);
        }
    }
}
