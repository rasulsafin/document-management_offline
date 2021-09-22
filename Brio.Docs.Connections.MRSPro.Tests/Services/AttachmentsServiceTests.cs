using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Client.Dtos;
using System;
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
        [DataRow(
            "C:\\Users\\Admin\\Downloads\\Big_Flopa.jpg",
            "61024880203f664081129a8c",
            "Big_Flopa.jpg",
            "60f178ef0049c040b8e7c584")]
        public async Task TryUploadAttachmentAsync_UploadingAttachments_ReturnsTrue(string path, string id, string originalName, string parentId)
        {
            var file = File.ReadAllBytes(path);
            var attachment = new PhotoAttachmentData()
            {
                File = file.ToString(),
            };

            var result = await service.TryUploadAttachmentAsync(attachment, id, originalName, parentId, file);

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
