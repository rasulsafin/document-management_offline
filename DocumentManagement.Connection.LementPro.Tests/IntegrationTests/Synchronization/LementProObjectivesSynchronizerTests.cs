using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Synchronization
{
    [TestClass]
    public class LementProObjectivesSynchronizerTests
    {
        private static readonly string TEST_BIM_FILE_PATH = "Resources/HelloWallIfc4TEST.ifc";
        private static readonly string TEST_PNG_FILE_PATH = "Resources/TestIcon.png";
        private static readonly string TEST_TXT_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static LementProObjectivesSynchronizer synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
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

            var context = await LementProConnectionContext.CreateContext(connectionInfo);
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
        public async Task Add_ObjectiveWithEmptyIdWithFiles_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue with FILES",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                Items = new List<ItemExternalDto>
                {
                    new ItemExternalDto
                    {
                        FileName = Path.GetFileName(TEST_BIM_FILE_PATH),
                        FullPath = Path.GetFullPath(TEST_BIM_FILE_PATH),
                        ItemType = ItemType.Bim,
                    },
                    new ItemExternalDto
                    {
                        FileName = Path.GetFileName(TEST_PNG_FILE_PATH),
                        FullPath = Path.GetFullPath(TEST_PNG_FILE_PATH),
                        ItemType = ItemType.Media,
                    },
                    new ItemExternalDto
                    {
                        FileName = Path.GetFileName(TEST_TXT_FILE_PATH),
                        FullPath = Path.GetFullPath(TEST_TXT_FILE_PATH),
                        ItemType = ItemType.File,
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
            await Task.Delay(3000);
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }

        [TestMethod]
        public async Task Update_JustAddedObjectiveWithItems_UpdatedSuccessfully()
        {
            var title = "First type OPEN issue";
            var itemToRemoveName = Path.GetFileName(TEST_BIM_FILE_PATH);

            // Add
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = title,
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                Items = new List<ItemExternalDto>
                {   new ItemExternalDto
                    {
                        FileName = itemToRemoveName,
                        FullPath = Path.GetFullPath(TEST_BIM_FILE_PATH),
                        ItemType = ItemType.Bim,
                    },
                    new ItemExternalDto
                    {
                        FileName = Path.GetFileName(TEST_PNG_FILE_PATH),
                        FullPath = Path.GetFullPath(TEST_PNG_FILE_PATH),
                        ItemType = ItemType.Media,
                    },
                    new ItemExternalDto
                    {
                        FileName = Path.GetFileName(TEST_TXT_FILE_PATH),
                        FullPath = Path.GetFullPath(TEST_TXT_FILE_PATH),
                        ItemType = ItemType.File,
                    },
                },
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            // Update
            await Task.Delay(3000);
            added.Items = added.Items.Where(i => i.FileName != itemToRemoveName).ToList();
            var result = await synchronizer.Update(added);
            result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(objective.Items.Count - 1, result.Items.Count);
        }

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneObjectiveAdded_RetrivedSuccessful()
        {
            var creationTime = DateTime.Now;
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = creationTime,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                UpdatedAt = creationTime,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail();
            await Task.Delay(3000);

            var result = await synchronizer.GetUpdatedIDs(creationTime);

            Assert.IsTrue(result.Any(o => o == added.ExternalID));
        }
    }
}
