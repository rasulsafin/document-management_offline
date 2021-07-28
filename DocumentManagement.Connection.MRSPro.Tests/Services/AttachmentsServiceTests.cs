using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;
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
        public async Task TryGetExistingAttachmentsByIdsAsync_GettingExistingAttachments_ReturnsAttachmentsByIdsList()
        {
            var projects = await service.GetAllAsync(DateTime.MinValue);
            var existingIds = projects.Take(5).Select(p => p.Id).ToList();
            var result = await service.TryGetByIdsAsync(existingIds);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [DataRow("60b5c2d69fbb9657cf2e0d5f")]
        public async Task TryGetExistingAttachmentsByOwnerIdAsync_GettingExistingAttachments_ReturnsAttachmentsByOwnerIdList(string id)
        {
            var attachments = await service.GetByOwnerIdAsync(id);

            Assert.IsNotNull(attachments);
        }

        [TestMethod]
        [DataRow("60ed826800fac340ae7049fe")]
        public async Task TryGetExistingAttachmentsByParentIdAsync_GettingAttachments_ReturnsAttachmentsByParentIdList(string id)
        {
            var attachments = await service.GetByParentIdAsync(id);

            Assert.IsNotNull(attachments);
        }

        [TestMethod]
        [DataRow("60f1790939c96a40a149d543")]
        public async Task TryDownloadAttachmentByIdAsync_DownloadingAttachment_ReturnsAttachment(string id)
        {
            var attachments = await service.TryDownloadAttachmentByIdAsync(id);

            Assert.IsNotNull(attachments);
        }

        [TestMethod]
        [DataRow(
            "C:\\Users\\Admin\\Downloads\\Большой_Шлёпа.jpg",
            "60f1790939c96a40a149d556",
            "%D0%91%D0%BE%D0%BB%D1%8C%D1%88%D0%BE%D0%B9_%D0%A8%D0%BB%D1%91%D0%BF%D0%B0.jpg",
            "60f178ef0049c040b8e7c584",
            "task")]
        public async Task TryUploadAttachmentAsync_UploadingAttachments_ReturnsTrue(string path, string id, string originalName, string parentId, string parentType)
        {
            var file = File.ReadAllBytes(path);
            var attachment = new PhotoAttachmentData()
            {
                File = file.ToString(),
            };

            var result = await service.TryUploadAttachmentAsync(attachment, id, originalName, parentId, parentType, file);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow("60f12d27546732672f28ede4")]
        public async Task TryDeleteAttachmentAsync_DeletingAttachment_ReturnsTrue(string id)
        {
            var result = await service.TryDeleteByIdAsync(id);

            Assert.IsTrue(result);
        }
    }
}
