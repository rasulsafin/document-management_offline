using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Integration.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudControllerTests
    {
        private const string VALID_USERNAME = "avsingaevskiy";
        private const string VALID_PASSWORD = "AndreyS186";

        private static BrioCloudController controller;

        [ClassInitialize]
        public static void ClassInitialize(TestContext unused)
        {
            controller = new BrioCloudController(VALID_USERNAME, VALID_PASSWORD);
        }

        [TestMethod]
        [DataRow("/")]
        [DataRow("/BRIO PACH")]
        public async Task GetListAsync_ValidPath_IsNotNull(string path)
        {
            var result = await controller.GetListAsync(path);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [DataRow("абырвалг")]
        [ExpectedException(typeof(WebException))]
        public async Task GetListAsync_InvalidPath_IsNotNull(string path)
        {
            var result = await controller.GetListAsync(path);

            Assert.IsNotNull(result);
        }
    }
}
