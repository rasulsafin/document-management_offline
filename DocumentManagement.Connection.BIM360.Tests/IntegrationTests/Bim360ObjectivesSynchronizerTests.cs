using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.BIM360.Tests.IntegrationTests
{
    [TestClass]
    public class Bim360ObjectivesSynchronizerTests
    {
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static Bim360ObjectivesSynchronizer synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var lastSyncDate = DateTime.MinValue;
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

            var context = await Bim360ConnectionContext.CreateContext(connectionInfo);
            synchronizer = new Bim360ObjectivesSynchronizer(context);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ProjectExternalID = "b.e0f02bdd-4355-4ab0-80bd-cecc3e6e9716",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "64868b1e-4431-48b3-8e54-5c287d227210" },
                UpdatedAt = DateTime.Now,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                DynamicFields = new List<DynamicFieldExternalDto>
                {
                    new DynamicFieldExternalDto
                    {
                        Name = nameof(Issue.Attributes.NgIssueSubtypeID),
                        Value = "1a1439dc-a221-4c78-a461-22e4e19f6b39",
                    },
                },
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyIdWithItems_AddedSuccessfully()
        {
            var objective = new ObjectiveExternalDto
            {
                ProjectExternalID = "b.e0f02bdd-4355-4ab0-80bd-cecc3e6e9716",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "64868b1e-4431-48b3-8e54-5c287d227210" },
                UpdatedAt = DateTime.Now,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                DynamicFields = new List<DynamicFieldExternalDto>
                {
                    new DynamicFieldExternalDto
                    {
                        Name = nameof(Issue.Attributes.NgIssueSubtypeID),
                        Value = "1a1439dc-a221-4c78-a461-22e4e19f6b39",
                    },
                },
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
                ProjectExternalID = "b.e0f02bdd-4355-4ab0-80bd-cecc3e6e9716",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "64868b1e-4431-48b3-8e54-5c287d227210" },
                UpdatedAt = DateTime.Now,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                DynamicFields = new List<DynamicFieldExternalDto>
                {
                    new DynamicFieldExternalDto
                    {
                        Name = nameof(Issue.Attributes.NgIssueSubtypeID),
                        Value = "1a1439dc-a221-4c78-a461-22e4e19f6b39",
                    },
                },
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
                ProjectExternalID = "b.e0f02bdd-4355-4ab0-80bd-cecc3e6e9716",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "64868b1e-4431-48b3-8e54-5c287d227210" },
                UpdatedAt = DateTime.Now,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                Title = title,
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
                DynamicFields = new List<DynamicFieldExternalDto>
                {
                    new DynamicFieldExternalDto
                    {
                        Name = nameof(Issue.Attributes.NgIssueSubtypeID),
                        Value = "1a1439dc-a221-4c78-a461-22e4e19f6b39",
                    },
                },
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
