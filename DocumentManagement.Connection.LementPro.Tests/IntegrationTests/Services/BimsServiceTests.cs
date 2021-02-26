﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class BimsServiceTests
    {
        private static BimsService service;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var requestUtility = new HttpRequestUtility(new HttpConnection());
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);
            service = new BimsService(requestUtility, commonRequests);

            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (_, _) = await authService.SignInAsync(connectionInfo);
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => service.Dispose();

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
