using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Tests.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Synchronization
{
    [TestClass]
    public class SynchronizerObjectiveLocationTests : IDisposable
    {
        private Mock<IConnection> connection;
        private SharedDatabaseFixture fixture;
        private Mock<ISynchronizer<ObjectiveExternalDto>> objectiveSynchronizer;
        private (Project local, Project synchronized, ProjectExternalDto remote) projects;
        private ObjectiveExternalDto remote;
        private ServiceProvider serviceProvider;
        private Synchronizer synchronizer;

        [TestInitialize]
        public async Task Setup()
        {
            fixture = SynchronizerTestsHelper.CreateFixture();
            serviceProvider = SynchronizerTestsHelper.CreateServiceProvider(fixture.Context);
            synchronizer = serviceProvider.GetService<Synchronizer>();

            connection = new Mock<IConnection>();
            var projectSynchronizer = SynchronizerTestsHelper.CreateSynchronizerStub<ProjectExternalDto>();
            objectiveSynchronizer =
                SynchronizerTestsHelper.CreateSynchronizerStub<ObjectiveExternalDto>(x => remote = x);
            var context = SynchronizerTestsHelper.CreateConnectionContextStub(projectSynchronizer.Object, objectiveSynchronizer.Object);
            connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>())).ReturnsAsync(context.Object);
            projects = await SynchronizerTestsHelper.ArrangeProject(projectSynchronizer, fixture);
        }

        [TestMethod]
        public async Task Synchronize_NewLocalObjectiveWithoutLocation_AddObjectiveToRemoteWithoutLocation()
        {
            // Arrange.
            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            objectiveLocal.Project = projects.local;
            objectiveLocal.ObjectiveType =
                await fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await fixture.Context.Objectives.AddAsync(objectiveLocal);
            await fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(synchronized);
            Assert.IsNotNull(remote);
            Assert.AreEqual(null, synchronized.Location);
            Assert.AreEqual(null, remote.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewRemoteObjectiveWithoutLocation_AddObjectiveToLocalWithoutLocation()
        {
            // Arrange.
            ObjectiveExternalDto objectiveRemote = await GetDummyRemoteObjective();
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(local);
            Assert.IsNotNull(synchronized);
            Assert.AreEqual(null, local.Location);
            Assert.AreEqual(null, synchronized.Location);
        }

        [TestCleanup]
        public void Cleanup()
            => Dispose();

        public void Dispose()
        {
            fixture.Dispose();
            serviceProvider.Dispose();
            GC.SuppressFinalize(this);
        }

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(objectiveSynchronizer, call, times);

        private async Task<ObjectiveExternalDto> GetDummyRemoteObjective()
        {
            var objectiveType = await fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = projects.remote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                CreationDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Title = "Title",
                Description = "Description",
                BimElements = new List<BimElementExternalDto>(),
                Status = ObjectiveStatus.Open,
                UpdatedAt = DateTime.UtcNow,
            };
            return objectiveRemote;
        }

        private void MockRemoteObjectives(IReadOnlyCollection<ObjectiveExternalDto> array)
            => SynchronizerTestsHelper.MockGetRemote(objectiveSynchronizer, array, x => x.ExternalID);

        private async Task<ICollection<SynchronizingResult>> Synchronize()
            => await synchronizer.Synchronize(
                new SynchronizingData
                {
                    User = await fixture.Context.Users.FirstOrDefaultAsync(),
                },
                connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                CancellationToken.None);
    }
}
