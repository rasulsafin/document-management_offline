using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Tests
{
    [TestClass]
    public class IssuesTest
    {
        private static readonly string TEST_FILE_PATH = "My Test Folder/123.txt";
        private static readonly string TEST_PROJECT_NAME = "Sample Project";
        private static readonly string TEST_ISSUE_NAME = "Integration test";

        private static IssuesService issuesService;
        private static string issuesContainer;
        private static Random random = new Random();

        [ClassInitialize]
        public static void Initialize(TestContext _)
        {
            var connection = new ForgeConnection();
            var authService = new AuthenticationService(connection);
            var authenticator = new Authenticator(authService);
            var hubsService = new HubsService(connection);
            var projectsService = new ProjectsService(connection);
            issuesService = new IssuesService(connection);

            var connectionInfo = new ConnectionInfoDto
            {
                ID = new ID<ConnectionInfoDto>(2),
                ConnectionType = new ConnectionTypeDto
                {
                    AppProperty = new Dictionary<string, string>
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
                    ID = new ID<ConnectionTypeDto>(2),
                    Name = "BIM360",
                },
            };

            // Authorize
            var signInTask = authenticator.SignInAsync(connectionInfo);
            signInTask.Wait();
            if (signInTask.Result.authStatus.Status != RemoteConnectionStatusDto.OK)
                Assert.Fail("Authorization failed");

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hubsTask = hubsService.GetHubsAsync();
            hubsTask.Wait();
            var hub = hubsTask.Result.FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose project
            var projectsTask = projectsService.GetProjectsAsync(hub.ID);
            projectsTask.Wait();
            var project = projectsTask.Result.FirstOrDefault(x => x.Attributes.Name == TEST_PROJECT_NAME);
            if (project == default)
                Assert.Fail("Testing project doesn't exist");

            issuesContainer = project.Relationships.IssuesContainer.Data.ID;
        }

        [TestMethod]
        public async Task CanGetIssues()
        {
            var issues = await issuesService.GetIssuesAsync(issuesContainer);

            if (issues == null)
                Assert.Fail();
        }

        [TestMethod]
        public async Task CanPatchIssue()
        {
            var issues = await issuesService.GetIssuesAsync(issuesContainer);

            if (issues == null || issues.Count == 0)
                Assert.Fail("Testing issue hasn't got issues");

            var issue = issues.FirstOrDefault(x => x.Attributes.Title == TEST_ISSUE_NAME);

            if (issue == default)
                Assert.Fail("Testing issue doesn't exist");

            issue.Attributes.Description =
                    uint.TryParse(issue.Attributes.Description, out var number) || number == uint.MaxValue
                            ? 0.ToString()
                            : (++number).ToString();

            issue.Attributes.DueDate = null;

            var changedDescription = issue.Attributes.Description;
            issue = await issuesService.PatchIssueAsync(issuesContainer, issue);

            if (issue.Attributes.Description != changedDescription)
                Assert.Fail("Pathing doesn't work");
        }
    }
}
