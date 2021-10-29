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
    public class IssuesTests
    {
        private static readonly string TEST_ISSUE_ID = "83188de8-8fce-4eba-8b02-8a0f4aca276f";
        private static readonly Random RANDOM = new Random();

        private static Project project;
        private static IssuesService issuesService;
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
            issuesService = serviceProvider.GetService<IssuesService>();

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
                Attributes = new Issue.IssueAttributes
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
                    root.ID);

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
                Attributes = new Issue.IssueAttributes
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

        [TestMethod]
        public async Task GetComments_HaveAccessToTestingIssue_ReturnsCommentsList()
        {
            var comments = await issuesService.GetCommentsAsync(issuesContainer, TEST_ISSUE_ID);
            Assert.IsNotNull(comments);
            Assert.AreEqual(3, comments.Count);
        }
    }
}
