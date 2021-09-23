using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Tests
{
    [TestClass]
    public class LocationsTests
    {
        private static Project project;
        private static LocationService locationService;
        private static string issuesContainer;
        private static ForgeConnection connection;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddBim360();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();

            connection = serviceProvider.GetService<ForgeConnection>();
            var authenticator = serviceProvider.GetService<Authenticator>();
            var hubsService = serviceProvider.GetService<HubsService>();
            var projectsService = serviceProvider.GetService<ProjectsService>();
            locationService = serviceProvider.GetService<LocationService>();

            if (authenticator == null || hubsService == null || connection == null || projectsService == null)
                throw new Exception("Required services are null");

            var connectionInfo = new ConnectionInfoExternalDto
            {
                ConnectionType = new ConnectionTypeExternalDto
                {
                    AppProperties = new Dictionary<string, string>
                    {
                        { "CLIENT_ID", "m5fLEAiDRlW3G7vgnkGGGcg4AABM7hCf" },
                        { "CLIENT_SECRET", "dEGEHfbl9LWmEnd7" },
                        { "RETURN_URL", "http://localhost:8000/oauth/" },
                    },
                    AuthFieldNames = new List<string>
                    {
                        "token",
                        "refreshtoken",
                        "end",
                    },
                    Name = "BIM360",
                },
            };

            // Authorize
            var signInTask = authenticator.SignInAsync(connectionInfo);
            signInTask.Wait();
            if (signInTask.Result.authStatus.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            connection.GetToken = () => connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hubsTask = hubsService.GetHubsAsync();
            hubsTask.Wait();
            var hub = hubsTask.Result.FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose project
            var projectsTask = projectsService.GetProjectsAsync(hub.ID);
            projectsTask.Wait();
            project = projectsTask.Result.FirstOrDefault(x => x.Attributes.Name == INTEGRATION_TEST_PROJECT);
            if (project == default)
                Assert.Fail("Testing project doesn't exist");

            issuesContainer = project.Relationships.IssuesContainer.Data.ID;
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
            => await Task.Delay(5000);

        [TestMethod]
        public async Task GetLocations_HaveAccessToIssueContainer_ReturnsIssuesList()
        {
            var locations = await locationService.GetLocationsAsync(issuesContainer, "default");

            if (locations == null)
                Assert.Fail();
        }
    }
}
