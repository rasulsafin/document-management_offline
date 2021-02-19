using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Models.Issue;

namespace MRS.DocumentManagement.Connection.Bim360.Tests
{
    [TestClass]
    public class IssuesTests
    {
        private static readonly string TEST_PROJECT_NAME = "Sample Project";
        private static readonly string TEST_ISSUE_ID = "da04f18d-b8e8-407b-983e-385f1a0520ea";
        private static readonly Random RANDOM = new Random();

        private static Project project;
        private static IssuesService issuesService;
        private static string issuesContainer;
        private static ForgeConnection connection;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            connection = new ForgeConnection();
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
            project = projectsTask.Result.FirstOrDefault(x => x.Attributes.Name == TEST_PROJECT_NAME);
            if (project == default)
                Assert.Fail("Testing project doesn't exist");

            issuesContainer = project.Relationships.IssuesContainer.Data.ID;
        }

        [TestMethod]
        public async Task GetIssues_HaveAccessToIssueContainer_ReturnsIssuesList()
        {
            var issues = await issuesService.GetIssuesAsync(issuesContainer);

            if (issues == null)
                Assert.Fail();
        }

        [TestMethod]
        public async Task PatchTestingIssue_HaveAccessToTestingIssue_PatchedPropertyChanged()
        {
            var issues = await issuesService.GetIssuesAsync(issuesContainer);

            if (issues == null || issues.Count == 0)
                Assert.Fail("Testing issue hasn't got issues");

            var issue = issues.FirstOrDefault(x => x.ID == TEST_ISSUE_ID);

            if (issue == default)
                Assert.Fail("Testing issue doesn't exist");

            issue.Attributes.Description =
                    !uint.TryParse(issue.Attributes.Description, out var number) || number == uint.MaxValue
                            ? 0.ToString()
                            : (++number).ToString();

            var changedDescription = issue.Attributes.Description;
            issue = await issuesService.PatchIssueAsync(issuesContainer, issue);

            if (issue.Attributes.Description != changedDescription)
                Assert.Fail("Pathing doesn't work");
        }

        [TestMethod]
        public async Task GetIssuesTypes_HaveAccessToIssueContainer_ReturnsTypesList()
        {
            var types = await issuesService.GetIssueTypesAsync(issuesContainer);

            Assert.IsNotNull(types);
            Assert.IsFalse(types.Count == 0, "Issue types are empty");
        }

        [TestMethod]
        public async Task GetIssuesSubtypes_HaveAccessToIssueContainer_SubtypesNotEmpty()
        {
            var types = await issuesService.GetIssueTypesAsync(issuesContainer);

            Assert.IsNotNull(types);
            Assert.IsFalse(types.Count == 0, "Issue types are empty");
            Assert.IsNotNull(types[0].Subtypes);
            Assert.IsFalse(types[0].Subtypes.Length == 0, "Issue subtypes are empty");
        }

        [TestMethod]
        public async Task PostIssue_HaveAccessToIssueContainer_Success()
        {
            var types = await issuesService.GetIssueTypesAsync(issuesContainer);

            Assert.IsNotNull(types);
            Assert.IsFalse(types.Count == 0, "Issue types are empty");
            Assert.IsNotNull(types[0].Subtypes);
            Assert.IsFalse(types[0].Subtypes.Length == 0, "Issue subtypes are empty");

            var title = "Integration Post Test";
            var issue = new Issue
            {
                Attributes = new IssueAttributes
                {
                    Title = title,
                    NgIssueTypeID = types[0].ID,
                    NgIssueSubtypeID = types[0].Subtypes[0].ID,
                },
            };
            issue = await issuesService.PostIssueAsync(issuesContainer, issue);

            Assert.IsNotNull(issue, "Response is empty");
            Assert.IsFalse(string.IsNullOrEmpty(issue.ID), "Response issue has no id");
            Assert.AreEqual(title, issue.Attributes.Title);
        }

        [TestMethod]
        public async Task PostIssueAndAttachmentRandomFile_HaveFileAtBim360AndAccessToIssueContainer_Success()
        {
            var foldersService = new FoldersService(connection);

            // Step 3: Find the resource item in a project.
            var root = project.Relationships.RootFolder.Data;
            Assert.IsNotNull(root, "Can't take root folder");

            var files = await foldersService.SearchAsync(project.ID,
                    root.ID,
                    Array.Empty<(string filteringField, string filteringValue)>());

            Assert.IsNotNull(files);
            Assert.IsFalse(files.Count == 0, "Files are empty");

            var file = files[RANDOM.Next(files.Count)];

            var types = await issuesService.GetIssueTypesAsync(issuesContainer);

            Assert.IsNotNull(types);
            Assert.IsFalse(types.Count == 0, "Issue types are empty");
            Assert.IsNotNull(types[0].Subtypes);
            Assert.IsFalse(types[0].Subtypes.Length == 0, "Issue subtypes are empty");

            var title = "Integration Post Attachment Test";
            var issue = new Issue
            {
                Attributes = new IssueAttributes
                {
                    Title = title,
                    NgIssueTypeID = types[0].ID,
                    NgIssueSubtypeID = types[0].Subtypes[0].ID,
                },
            };
            issue = await issuesService.PostIssueAsync(issuesContainer, issue);

            Assert.IsNotNull(issue, "Response is empty");
            Assert.IsFalse(string.IsNullOrEmpty(issue.ID), "Response issue has no id");
            Assert.AreEqual(title, issue.Attributes.Title);

            var attachment = new Attachment
            {
                Attributes = new Attachment.AttachmentAttributes
                {
                    Name = file.item.Attributes.DisplayName,
                    IssueId = issue.ID,
                    Urn = file.item.ID,
                },
            };

            attachment = await issuesService.PostIssuesAttachmentsAsync(issuesContainer, attachment);
            Assert.IsNotNull(attachment, "Response is empty");
            Assert.IsFalse(string.IsNullOrEmpty(attachment.ID), "Response attachment has no id");
        }

        [TestMethod]
        public async Task GetAttachments_HaveAccessToTestingIssue_ReturnsAttachmentsList()
        {
            var attachments = await issuesService.GetAttachmentsAsync(issuesContainer, TEST_ISSUE_ID);
            Assert.IsNotNull(attachments);
        }
    }
}
