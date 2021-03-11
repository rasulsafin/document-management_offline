using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Synchronization
{
    [TestClass]
    public class LementProObjectivesSynchronizerTests
    {
        public static LementProObjectivesSynchronizer synchronizer;
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var lastSyncDate = DateTime.MinValue;
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

            var context = await LementProConnectionContext.CreateContext(connectionInfo, lastSyncDate);
            synchronizer = new LementProObjectivesSynchronizer(context);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyIdWithItems_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                Items = new List<ItemExternalDto>
                {
                    new ItemExternalDto
                    {
                        FileName = Path.GetFileName(TEST_FILE_PATH),
                        FullPath = Path.GetFullPath(TEST_FILE_PATH),
                    },
                },
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Remove_JustAddedObjective_RemovedSuccessfully()
        {
            // Add
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to remove.");

            // Remove
            await Task.Delay(3000);
            var result = await synchronizer.Remove(added);

            Assert.IsNotNull(result);
            Assert.AreEqual(ObjectiveStatus.Ready, result.Status);
        }

        [TestMethod]
        public async Task Update_JustAddedObjective_UpdatedSuccessfully()
        {
            var title = "First type OPEN issue";

            // Add
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = title,
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            // Update
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }
    }
}
