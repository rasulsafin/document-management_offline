using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronization;
using MRS.DocumentManagement.Synchronization.Models;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class SynchronizerObjectiveTests
    {
        private static Synchronizer synchronizer;
        private static IMapper mapper;

        private static Mock<ISynchronizer<ObjectiveExternalDto>> ObjectiveSynchronizer { get; set; }

        private static Mock<ISynchronizer<ProjectExternalDto>> ProjectSynchronizer { get; set; }

        private static SharedDatabaseFixture Fixture { get; set; }

        private static Mock<IConnection> Connection { get; set; }

        private static Mock<IConnectionContext> Context { get; set; }

        private static ObjectiveExternalDto ResultObjectiveExternalDto { get; set; }

        private static (Project local, Project synchronized, ProjectExternalDto remote) Project { get; set; }

        [TestInitialize]
        public async Task Setup()
        {
            Fixture = new SharedDatabaseFixture(
                context =>
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    var users = MockData.DEFAULT_USERS;
                    var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                    context.Users.AddRange(users);
                    context.ObjectiveTypes.AddRange(objectiveTypes);
                    context.SaveChanges();
                });

            Connection = new Mock<IConnection>();
            Context = new Mock<IConnectionContext>();
            Connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoDto>(), It.IsAny<DateTime>()))
               .ReturnsAsync(Context.Object);
            ProjectSynchronizer = new Mock<ISynchronizer<ProjectExternalDto>>();
            ObjectiveSynchronizer = new Mock<ISynchronizer<ObjectiveExternalDto>>();
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult);
            ProjectSynchronizer.Setup(x => x.Update(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult);
            ProjectSynchronizer.Setup(x => x.Remove(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult);
            ObjectiveSynchronizer.Setup(x => x.Add(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult)
               .Callback<ObjectiveExternalDto>(x => ResultObjectiveExternalDto = x);
            ObjectiveSynchronizer.Setup(x => x.Update(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult)
               .Callback<ObjectiveExternalDto>(x => ResultObjectiveExternalDto = x);
            ObjectiveSynchronizer.Setup(x => x.Remove(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult)
               .Callback<ObjectiveExternalDto>(x => ResultObjectiveExternalDto = x);
            Context.Setup(x => x.ObjectivesSynchronizer).Returns(ObjectiveSynchronizer.Object);
            Context.Setup(x => x.ProjectsSynchronizer).Returns(ProjectSynchronizer.Object);

            IServiceCollection services = new ServiceCollection();
            var resolver1 = new ObjectiveExternalDtoProjectIdResolver(Fixture.Context);
            services.AddTransient(x => resolver1);
            var resolver2 = new ObjectiveExternalDtoObjectiveTypeResolver(Fixture.Context);
            services.AddTransient(x => resolver2);
            var resolver3 = new ObjectiveExternalDtoObjectiveTypeIDResolver(Fixture.Context);
            services.AddTransient(x => resolver3);
            services.AddAutoMapper(typeof(MappingProfile));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();

            synchronizer = new Synchronizer(mapper);

            Project = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Synchronize_ObjectivesUnchanged_DoNothing()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, objectiveSynchronized);
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteObjectiveSame_Synchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            objectiveRemote.Description = objectiveLocal.Description = "New same description";
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, objectiveSynchronized);
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveAddedLocal_AddObjectiveToRemoteAndSynchronize()
        {
            // Arrange.
            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            objectiveLocal.Project = Project.local;
            objectiveLocal.ObjectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Objectives.AddAsync(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                  new SynchronizingData
                  {
                      Context = Fixture.Context,
                      User = await Fixture.Context.Users.FirstAsync(),
                  },
                  Connection.Object,
                  new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Include(x => x.Project).Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Include(x => x.Project).Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveAddedRemote_AddObjectiveToLocalAndSynchronize()
        {
            // Arrange.
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = Project.remote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                CreationDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Title = "Title",
                Description = "Description",
                Status = ObjectiveStatus.Open,
                UpdatedAt = DateTime.UtcNow,
            };
            Context.Setup(x => x.Objectives).ReturnsAsync(new[] { objectiveRemote });

            // Act.
            await synchronizer.Synchronize(
                    new SynchronizingData
                    {
                        Context = Fixture.Context,
                        User = await Fixture.Context.Users.FirstAsync(),
                    },
                    Connection.Object,
                    new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Include(x => x.Project).Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Include(x => x.Project).Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveRemovedFromLocal_RemoveObjectiveFromRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            Fixture.Context.Remove(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            Assert.AreEqual(0, await Fixture.Context.Objectives.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Objectives.Unsynchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveRemovedFromRemote_RemoveObjectiveFromLocalAndSynchronize()
        {
            // Arrange.
            await ArrangeObjective(true);

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            Assert.AreEqual(0, await Fixture.Context.Objectives.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Objectives.Unsynchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveHasLocalChanges_ApplyLocalChangesToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();

            var description = objectiveLocal.Description = "New local description";
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            Assert.AreEqual(description, ResultObjectiveExternalDto.Description);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveHasRemoteChanges_ApplyRemoteChangesToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            var description = objectiveRemote.Description = "New remote description";

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            Assert.AreEqual(description, local.Description);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesHaveChanges_MergeObjectivesAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveRemote.UpdatedAt = DateTime.UtcNow.AddDays(1);
            var title = objectiveLocal.Title = "New local title";
            var description = objectiveRemote.Description = "New remote description";
            var dueDateIrrelevant = objectiveLocal.DueDate = new DateTime(2021, 3, 10);
            var dueDateRelevant = objectiveRemote.DueDate = new DateTime(2021, 3, 11);
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            Assert.AreEqual(title, synchronized.Title);
            Assert.AreEqual(description, synchronized.Description);
            Assert.AreNotEqual(dueDateIrrelevant, synchronized.DueDate);
            Assert.AreEqual(dueDateRelevant, synchronized.DueDate);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewItem_AddItemToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            var item = MockData.DEFAULT_ITEMS[0];
            item.Objectives ??= new List<ObjectiveItem>();
            item.Objectives.Add(
                new ObjectiveItem
                {
                    Item = item,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            Assert.AreEqual(1, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveHasNewItem_AddItemToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, remoteObjective) = await ArrangeObjective();
            remoteObjective.Items.Add(
                new ItemExternalDto
                {
                    ExternalID = "new_external_item_id",
                    FileName = "1.txt",
                    ItemType = ItemTypeDto.File,
                    UpdatedAt = DateTime.UtcNow,
                });

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            Assert.AreEqual(1, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(remoteObjective), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteObjectiveHaveNewItems_AddItemsAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, remoteObjective) = await ArrangeObjective();
            var item = MockData.DEFAULT_ITEMS[0];
            item.Objectives ??= new List<ObjectiveItem>();
            item.Objectives.Add(
                new ObjectiveItem
                {
                    Item = item,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.SaveChangesAsync();
            remoteObjective.Items.Add(
                new ItemExternalDto
                {
                    ExternalID = "new_external_item_id",
                    FileName = "1.txt",
                    ItemType = ItemTypeDto.File,
                    UpdatedAt = DateTime.UtcNow,
                });

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            Assert.AreEqual(2, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(2, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromLocalObjective_RemoveItemFromRemoteAndSynchronize()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var item = MockData.DEFAULT_ITEMS[0];
            var externalItem = new ItemExternalDto
            {
                ExternalID = "new_external_item_id",
                FileName = "1.txt",
                ItemType = ItemTypeDto.File,
                UpdatedAt = DateTime.UtcNow,
            };
            objectiveRemote.Items.Add(externalItem);
            item.Objectives ??= new List<ObjectiveItem>();
            item.IsSynchronized = true;
            item.ExternalID = externalItem.ExternalID;
            item.Objectives.Add(
                new ObjectiveItem
                {
                    Item = item,
                    Objective = objectiveSynchronized,
                });
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteObjectiveAndItemIsNeeded_UnlinkLocalItemAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var localItem = MockData.DEFAULT_ITEMS[0];
            var synchronizedItem = MockData.DEFAULT_ITEMS[0];
            synchronizedItem.Objectives ??= new List<ObjectiveItem>();
            synchronizedItem.IsSynchronized = true;
            synchronizedItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = synchronizedItem,
                    Objective = objectiveSynchronized,
                });
            localItem.Project = Project.local;
            localItem.Objectives ??= new List<ObjectiveItem>();
            localItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = localItem,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddRangeAsync(localItem, synchronizedItem);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                    ProjectsFilter = x => false,
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteObjectiveAndItemIsNotNeeded_RemoveItemAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var localItem = MockData.DEFAULT_ITEMS[0];
            var synchronizedItem = MockData.DEFAULT_ITEMS[0];
            synchronizedItem.Objectives ??= new List<ObjectiveItem>();
            synchronizedItem.IsSynchronized = true;
            synchronizedItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = synchronizedItem,
                    Objective = objectiveSynchronized,
                });
            localItem.Objectives ??= new List<ObjectiveItem>();
            localItem.Objectives.Add(
                new ObjectiveItem
                {
                    Item = localItem,
                    Objective = objectiveLocal,
                });
            await Fixture.Context.Items.AddRangeAsync(localItem, synchronizedItem);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                    ProjectsFilter = x => false,
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveChangeProject_RemoteAndSynchronizedObjectivesChangeProject()
        {
            var connection = new Mock<IConnection>();
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = new User(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = project => true,
                },
                connection.Object,
                new ConnectionInfoDto());
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveChangeProject_LocalAndSynchronizedObjectivesChangeProject()
        {
            var connection = new Mock<IConnection>();
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = new User(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = project => true,
                },
                connection.Object,
                new ConnectionInfoDto());
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveChangeParent_RemoteAndSynchronizedObjectivesChangeProject()
        {
            var connection = new Mock<IConnection>();
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = new User(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = project => true,
                },
                connection.Object,
                new ConnectionInfoDto());
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveChangeParent_LocalAndSynchronizedObjectivesChangeParent()
        {
            // Arrange
            var existingObjective = MockData.DEFAULT_OBJECTIVES[1];
            existingObjective.Project = Project.local;
            var existingSynchronizedObjective = MockData.DEFAULT_OBJECTIVES[1];
            existingSynchronizedObjective.Project = Project.synchronized;
            var type = Fixture.Context.ObjectiveTypes.First();
            existingObjective.ObjectiveType = existingSynchronizedObjective.ObjectiveType = type;
            await Fixture.Context.Objectives.AddAsync(existingObjective);
            await Fixture.Context.SaveChangesAsync();
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            var remoteExisting = mapper.Map<ObjectiveExternalDto>(existingObjective);

            // Remote changed
            var remoteId = objectiveRemote.ParentObjectiveExternalID = remoteExisting.ExternalID;
            Context.Setup(p => p.Objectives).ReturnsAsync(new[] { remoteExisting, objectiveRemote });

            // Act
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfoDto());

            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();
            var remote = (await Context.Object.Objectives).First();

            // Assert
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            CheckObjectives(local, objectiveLocal);
            CheckObjectives(synchronized, mapper.Map<Objective>(remote), false);
            CheckSynchronizedObjectives(local, synchronized);
            Assert.AreEqual(remoteId, synchronized.ExternalID);
            Assert.AreEqual(remoteId, remote.ExternalID);
            Assert.AreEqual(remoteId, local.ExternalID);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteObjectiveChangeParent_ChangeParentAtRemoteAndSynchronizedObjectives()
        {
            // Arrange
            var localObjective2 = MockData.DEFAULT_OBJECTIVES[1];
            var localObjective3 = MockData.DEFAULT_OBJECTIVES[1];
            var localObjective4 = MockData.DEFAULT_OBJECTIVES[1];
            localObjective2.Project
                = localObjective3.Project
                = localObjective4.Project = Project.local;
            var synchronizedObjective2 = MockData.DEFAULT_OBJECTIVES[1];
            var synchronizedObjective3 = MockData.DEFAULT_OBJECTIVES[1];
            var synchronizedObjective4 = MockData.DEFAULT_OBJECTIVES[1];
            synchronizedObjective2.Project
                = synchronizedObjective3.Project
                = synchronizedObjective4.Project
                = Project.synchronized;
            var type = Fixture.Context.ObjectiveTypes.First();

            // TODO: Fix this test
            //localObjective1.ObjectiveType = synchronizedObjective1.ObjectiveType = type;
            //await Fixture.Context.Objectives.AddAsync(localObjective1);
            //await Fixture.Context.SaveChangesAsync();
            //var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            //var remoteExisting = mapper.Map<ObjectiveExternalDto>(localObjective1);

            //// Remote changed
            //objectiveRemote.ParentObjectiveExternalID = remoteExisting.ExternalID;
            //Context.Setup(p => p.Objectives).ReturnsAsync(new[] { remoteExisting, objectiveRemote });

            //// Local changed
            //var localAndSynchronizedParentId = objectiveLocal.ParentObjectiveID = localObjective1.ID;
            //Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            //await Fixture.Context.SaveChangesAsync();

            //// Act
            //await synchronizer.Synchronize(
            //    new SynchronizingData
            //    {
            //        Context = Fixture.Context,
            //        User = await Fixture.Context.Users.FirstAsync(),
            //    },
            //    Connection.Object,
            //    new ConnectionInfoDto());

            //var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            //var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();
            //var remote = (await Context.Object.Objectives).First();

            //// Assert
            //ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            //ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            //ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Once);
            //CheckObjectives(local, objectiveLocal);
            //CheckObjectives(synchronized, mapper.Map<Objective>(remote), false);
            //CheckSynchronizedObjectives(local, synchronized);
            //Assert.AreEqual(localAndSynchronizedParentId, synchronized.ExternalID);
            //Assert.AreEqual(localAndSynchronizedParentId, remote.ExternalID);
        }

        private void CheckSynchronizedObjectives(Objective local, Objective synchronized)
        {
            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(local.Items?.Count ?? 0, synchronized.Items?.Count ?? 0);
            Assert.AreEqual(local.ChildrenObjectives?.Count ?? 0, synchronized.ChildrenObjectives?.Count ?? 0);
            Assert.AreEqual(local.DynamicFields?.Count ?? 0, synchronized.DynamicFields?.Count ?? 0);
            Assert.AreEqual(local.BimElements?.Count ?? 0, synchronized.BimElements?.Count ?? 0);
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);

            CheckObjectives(local, synchronized, false);

            foreach (var childObjective in local.ChildrenObjectives ?? Enumerable.Empty<Objective>())
            {
                var synchronizedchildObjective = synchronized.ChildrenObjectives?
                  .FirstOrDefault(x => childObjective.ExternalID == x.ExternalID || childObjective.SynchronizationMateID == x.ID);
            }

            foreach (var item in local.Items ?? Enumerable.Empty<ObjectiveItem>())
            {
                var synchronizedItem = synchronized.Items?
                   .FirstOrDefault(x => item.Item.SynchronizationMateID == x.ItemID);
                Assert.AreNotEqual(null, synchronizedItem);
                SynchronizerTestsHelper.CheckSynchronizedItems(item.Item, synchronizedItem?.Item);
            }

            foreach (var bimElement in local.BimElements ?? Enumerable.Empty<BimElementObjective>())
            {
                var synchronizedItem = synchronized.BimElements?
                   .FirstOrDefault(x => bimElement.BimElementID == x.BimElementID);
                //  CheckSynchronizedBimElements(bimElement.BimElement, bimElement.BimElement);
            }
        }

        private void CheckObjectives(Objective a, Objective b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Project.ExternalID, b.Project.ExternalID);
            Assert.AreEqual(a.AuthorID, b.AuthorID);
            Assert.AreEqual(a.ParentObjectiveID, b.ParentObjectiveID);
            Assert.AreEqual(a.ObjectiveType.Name, b.ObjectiveType.Name);
            Assert.AreEqual(a.CreationDate, b.CreationDate);
            Assert.AreEqual(a.DueDate, b.DueDate);
            Assert.AreEqual(a.Title, b.Title);
            Assert.AreEqual(a.Description, b.Description);
            Assert.AreEqual(a.Status, b.Status);

            if (checkIDs)
            {
                SynchronizerTestsHelper.CheckIDs(a, b);
            }
        }

        private async Task<(Objective local, Objective synchronized, ObjectiveExternalDto remote)> ArrangeObjective(bool emptyRemote = false)
        {
            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveSynchronized = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = Project.local.ExternalID,
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                UpdatedAt = DateTime.Now,
                CreationDate = objectiveLocal.CreationDate,
                DueDate = objectiveLocal.DueDate,
                Title = objectiveLocal.Title,
                Description = objectiveLocal.Description,
                Items = new List<ItemExternalDto>(),
                Status = (ObjectiveStatus)objectiveLocal.Status,
            };

            objectiveLocal.ExternalID = objectiveSynchronized.ExternalID = objectiveRemote.ExternalID;
            objectiveLocal.Project = Project.local;
            objectiveLocal.ProjectID = Project.local.ID;
            objectiveLocal.ObjectiveType = objectiveType;
            objectiveLocal.ObjectiveTypeID = objectiveType.ID;
            objectiveSynchronized.ObjectiveType = objectiveType;
            objectiveSynchronized.ObjectiveTypeID = objectiveType.ID;
            objectiveSynchronized.Project = Project.synchronized;
            objectiveSynchronized.ProjectID = Project.synchronized.ID;
            Context
               .Setup(x => x.Objectives)
               .ReturnsAsync(emptyRemote ? ArraySegment<ObjectiveExternalDto>.Empty : new[] { objectiveRemote });
            if (emptyRemote)
                objectiveRemote = null;
            objectiveSynchronized.IsSynchronized = true;
            objectiveLocal.SynchronizationMate = objectiveSynchronized;
            await Fixture.Context.Objectives.AddRangeAsync(objectiveSynchronized, objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            return (objectiveLocal, objectiveSynchronized, objectiveRemote);
        }
    }
}
