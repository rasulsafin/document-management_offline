using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.BIM360.Tests.IntegrationTests
{
    [TestClass]
    public class Bim360ObjectivesSynchronizerTests
    {
        public static Bim360ObjectivesSynchronizer synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
            var lastSyncDate = DateTime.MinValue;
            var connectionInfo = new ConnectionInfoDto
            {
                ID = new ID<ConnectionInfoDto>(2),
                ConnectionType = new ConnectionTypeDto
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
                    ID = new ID<ConnectionTypeDto>(2),
                    Name = "BIM360",
                },
            };

            var context = await Bim360ConnectionContext.CreateContext(connectionInfo, lastSyncDate);
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
                DueDate = DateTime.MaxValue,
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);

            // Delete added issue
            await synchronizer.Remove(result);
        }

        [TestMethod]
        public async Task Remove_JustAddedObjective_RemovedSuccessfully()
        {
            // Add
            var objective = new ObjectiveExternalDto
            {
                ProjectExternalID = "e0f02bdd-4355-4ab0-80bd-cecc3e6e9716",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "64868b1e-4431-48b3-8e54-5c287d227210" },
                UpdatedAt = DateTime.Now,
                CreationDate = DateTime.Now,
                DueDate = DateTime.MaxValue,
                Title = "First type OPEN issue",
                Description = "ASAP: everything wrong! redo!!!",
                Status = ObjectiveStatus.Open,
            };
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to remove.");

            // Remove
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
                ProjectExternalID = "e0f02bdd-4355-4ab0-80bd-cecc3e6e9716",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = "64868b1e-4431-48b3-8e54-5c287d227210" },
                UpdatedAt = DateTime.Now,
                CreationDate = DateTime.Now,
                DueDate = DateTime.MaxValue,
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
