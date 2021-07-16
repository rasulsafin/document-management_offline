using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;
using static MRS.DocumentManagement.Connection.MrsPro.Tests.TestConstants;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Synchronization
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

            var result = await connection!.Connect(connectionInfo, CancellationToken.None);
            if (result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            var context = await connection.GetContext(connectionInfo);
            synchronizer = context.ObjectivesSynchronizer;
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
        public async Task Add_ObjectiveWithoutParent_AddedSuccessfully(string typeValue)
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
        public async Task Remove_JustAddedObjective_RemovedSuccessfully(string typeValue)
        {
            var objective = await ArrangeObjective(typeValue);

            var result = await synchronizer.Remove(objective);
            justAdded = null;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Update_JustAddedIssueObjective_UpdatedSuccessfully(string typeValue)
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
        public async Task Update_JustAddedElementObjective_UpdatedSuccessfully(string typeValue)
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
        public async Task GetUpdatedIDs_AtLeastOneObjectiveAdded_RetrivedSuccessful()
        {
            var added = await ArrangeObjective(ISSUE_TYPE);
            justAdded = added;

            var result = await synchronizer.GetUpdatedIDs(DateTime.Now.AddDays(-1));
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Any(x => x == added.ExternalID) == true);
        }

        [TestMethod]
        [DataRow("task", DisplayName = "ISSUE_TYPE")]
        [DataRow("project", DisplayName = "ELEMENT_TYPE")]
        public async Task Get_ExistingObjectiveById_RetrivedSuccessful(string typeValue)
        {
            var added = await ArrangeObjective(typeValue);
            justAdded = added;

            var result = await synchronizer.Get(new[] { added.ExternalID });
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Any(x => x.ExternalID == added.ExternalID) == true);
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
