using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.External.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudManagerTests
    {
        private const string VALID_USERNAME = "avsingaevskiy";
        private const string VALID_PASSWORD = "AndreyS186";

        private const string PATH = "D:\\TEST";

        private const string DOWNLOAD_SUBFOLDER = "BRIO PACH";
        private const string UPLOAD_SUBFOLDER = "TEST FILES";
        private const string DELETE_SUBFOLDER = "DELETE FILES";

        private static BrioCloudManager manager;
        private static IEnumerable<CloudElement> elements;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var controller = new BrioCloudController(VALID_USERNAME, VALID_PASSWORD);
            elements = await controller.GetListAsync($"/{DOWNLOAD_SUBFOLDER}");

            var directoryInfo = new DirectoryInfo(Path.Combine(PATH, DOWNLOAD_SUBFOLDER));

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            manager = new BrioCloudManager(controller);
        }

        [TestMethod]
        public async Task PullFile_FileExistsValidPath_True()
        {
            foreach (var element in elements)
            {
                var result = await manager.PullFile(element.Href, Path.Combine(PATH, DOWNLOAD_SUBFOLDER, element.DisplayName));

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task PullFile_FileNotExistsValidPath_False()
        {
            string href = "/абырвалг";
            string fileName = Path.Combine(PATH, DOWNLOAD_SUBFOLDER, "абырвалг");

            var result = await manager.PullFile(href, fileName);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public async Task PullFile_FileExistsInvalidPath_Error()
        {
            string nonExistentPath = "X:\\TEST";
            var element = elements.First();

            await manager.PullFile(element.Href, Path.Combine(nonExistentPath, element.DisplayName));
        }

        [TestMethod]
        public async Task PushFile_FileExistsValidPath_Success()
        {
            var files = Directory.GetFiles(Path.Combine(PATH, UPLOAD_SUBFOLDER));

            foreach (var file in files)
            {
                var result = await manager.PushFile($"{UPLOAD_SUBFOLDER}", file);

                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task PushFile_FileNotExistsValidPath_IsNull()
        {
            var file = "X:\\TEST\\абырвалг";

            var result = await manager.PushFile($"{UPLOAD_SUBFOLDER}", file);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task DeleteFile_FileExists_True()
        {
            var files = Directory.GetFiles(Path.Combine(PATH, DELETE_SUBFOLDER));

            foreach (var file in files)
            {
                string href = await manager.PushFile(DELETE_SUBFOLDER, file);
                var result = await manager.DeleteFile(href);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task DeleteFile_FileNotExists_False()
        {
            string href = "/абырвалг";

            var result = await manager.DeleteFile(href);

            Assert.IsFalse(result);
        }
    }
}
