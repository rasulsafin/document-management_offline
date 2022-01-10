using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
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

        [TestCleanup]
        public void Cleanup()
            => Dispose();

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
            Assert.IsNull(synchronized.Location);
            Assert.IsNull(remote.Location);
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
            Assert.IsNull(local.Location);
            Assert.IsNull(synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewLocalObjectiveWithLocation_AddObjectiveToRemoteWithLocation()
        {
            // Arrange.
            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            objectiveLocal.Project = projects.local;
            objectiveLocal.ObjectiveType =
                await fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            projects.local.Items ??= new List<Item>();
            var item = MockData.DEFAULT_ITEMS[0];
            projects.local.Items.Add(item);
            objectiveLocal.Location = new Location
            {
                PositionX = 1.29,
                PositionY = -222.5,
                PositionZ = 0.0001,
                CameraPositionX = 0.000,
                CameraPositionY = 0.111,
                CameraPositionZ = 0.2323,
                Guid = Guid.NewGuid().ToString(),
                Item = item,
            };

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
            Assert.IsNotNull(synchronized.Location);
            Assert.IsNotNull(remote.Location);
            AssertLocation(objectiveLocal.Location, remote.Location);
            AssertLocation(objectiveLocal.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewRemoteObjectiveWithLocation_AddObjectiveToLocalWithLocation()
        {
            // Arrange.
            ObjectiveExternalDto objectiveRemote = await GetDummyRemoteObjective();
            projects.local.Items ??= new List<Item>();
            var item = MockData.DEFAULT_ITEMS[0];
            projects.local.Items.Add(item);
            objectiveRemote.Location = new LocationExternalDto
            {
                Location = (1.29, -222.5, 0.0001),
                CameraPosition = (0.000, 0.111, 0.2323),
                Guid = Guid.NewGuid().ToString(),
                Item = new ItemExternalDto
                {
                    ExternalID = item.ExternalID,
                    FileName = item.Name,
                },
            };
            await fixture.Context.SaveChangesAsync();
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(local);
            Assert.IsNotNull(synchronized);
            Assert.IsNotNull(local.Location);
            Assert.IsNotNull(synchronized.Location);
            AssertLocation(objectiveRemote.Location, local.Location);
            AssertLocation(objectiveRemote.Location, synchronized.Location);
        }

        public void Dispose()
        {
            fixture.Dispose();
            serviceProvider.Dispose();
            GC.SuppressFinalize(this);
        }

        private void AssertLocation(Location expected, Location actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.PositionX, actual.PositionX);
            Assert.AreEqual(expected.PositionY, actual.PositionY);
            Assert.AreEqual(expected.PositionZ, actual.PositionZ);
            Assert.AreEqual(expected.CameraPositionX, actual.CameraPositionX);
            Assert.AreEqual(expected.CameraPositionY, actual.CameraPositionY);
            Assert.AreEqual(expected.CameraPositionZ, actual.CameraPositionZ);
        }

        private void AssertLocation(Location expected, LocationExternalDto actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.PositionX, actual.Location.x);
            Assert.AreEqual(expected.PositionY, actual.Location.y);
            Assert.AreEqual(expected.PositionZ, actual.Location.z);
            Assert.AreEqual(expected.CameraPositionX, actual.CameraPosition.x);
            Assert.AreEqual(expected.CameraPositionY, actual.CameraPosition.y);
            Assert.AreEqual(expected.CameraPositionZ, actual.CameraPosition.z);
        }

        private void AssertLocation(LocationExternalDto expected, Location actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.Location.x, actual.PositionX);
            Assert.AreEqual(expected.Location.y, actual.PositionY);
            Assert.AreEqual(expected.Location.z, actual.PositionZ);
            Assert.AreEqual(expected.CameraPosition.x, actual.CameraPositionX);
            Assert.AreEqual(expected.CameraPosition.y, actual.CameraPositionY);
            Assert.AreEqual(expected.CameraPosition.z, actual.CameraPositionZ);
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
