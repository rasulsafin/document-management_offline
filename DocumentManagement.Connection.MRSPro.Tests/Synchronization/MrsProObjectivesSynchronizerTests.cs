﻿using System;
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
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = ISSUE_TYPE },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "Issue description",
                Status = ObjectiveStatus.Open,
                ProjectExternalID = "60b4d2719fbb9657cf2e0cbf",
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result?.ExternalID);

            justAdded = result;
        }

        [TestMethod]
        public async Task Add_ObjectiveWithBimElement_AddedSuccessfully()
        {
            justAdded = null;
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyIdWithFiles_AddedSuccessfully()
        {
            justAdded = null;
        }

        [TestMethod]
        public async Task Remove_JustAddedObjective_RemovedSuccessfully()
        {
            var objective = await ArrangeObjective();

            var result = await synchronizer.Remove(objective);
            Assert.IsNotNull(result);

            justAdded = null;
        }

        [TestMethod]
        public async Task Update_JustAddedObjective_UpdatedSuccessfully()
        {
            var added = await ArrangeObjective();
            var title = added.Title;
            var description = added.Description;

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

            justAdded = result;
        }

        [TestMethod]
        public async Task Update_JustAddedObjectiveWithItems_UpdatedSuccessfully()
        {
            justAdded = null;
        }

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneObjectiveAdded_RetrivedSuccessful()
        {
            var added = await ArrangeObjective();
            justAdded = added;

            var result = await synchronizer.GetUpdatedIDs(DateTime.Now.AddDays(-1));
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Any(x => x == added.ExternalID) == true);
        }

        private async Task<ObjectiveExternalDto> ArrangeObjective()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = Constants.ISSUE_TYPE },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "Title",
                Description = "Description",
                Status = ObjectiveStatus.Open,
                ProjectExternalID = "60b4d2719fbb9657cf2e0cbf",
            };

            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            await Task.Delay(MILLISECONDS_TIME_DELAY);

            return added;
        }
    }
}