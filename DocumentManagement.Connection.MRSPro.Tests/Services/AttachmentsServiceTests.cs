using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;
using static MRS.DocumentManagement.Connection.MrsPro.Tests.TestConstants;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class AttachmentsServiceTests
    {
        private static AttachmentsService service;
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
            service = serviceProvider.GetService<AttachmentsService>();
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
        public async Task TryGetExistingAttachmentsByIdsAsync_ReturnsAttachmentsByIdsList()
        {
            var projects = await service.GetAll(DateTime.MinValue);
            var existingIds = projects.Take(5).Select(p => p.Id).ToList();
            var result = await service.TryGetByIds(existingIds);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [DataRow("60b5c2d69fbb9657cf2e0d5f")]
        public async Task TryGetExistingAttachmentsByOwnerIdAsync_ReturnsAttachmentsByOwnerIdList(string id)
        {
            var attachments = await service.GetByOwnerId(id);

            Assert.IsNotNull(attachments);
        }

        [TestMethod]
        [DataRow("60ed826800fac340ae7049fe")]
        public async Task TryGetExistingAttachmentsByParentIdAsync_ReturnsAttachmentsByParentIdList(string id)
        {
            var attachments = await service.GetByParentId(id);

            Assert.IsNotNull(attachments);
        }

        [TestMethod]
        [DataRow("60eef9c539aeb14087ace51c")]
        public async Task TryDownloadAttachmentById_ReturnsAttachment(string id)
        {
            var attachments = await service.DownloadAttachmentById(id);

            Assert.IsNotNull(attachments);
        }

        [TestMethod]
        [DataRow("C:\\repos\\document-management\\DocumentManagement.Connection.MRSPro.Tests\\bin\\Debug\\net5.0\\Downloads",
            "kotik.png",
            "60ed826800fac340ae7049fe")]
        public async Task TryUploadAttachment_ReturnsBool(string pathToDir, string filename, string parentId)
        {
            var file = File.ReadAllBytes($"{pathToDir}\\{filename}");
            HttpContent bytesContent = new ByteArrayContent(file);
            var form = new MultipartFormDataContent();
            form.Add(bytesContent, "file");

            var attachment = new Attachment()
            {
                Latitude = 0,
                Longitude = 0,
                OriginalFileName = filename,
                ParentId = parentId,
                ParentType = ISSUE_TYPE,
                //UrlToFile = $"{pathToDir}\\{filename}",
            };

            var result = await service.UploadAttachment(attachment);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow(
            "C:\\Users\\Admin\\Downloads\\Большой_Шлёпа.jpg",
            "60ed826800fac340ae7049fe")]
        public async Task TryPutFile_ReturnsBool(string path, string parentId)
        {
            var file = File.ReadAllBytes(path);
            var attachment = new Attachment()
            {
                Latitude = 0,
                Longitude = 0,
                OriginalFileName = " ",
                ParentId = parentId,
                ParentType = ISSUE_TYPE,
                PhotoAttachment = file,
            };
            var result = await service.TryPutFile(attachment);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow("60f12d27546732672f28ede4")]
        public async Task TryDeleteAttachment_ReturnsBool(string id)
        {
            var result = await service.TryDeleteById(id);

            Assert.IsTrue(result);
        }
    }
}
