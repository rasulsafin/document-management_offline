using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Synchronization
{
    [TestClass]
    public class MrsProObjectivesSynchronizerTests
    {
        private static readonly string TEST_BIM_FILE_PATH = "Resources/HelloWallIfc4TEST.ifc";
        private static readonly string TEST_PNG_FILE_PATH = "Resources/TestIcon.png";
        private static readonly string TEST_TXT_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static ISynchronizer<ObjectiveExternalDto> synchronizer;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            var connection = serviceProvider.GetService<MrsProConnection>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = "asidorov@briogroup.ru";
            var password = "GhundU72!c";
            var companyCode = "skprofitgroup";

            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { Constants.AUTH_EMAIL, email },
                    { Constants.AUTH_PASS, password },
                    { Constants.COMPANY_CODE, companyCode },
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
             => await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

        [TestCleanup]
        public async Task Cleanup()
          => await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = Constants.ISSUE_TYPE },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "Issue description",
                Status = ObjectiveStatus.Open,
                ProjectExternalID = "60b4d2719fbb9657cf2e0cbf",
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithBimElement_AddedSuccessfully()
        {

        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyIdWithFiles_AddedSuccessfully()
        {

        }

        [TestMethod]
        public async Task Remove_JustAddedObjective_RemovedSuccessfully()
        {

        }

        [TestMethod]
        public async Task Update_JustAddedObjective_UpdatedSuccessfully()
        {
            var title = "First type OPEN issue";
            var description = "Issue description";

            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = Constants.ISSUE_TYPE },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = title,
                Description = description,
                Status = ObjectiveStatus.Open,
                ProjectExternalID = "60b4d2719fbb9657cf2e0cbf",
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

            var newTitle = added.Title = $"UPDATED: {title}";
            var newDescription = added.Description = $"UPDATED: {description}";
            var newDueDate = added.DueDate = added.DueDate.AddDays(1);
            var newStatus = added.Status = ObjectiveStatus.Open;
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.ExternalID);
            Assert.AreEqual(newTitle, result.Title);
            Assert.AreEqual(newDescription, result.Description);
            Assert.AreEqual(newDueDate, result.DueDate);
            Assert.AreEqual(newStatus, result.Status);
        }

        [TestMethod]
        public async Task Update_JustAddedObjectiveWithItems_UpdatedSuccessfully()
        {

        }

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneObjectiveAdded_RetrivedSuccessful()
        {

        }
    }
}
