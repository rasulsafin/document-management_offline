using System;
using System.Collections.Generic;
using System.IO;
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
using Brio.Docs.Tests.Synchronization.Helpers;
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
        private readonly Lazy<string> locationGuidDefault = new Lazy<string>(() => Guid.NewGuid().ToString());
        private Mock<IConnection> connection;
        private SharedDatabaseFixture fixture;
        private Mock<ISynchronizer<ObjectiveExternalDto>> objectiveSynchronizer;
        private (Project local, Project synchronized, ProjectExternalDto remote) projects;
        private ObjectiveExternalDto remoteResult;
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
                SynchronizerTestsHelper.CreateSynchronizerStub<ObjectiveExternalDto>(x => remoteResult = x);
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
            var objectiveLocal = await CreateDummyLocalObjective();
            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await fixture.Context.Objectives.AddAsync(objectiveLocal);
            await fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(synchronized);
            Assert.IsNotNull(remoteResult);
            Assert.IsNull(synchronized.Location);
            Assert.IsNull(remoteResult.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewRemoteObjectiveWithoutLocation_AddObjectiveToLocalWithoutLocation()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(local);
            Assert.IsNotNull(synchronized);
            Assert.IsNull(remoteResult);
            Assert.IsNull(local.Location);
            Assert.IsNull(synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewLocalObjectiveWithLocation_AddObjectiveToRemoteWithLocation()
        {
            // Arrange.
            var objectiveLocal = await CreateDummyLocalObjective();
            var item = GetItemExistingItem();
            objectiveLocal.Location = CreateLocation(item);

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await fixture.Context.Objectives.AddAsync(objectiveLocal);
            await fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(synchronized);
            Assert.IsNotNull(remoteResult);
            Assert.IsNotNull(synchronized.Location);
            Assert.IsNotNull(remoteResult.Location);
            AssertLocation(objectiveLocal.Location, remoteResult.Location);
            AssertLocation(objectiveLocal.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_LocationItemChangedFromRemote_ChangeLocationItemOnLocal()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var item1Local = GetItemExistingItem(isSynchronized: true);
            var item1Synchronized = item1Local.SynchronizationMate;
            var item2 = GetItemExistingItem(1);

            var objectiveLocal = await CreateDummyLocalObjective(true, objectiveRemote.ExternalID);
            objectiveLocal.Location = CreateLocation(item1Local);

            var objectiveSynchronized = objectiveLocal.SynchronizationMate;
            objectiveSynchronized.Location = CreateLocation(item1Synchronized);

            objectiveRemote.Location = CreateLocationDto(item2);

            MockRemoteObjectives(new[] { objectiveRemote });
            await fixture.Context.Items.AddRangeAsync(item1Local, item1Synchronized, item2);
            await fixture.Context.Objectives.AddRangeAsync(objectiveLocal, objectiveSynchronized);
            await fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(local);
            Assert.IsNotNull(synchronized);
            Assert.IsNull(remoteResult);
            Assert.IsNotNull(local.Location);
            Assert.IsNotNull(synchronized.Location);
            AssertLocationItem(objectiveRemote.Location, local.Location);
            AssertLocationItem(objectiveRemote.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_LocationItemChangedFromLocal_ChangeLocationItemOnRemote()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var item1Local = GetItemExistingItem(isSynchronized: true);
            var item1Synchronized = item1Local.SynchronizationMate;
            var item2 = GetItemExistingItem(1);

            var objectiveLocal = await CreateDummyLocalObjective(true, objectiveRemote.ExternalID);
            objectiveLocal.Location = CreateLocation(item2);

            var objectiveSynchronized = objectiveLocal.SynchronizationMate;
            objectiveSynchronized.Location = CreateLocation(item1Synchronized);

            objectiveRemote.Location = CreateLocationDto(item1Synchronized);

            MockRemoteObjectives(new[] { objectiveRemote });
            await fixture.Context.Items.AddRangeAsync(item1Local, item1Synchronized, item2);
            await fixture.Context.Objectives.AddRangeAsync(objectiveLocal, objectiveSynchronized);
            await fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(local);
            Assert.IsNotNull(synchronized);
            Assert.IsNotNull(remoteResult);
            Assert.IsNotNull(local.Location);
            Assert.IsNotNull(synchronized.Location);
            Assert.IsNotNull(remoteResult.Location);
            AssertLocationItem(local.Location, synchronized.Location);
            AssertLocationItem(local.Location, remoteResult.Location);
        }

        [TestMethod]
        public async Task Synchronize_NewRemoteObjectiveWithLocation_AddObjectiveToLocalWithLocation()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var item = GetItemExistingItem();
            objectiveRemote.Location = CreateLocationDto(item);
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
            Assert.IsNull(remoteResult);
            Assert.IsNotNull(local.Location);
            Assert.IsNotNull(synchronized.Location);
            AssertLocation(objectiveRemote.Location, local.Location);
            AssertLocation(objectiveRemote.Location, synchronized.Location);
        }

        [TestMethod]
        public async Task Synchronize_RemoteLocationChanged_ChangeLocalLocation()
        {
            // Arrange.
            var objectiveRemote = await CreateDummyRemoteObjective();
            var itemLocal = GetItemExistingItem(isSynchronized: true);
            var itemSynchronized = itemLocal.SynchronizationMate;

            var objectiveLocal = await CreateDummyLocalObjective(true, objectiveRemote.ExternalID);
            objectiveLocal.Location = CreateLocation(itemLocal);

            var objectiveSynchronized = objectiveLocal.SynchronizationMate;
            objectiveSynchronized.Location = CreateLocation(itemSynchronized);

            objectiveRemote.Location = CreateLocationDto(itemLocal);
            objectiveRemote.Location.CameraPosition = (1.111, 2.454, -4666.22);

            MockRemoteObjectives(new[] { objectiveRemote });
            await fixture.Context.Items.AddRangeAsync(itemLocal, itemSynchronized);
            await fixture.Context.Objectives.AddRangeAsync(objectiveLocal, objectiveSynchronized);
            await fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();
            var local = await fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            Assert.IsNotNull(local);
            Assert.IsNotNull(synchronized);
            Assert.IsNull(remoteResult);
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

        private static void AssertLocation(Location expected, Location actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.PositionX, actual.PositionX);
            Assert.AreEqual(expected.PositionY, actual.PositionY);
            Assert.AreEqual(expected.PositionZ, actual.PositionZ);
            Assert.AreEqual(expected.CameraPositionX, actual.CameraPositionX);
            Assert.AreEqual(expected.CameraPositionY, actual.CameraPositionY);
            Assert.AreEqual(expected.CameraPositionZ, actual.CameraPositionZ);
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocation(Location expected, LocationExternalDto actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.PositionX, actual.Location.x);
            Assert.AreEqual(expected.PositionY, actual.Location.y);
            Assert.AreEqual(expected.PositionZ, actual.Location.z);
            Assert.AreEqual(expected.CameraPositionX, actual.CameraPosition.x);
            Assert.AreEqual(expected.CameraPositionY, actual.CameraPosition.y);
            Assert.AreEqual(expected.CameraPositionZ, actual.CameraPosition.z);
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocation(LocationExternalDto expected, Location actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.Location.x, actual.PositionX);
            Assert.AreEqual(expected.Location.y, actual.PositionY);
            Assert.AreEqual(expected.Location.z, actual.PositionZ);
            Assert.AreEqual(expected.CameraPosition.x, actual.CameraPositionX);
            Assert.AreEqual(expected.CameraPosition.y, actual.CameraPositionY);
            Assert.AreEqual(expected.CameraPosition.z, actual.CameraPositionZ);
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocation(LocationExternalDto expected, LocationExternalDto actual)
        {
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.Location.x, actual.Location.x);
            Assert.AreEqual(expected.Location.y, actual.Location.y);
            Assert.AreEqual(expected.Location.z, actual.Location.z);
            Assert.AreEqual(expected.CameraPosition.x, actual.CameraPosition.x);
            Assert.AreEqual(expected.CameraPosition.y, actual.CameraPosition.y);
            Assert.AreEqual(expected.CameraPosition.z, actual.CameraPosition.z);
            AssertLocationItem(expected, actual);
        }

        private static void AssertLocationItem(Location expected, Location actual)
        {
            Assert.IsNotNull(actual.Item);
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID);
        }

        private static void AssertLocationItem(Location expected, LocationExternalDto actual)
        {
            Assert.IsNotNull(actual.Item);
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID);
        }

        private static void AssertLocationItem(LocationExternalDto expected, Location actual)
        {
            Assert.IsNotNull(actual.Item);
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID);
        }

        private static void AssertLocationItem(LocationExternalDto expected, LocationExternalDto actual)
        {
            Assert.IsNotNull(actual.Item);
            Assert.AreEqual(expected.Item.ExternalID, actual.Item.ExternalID);
        }

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(objectiveSynchronizer, call, times);

        private async Task<Objective> CreateDummyLocalObjective(bool isSynchronized = false, string externalId = null)
        {
            var objectiveLocal = await CreateDummyObjective(externalId);
            objectiveLocal.Project = projects.local;

            if (isSynchronized)
            {
                var synchronized = CreateDummySynchronizedObjective(externalId);
                objectiveLocal.SynchronizationMate = await synchronized;
            }

            return objectiveLocal;
        }

        private async Task<Objective> CreateDummyObjective(string externalId)
        {
            var objectiveSynchronized = MockData.DEFAULT_OBJECTIVES[0];
            objectiveSynchronized.ObjectiveType = await fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            objectiveSynchronized.ExternalID = externalId;
            return objectiveSynchronized;
        }

        private async Task<ObjectiveExternalDto> CreateDummyRemoteObjective()
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

        private async Task<Objective> CreateDummySynchronizedObjective(string externalId = null)
        {
            var objectiveSynchronized = await CreateDummyObjective(externalId);
            objectiveSynchronized.Project = projects.synchronized;
            objectiveSynchronized.IsSynchronized = true;
            return objectiveSynchronized;
        }

        private Location CreateLocation(Item item)
            => new Location
            {
                PositionX = 1.29,
                PositionY = -222.5,
                PositionZ = 0.0001,
                CameraPositionX = 0.000,
                CameraPositionY = 0.111,
                CameraPositionZ = 0.2323,
                Guid = locationGuidDefault.Value,
                Item = item,
            };

        private LocationExternalDto CreateLocationDto(Item item)
            => new LocationExternalDto
            {
                Location = (1.29, -222.5, 0.0001),
                CameraPosition = (0.000, 0.111, 0.2323),
                Guid = locationGuidDefault.Value,
                Item = new ItemExternalDto
                {
                    ExternalID = item.ExternalID,
                    FileName = item.Name,
                },
            };

        private Item GetItemExistingItem(int index = 0, bool isSynchronized = false)
        {
            projects.local.Items ??= new List<Item>();
            var item = MockData.DEFAULT_ITEMS[index];
            projects.local.Items.Add(item);

            if (isSynchronized)
            {
                projects.synchronized.Items ??= new List<Item>();
                var itemSynchronized = MockData.DEFAULT_ITEMS[index];
                projects.synchronized.Items.Add(itemSynchronized);
                itemSynchronized.IsSynchronized = true;
                item.SynchronizationMate = itemSynchronized;

                projects.remote.Items ??= new List<ItemExternalDto>();
                projects.remote.Items.Add(
                    new ItemExternalDto
                    {
                        ExternalID = itemSynchronized.ExternalID,
                        FileName = itemSynchronized.Name,
                        FullPath = Path.GetFullPath(itemSynchronized.RelativePath),
                        ItemType = ItemType.File,
                        UpdatedAt = itemSynchronized.UpdatedAt,
                    });
            }

            return item;
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
