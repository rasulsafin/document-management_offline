using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Synchronization;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.Bim360.Tests.IntegrationTests
{
    [TestClass]
    public class Bim360ObjectivesSynchronizerTests
    {
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static readonly string PROJECT_ID = "b.08616ce0-2cf7-43c3-a69e-b831e0870824";

        private static string ngIssueTypeID;
        private static ISynchronizer<ObjectiveExternalDto> synchronizer;
        private static ServiceProvider serviceProvider;
        private static TypeSubtypeEnumCreator typeEnumCreator;

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
            serviceProvider = services.BuildServiceProvider();

            var connection = serviceProvider.GetService<Bim360Connection>();
            if (connection == null)
                throw new Exception();

            await connection.Connect(connectionInfo, CancellationToken.None);

            var context = await connection.GetContext(connectionInfo) as Bim360ConnectionContext;
            var provider = context!.Scope.ServiceProvider;
            var filler = provider.GetService<SnapshotFiller>();
            var checker = provider.GetService<IAccessController>();
            var snapshot = provider.GetService<SnapshotGetter>();
            await checker!.CheckAccessAsync(CancellationToken.None);
            typeEnumCreator = provider.GetService<TypeSubtypeEnumCreator>();
            filler!.IgnoreTestEntities = false;
            await filler.UpdateHubsIfNull();
            await filler.UpdateProjectsIfNull();
            await filler.UpdateIssuesIfNull();
            await filler.UpdateIssueTypes();
            await filler.UpdateStatuses();
            ngIssueTypeID = snapshot!.GetProject(PROJECT_ID).IssueTypes.First().Value.ID;
            synchronizer = context.ObjectivesSynchronizer;
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

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
            => new ()
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
                    new ()
                    {
                        ExternalID = typeEnumCreator.EnumExternalID,
                        Value = ngIssueTypeID,
                    },
                },
            };
    }
}
