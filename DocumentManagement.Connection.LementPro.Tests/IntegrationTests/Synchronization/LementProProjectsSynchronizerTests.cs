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
    public class LementProProjectsSynchronizerTests
    {
        private static readonly string TEST_BIM_FILE_PATH = "Resources/HelloWallIfc4TEST.ifc";
        private static readonly string TEST_PNG_FILE_PATH = "Resources/TestIcon.png";
        private static readonly string TEST_TXT_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static LementProProjectsSynchronizer synchronizer;

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
            synchronizer = new LementProProjectsSynchronizer(context);
        }

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneProjectExists_RetrivedSuccessful()
        {
            var result = await synchronizer.GetUpdatedIDs(DateTime.Now);

            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task Add_ProjectWithFiles_AddedSuccessfully()
        {
            var creationDateTime = DateTime.Now;
            var project = new ProjectExternalDto
            {
                Title = $"CreatedBySyncTest {creationDateTime.ToShortTimeString()}",
                UpdatedAt = creationDateTime,
                Items = new List<ItemExternalDto>
                {   new ItemExternalDto
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

            var result = await synchronizer.Add(project);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Update_JustAddedProject_UpdatedSuccessfully()
        {
            var creationDateTime = DateTime.Now;
            var title = $"CreatedBySyncTest {creationDateTime.ToShortTimeString()}";

            // Add
            var project = new ProjectExternalDto
            {
                Title = title,
                UpdatedAt = creationDateTime,
            };
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail("Project adding failed. There is nothing to update.");

            // Update
            await Task.Delay(3000);
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }

        [TestMethod]
        public async Task Update_JustAddedProjectWithItems_UpdatedSuccessfully()
        {
            var creationDateTime = DateTime.Now;
            var itemToRemoveName = Path.GetFileName(TEST_BIM_FILE_PATH);
            var title = $"CreatedBySyncTest {creationDateTime.ToShortTimeString()}";

            // Add
            var project = new ProjectExternalDto
            {
                Title = title,
                UpdatedAt = creationDateTime,
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
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail("Project adding failed. There is nothing to update.");

            // Update
            await Task.Delay(3000);
            added.Items = added.Items.Where(i => i.FileName != itemToRemoveName).ToList();
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(project.Items.Count - 1, result.Items.Count);
        }

        [TestMethod]
        public async Task Remove_JustAddedProject_RemovedSuccessfully()
        {
            // Add
            var creationDateTime = DateTime.Now;
            var project = new ProjectExternalDto
            {
                Title = $"CreatedBySyncTest {creationDateTime.ToShortTimeString()}",
                UpdatedAt = creationDateTime,
            };
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail("Project adding failed. There is nothing to delete.");

            // Remove
            await Task.Delay(3000);
            var result = await synchronizer.Remove(added);

            Assert.IsNotNull(result);
        }
    }
}
