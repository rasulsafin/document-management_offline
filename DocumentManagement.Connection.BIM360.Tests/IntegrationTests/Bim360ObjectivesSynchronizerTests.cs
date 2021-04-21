using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization;
using MRS.DocumentManagement.Connection.Bim360.Synchronizers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.BIM360.Tests.IntegrationTests
{
    [TestClass]
    public class Bim360ObjectivesSynchronizerTests
    {
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static readonly string PROJECT_ID = "b.08616ce0-2cf7-43c3-a69e-b831e0870824";
        private static readonly  string NG_ISSUE_TYPE_ID = "3cbbb419-62ac-476f-a115-fd57defd0ac7";

        private static ISynchronizer<ObjectiveExternalDto> synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
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

            var services = new ServiceCollection();
            services.AddBim360();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            var serviceProvider = services.BuildServiceProvider();

            var connection = serviceProvider.GetService<Bim360Connection>();
            if (connection == null)
                throw new Exception();

            await connection.Connect(connectionInfo);

            var context = await connection.GetContext(connectionInfo);
            synchronizer = context.ObjectivesSynchronizer;
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var objective = CreateDto();

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyIdWithItems_AddedSuccessfully()
        {
            var objective = CreateDto();
            objective.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    FileName = Path.GetFileName(TEST_FILE_PATH),
                    FullPath = Path.GetFullPath(TEST_FILE_PATH),
                },
            };

            var result = await synchronizer.Add(objective);

            Assert.IsNotNull(result?.ExternalID);
        }

        [TestMethod]
        public async Task Remove_JustAddedObjective_RemovedSuccessfully()
        {
            // Add
            var objective = CreateDto();
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to remove.");

            // Remove
            await Task.Delay(3000);
            var result = await synchronizer.Remove(added);

            Assert.IsNotNull(result);
            Assert.AreEqual(ObjectiveStatus.Undefined, result.Status);
        }

        [TestMethod]
        public async Task Update_JustAddedObjective_UpdatedSuccessfully()
        {
            // Add
            var objective = CreateDto();
            var title = objective.Title;
            var added = await synchronizer.Add(objective);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            // Update
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }

        private ObjectiveExternalDto CreateDto()
            => new ObjectiveExternalDto
            {
                ProjectExternalID = PROJECT_ID,
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
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
                        ExternalID =
                            typeof(Issue.IssueAttributes).GetDataMemberName(
                                nameof(Issue.IssueAttributes.NgIssueTypeID)),
                        Value = NG_ISSUE_TYPE_ID,
                    },
                },
            };
    }
}
