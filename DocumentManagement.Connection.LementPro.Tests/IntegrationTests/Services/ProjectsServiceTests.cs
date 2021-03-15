using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class ProjectsServiceTests
    {
        private static ProjectsService service;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var requestUtility = new HttpRequestUtility(new HttpConnection());
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);
            service = new ProjectsService(requestUtility, commonRequests);

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

            var (_, _) = await authService.SignInAsync(connectionInfo);
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => service.Dispose();

        [TestMethod]
        public async Task GetAllProjectsAsync_ProjectsDefaultFolderNotEmpty_ReturnsProjectsList()
        {
            var result = await service.GetAllProjectsAsync();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetProjectAsync_ExistingProject_ReturnsProject()
        {
            var taskId = 402014;

            var result = await service.GetProjectAsync(taskId);

            Assert.IsNotNull(result);
        }
    }
}
