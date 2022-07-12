﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.LementPro.Tests.IntegrationTests.Synchronization
{
    [TestClass]
    public class LementProObjectivesSynchronizerTests
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
            services.AddLementPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            var connection = serviceProvider.GetService<LementProConnection>();

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

            await connection!.Connect(connectionInfo, CancellationToken.None);
            var context = await connection.GetContext(connectionInfo);
            synchronizer = context.ObjectivesSynchronizer;
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

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
                ProjectExternalID = "402014",
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithBimElement_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = "40179" },
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue with BIM ref",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                ProjectExternalID = "402014",
                BimElements = new List<BimElementExternalDto>
                {
                    new BimElementExternalDto
                    {
                        ElementName = "Element1",
                        ParentName = "BimElementParent",
                        GlobalID = "BimElementGUID",
                    },
                },
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
            Assert.IsNotNull(result.BimElements);
            Assert.IsTrue(result.BimElements.Any());
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
                ProjectExternalID = "402014",
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
            Assert.AreEqual(ObjectiveStatus.Closed, result.Status);
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
                {
                    new ItemExternalDto
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