using Brio.Docs.Connections.MrsPro.Services;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Brio.Docs.Connections.MrsPro.Constants;
using static Brio.Docs.Connections.MrsPro.Tests.TestConstants;

namespace Brio.Docs.Connections.MrsPro.Tests.Synchronization
{
    [TestClass]
    public class MrsProObjectivesSynchronizerTests
    {
        private static readonly string TEST_BIM_FILE_PATH = "Resources/HelloWallIfc4TEST.ifc";
        private static readonly string TEST_PNG_FILE_PATH = "Resources/TestIcon.png";
        private static readonly string TEST_TXT_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static readonly string PROJECT_ID = "60b4d2719fbb9657cf2e0cbf";
        private static ISynchronizer<ObjectiveExternalDto> synchronizer;
        private static ServiceProvider serviceProvider;
        private static ObjectiveExternalDto justAdded;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            var connection = serviceProvider.GetService<MrsProConnection>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = TEST_EMAIL;
            var password = TEST_PASSWORD;
            var companyCode = TEST_COMPANY;

            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { AUTH_EMAIL, email },
                    { AUTH_PASS, password },
                    { COMPANY_CODE, companyCode },
                },
            };

            var context = await connection.GetContext(connectionInfo);
            synchronizer = context.ObjectivesSynchronizer;

            var result = await connection!.Connect(connectionInfo, CancellationToken.None);
            if (result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
             => await Task.Delay(MILLISECONDS_TIME_DELAY);

        [TestCleanup]
        public async Task Cleanup()
        {
            if (justAdded == null)
                return;

            await Task.Delay(MILLISECONDS_TIME_DELAY);
            await synchronizer.Remove(justAdded);
        }

        [TestMethod]
        [DataRow("task", DisplayName = "ISSUE_TYPE")]
        [DataRow("project", DisplayName = "ELEMENT_TYPE")]
        public async Task AddAsync_ObjectiveWithoutParent_AddedSuccessfully(string typeValue)
        {
            var objective = new ObjectiveExternalDto
            {
                CreationDate = DateTime.Now,
                ProjectExternalID = PROJECT_ID,
                Status = ObjectiveStatus.Open,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = typeValue },
                Description = "Issue description",
                Title = "First type OPEN issue",
                DueDate = DateTime.Now.AddDays(5),
            };

            var result = await synchronizer.Add(objective);
            justAdded = result;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result?.ExternalID);
            Assert.IsNotNull(result?.Title);
            Assert.IsNotNull(result?.Description);
            Assert.IsNotNull(result?.DueDate);
            Assert.AreEqual(result?.Status, objective.Status);
        }

        [TestMethod]
        [DataRow("task", DisplayName = "ISSUE_TYPE")]
        [DataRow("project", DisplayName = "ELEMENT_TYPE")]
        public async Task AddAsync_ObjectiveWithElementTypeParent_AddedSuccessfully(string typeValue)
        {
            justAdded = await ArrangeObjective(ELEMENT_TYPE);
            await Task.Delay(MILLISECONDS_TIME_DELAY);

            var objective = new ObjectiveExternalDto
            {
                CreationDate = DateTime.Now,
                ProjectExternalID = PROJECT_ID,
                ParentObjectiveExternalID = justAdded.ExternalID,
                Status = ObjectiveStatus.Open,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = typeValue },
                Description = "Description",
                Title = $"Child objective ({typeValue})",
                DueDate = DateTime.Now.AddDays(5),
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result?.ExternalID);
            Assert.IsNotNull(result?.Title);
            Assert.IsNotNull(result?.Description);
            Assert.IsNotNull(result?.DueDate);
            Assert.AreEqual(result?.Status, objective.Status);
            Assert.IsNotNull(result?.ParentObjectiveExternalID);
            Assert.AreEqual(result?.ParentObjectiveExternalID, justAdded.ExternalID);
        }

        //[TestMethod]
        //public async Task Add_ObjectiveWithBimElement_AddedSuccessfully()
        //{
        //    justAdded = null;
        //}

        //[TestMethod]
        //public async Task Add_ObjectiveWithEmptyIdWithFiles_AddedSuccessfully()
        //{
        //    justAdded = null;
        //}

        [TestMethod]
        [DataRow("task", DisplayName = "ISSUE_TYPE")]
        [DataRow("project", DisplayName = "ELEMENT_TYPE")]
        public async Task RemoveAsync_JustAddedObjective_RemovedSuccessfully(string typeValue)
        {
            var objective = await ArrangeObjective(typeValue);

            var result = await synchronizer.Remove(objective);
            justAdded = null;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_JustAddedIssueObjective_UpdatedSuccessfully()
        {
            var added = await ArrangeObjective(ISSUE_TYPE);
            var title = added.Title;
            var description = added.Description;

            var newTitle = added.Title = $"UPDATED: {title}";
            var newDescription = added.Description = $"UPDATED: {description}";
            var newDueDate = added.DueDate = added.DueDate.AddDays(1);
            var newStatus = added.Status = ObjectiveStatus.Open;
            var result = await synchronizer.Update(added);
            justAdded = result;

            Assert.IsNotNull(result?.ExternalID);
            Assert.AreEqual(newTitle, result.Title);
            Assert.AreEqual(newDescription, result.Description);
            Assert.AreEqual(newDueDate, result.DueDate);
            Assert.AreEqual(newStatus, result.Status);
        }

        [TestMethod]
        public async Task UpdateAsync_JustAddedElementObjective_UpdatedSuccessfully()
        {
            var added = await ArrangeObjective(ELEMENT_TYPE);
            var title = added.Title;

            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);
            justAdded = result;

            Assert.IsNotNull(result?.ExternalID);
            Assert.AreEqual(newTitle, result.Title);
        }

        //[TestMethod]
        //public async Task Update_JustAddedObjectiveWithItems_UpdatedSuccessfully()
        //{
        //    justAdded = null;
        //}

        [TestMethod]
        [DataRow("task", DisplayName = "ISSUE_TYPE")]
        [DataRow("project", DisplayName = "ELEMENT_TYPE")]
        public async Task GetUpdatedIDsAsync_AtLeastOneObjectiveAdded_RetrievedSuccessfully(string typeValue)
        {
            var added = await ArrangeObjective(typeValue);
            justAdded = added;

            var result = await synchronizer.GetUpdatedIDs(DateTime.Now.AddDays(-1));
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Any(x => x == added.ExternalID) == true);
        }

        [TestMethod]
        [DataRow("task", DisplayName = "ISSUE_TYPE")]
        [DataRow("project", DisplayName = "ELEMENT_TYPE")]
        public async Task GetAsync_ExistingObjectiveById_RetrievedSuccessfully(string typeValue)
        {
            var added = await ArrangeObjective(typeValue);
            justAdded = added;

            var result = await synchronizer.Get(new[] { added.ExternalID });
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Any(x => x.ExternalID == added.ExternalID) == true);
        }

        [TestMethod]
        public async Task GetAsync_ExistingObjectivesWithFiles_RetrievedSuccessfully()
        {
            var projectElementId = "/5ebb7cb7782f96000146e7f3:ORGANIZATION/60b4d2719fbb9657cf2e0cbf:PROJECT"; // Project with items
            var issueId = "/5ebb7cb7782f96000146e7f3:ORGANIZATION/60b4d2719fbb9657cf2e0cbf:PROJECT/60f178ef0049c040b8e7c584:TASK"; // Issue with items
            var result = await synchronizer.Get(new[] { issueId, projectElementId });
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Any(x => x.ExternalID == projectElementId && x.ObjectiveType.ExternalId == ELEMENT_TYPE && x.Items?.Count > 0) == true);
            Assert.IsTrue(result?.Any(x => x.ExternalID == issueId && x.ObjectiveType.ExternalId == ISSUE_TYPE && x.Items?.Count > 0) == true);
        }

        private async Task<ObjectiveExternalDto> ArrangeObjective(string typeValue)
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = typeValue },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "Title",
                Description = "Description",
                Status = ObjectiveStatus.Open,
                ProjectExternalID = PROJECT_ID,
            };

            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            return added;
        }
    }
}
