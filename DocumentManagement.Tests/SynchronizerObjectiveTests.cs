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

        private static List<ObjectiveExternalDto> ResultObjectiveExternalDtos { get; set; }

        private static ObjectiveExternalDto ResultObjectiveExternalDto => ResultObjectiveExternalDtos.First();

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

            ResultObjectiveExternalDtos = new List<ObjectiveExternalDto>();
            Connection = new Mock<IConnection>();
            Context = new Mock<IConnectionContext>();
            Connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>())).ReturnsAsync(Context.Object);
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
               .Callback<ObjectiveExternalDto>(x =>
                {
                    x.ExternalID = $"new_objective_{Guid.NewGuid()}";
                    ResultObjectiveExternalDtos.Add(x);
                });
            ObjectiveSynchronizer.Setup(x => x.Update(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult)
               .Callback<ObjectiveExternalDto>(x => ResultObjectiveExternalDtos.Add(x));
            ObjectiveSynchronizer.Setup(x => x.Remove(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult)
               .Callback<ObjectiveExternalDto>(x => ResultObjectiveExternalDtos.Add(x));
            Context.Setup(x => x.ObjectivesSynchronizer).Returns(ObjectiveSynchronizer.Object);
            Context.Setup(x => x.ProjectsSynchronizer).Returns(ProjectSynchronizer.Object);

            IServiceCollection services = new ServiceCollection();
            services.AddTransient(x => new ObjectiveExternalDtoProjectIdResolver(Fixture.Context));
            services.AddTransient(x => new ObjectiveExternalDtoObjectiveTypeResolver(Fixture.Context));
            services.AddTransient(x => new ObjectiveExternalDtoObjectiveTypeIDResolver(Fixture.Context));
            services.AddTransient(x => new BimElementObjectiveTypeConverter(Fixture.Context));
            services.AddTransient(x => new DynamicFieldValueResolver(Fixture.Context));
            services.AddTransient(x => new DynamicFieldExternalDtoValueResolver(Fixture.Context));
            services.AddTransient(x => new ConnectionInfoAuthFieldValuesResolver(new CryptographyHelper()));
            services.AddTransient(x => new ConnectionInfoDtoAuthFieldValuesResolver(new CryptographyHelper()));
            services.AddTransient(x => new ObjectiveProjectIDResolver(Fixture.Context));
            services.AddTransient(x => new ObjectiveExternalDtoProjectResolver(Fixture.Context));
            services.AddTransient(x => new ObjectiveObjectiveTypeResolver(Fixture.Context));
            services.AddAutoMapper(typeof(MappingProfile));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();

            synchronizer = new Synchronizer();

            Project = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Objectives.AddAsync(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            await Fixture.Context.BimElementObjectives.AddAsync(
                new BimElementObjective
                {
                    BimElement = new BimElement
                    {
                        ParentName = "parent",
                        GlobalID = "guid",
                    },
                    Objective = objectiveLocal,
                });

            var dynamicField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            dynamicField.Objective = objectiveLocal;
            await Fixture.Context.DynamicFields.AddAsync(dynamicField);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
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
                BimElements = new List<BimElementExternalDto>
                {
                    new BimElementExternalDto
                    {
                        GlobalID = "guid",
                        ParentName = "1.ifc",
                    },
                },
                Status = ObjectiveStatus.Open,
                UpdatedAt = DateTime.UtcNow,
            };
            MockRemoteObjectives(new[] { objectiveRemote });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (_, _, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Remove);
            Assert.AreEqual(0, await Fixture.Context.Objectives.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Objectives.Unsynchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveRemovedFromRemote_RemoveObjectiveFromLocalAndSynchronize()
        {
            // Arrange.
            await ArrangeObjective(true);
            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);

            // Act.
            var (_, _, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
                ItemType = ItemType.File,
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize(true);

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewBimElement_AddBimElementToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            objectiveLocal.BimElements.Add(
                new BimElementObjective
                {
                    BimElement = new BimElement
                    {
                        GlobalID = "guid",
                        ParentName = "1.ifc",
                    },
                });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(1, await Fixture.Context.BimElements.CountAsync());
            Assert.AreEqual(2, await Fixture.Context.BimElementObjectives.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveHasNewBimElement_AddBimElementToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(
                new BimElementExternalDto
                {
                    GlobalID = "guid",
                    ParentName = "1.ifc",
                });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(1, await Fixture.Context.BimElements.CountAsync());
            Assert.AreEqual(2, await Fixture.Context.BimElementObjectives.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesHaveNewSameBimElements_SynchronizeBimElements()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            var element = new BimElement
            {
                GlobalID = "guid",
                ParentName = "1.ifc",
            };
            objectiveLocal.BimElements.Add(new BimElementObjective { BimElement = element });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(mapper.Map<BimElementExternalDto>(element));

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(1, await Fixture.Context.BimElements.CountAsync());
            Assert.AreEqual(2, await Fixture.Context.BimElementObjectives.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectivesHaveNewBimElements_SynchronizeBimElements()
        {
            // Arrange.
            var (objectiveLocal, _, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            objectiveLocal.BimElements.Add(
                new BimElementObjective
                {
                    BimElement = new BimElement
                    {
                        GlobalID = "guid",
                        ParentName = "1.ifc",
                    },
                });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(new BimElementExternalDto
            {
                GlobalID = "external_global_id",
                ParentName = "external_parent_name",
            });

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(2, await Fixture.Context.BimElements.CountAsync());
            Assert.AreEqual(4, await Fixture.Context.BimElementObjectives.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_BimElementRemovedFromLocalObjective_RemoveBimElementFromRemoteObjectiveAndSynchronize()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveSynchronized.BimElements ??= new List<BimElementObjective>();
            var element = new BimElement
            {
                GlobalID = "guid",
                ParentName = "1.ifc",
            };
            objectiveSynchronized.BimElements.Add(new BimElementObjective { BimElement = element });
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();
            objectiveRemote.BimElements ??= new List<BimElementExternalDto>();
            objectiveRemote.BimElements.Add(mapper.Map<BimElementExternalDto>(element));

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(0, await Fixture.Context.BimElements.CountAsync());
            Assert.AreEqual(0, await Fixture.Context.BimElementObjectives.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_BimElementRemovedFromRemoteObjective_RemoveBimElementFromLocalObjectiveAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.BimElements ??= new List<BimElementObjective>();
            objectiveSynchronized.BimElements ??= new List<BimElementObjective>();
            var element = new BimElement
            {
                GlobalID = "guid",
                ParentName = "1.ifc",
            };
            objectiveLocal.BimElements.Add(new BimElementObjective { BimElement = element });
            objectiveSynchronized.BimElements.Add(new BimElementObjective { BimElement = element });
            Fixture.Context.Objectives.Update(objectiveLocal);
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.BimElements.CountAsync());
            Assert.AreEqual(0, await Fixture.Context.BimElementObjectives.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewDynamicFieldWithSubfield_AddDynamicFieldToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();
            objectiveLocal.DynamicFields ??= new List<DynamicField>();
            objectiveLocal.DynamicFields.Add(
                new DynamicField
                {
                    Name = "Big DF",
                    ChildrenDynamicFields = new List<DynamicField>
                    {
                        new DynamicField
                        {
                            Name = "Small DF",
                            Value = "value",
                        },
                    },
                });
            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(4, await Fixture.Context.DynamicFields.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalObjectiveHasNewSubfield_AddDynamicFieldToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            objectiveLocal.DynamicFields ??= new List<DynamicField>();
            objectiveSynchronized.DynamicFields ??= new List<DynamicField>();
            objectiveRemote.DynamicFields ??= new List<DynamicFieldExternalDto>();

            var localField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            localField.ChildrenDynamicFields.Add(MockData.DEFAULT_DYNAMIC_FIELDS[1]);
            objectiveLocal.DynamicFields.Add(localField);

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            localField.SynchronizationMate = synchronizedField;
            objectiveSynchronized.DynamicFields.Add(synchronizedField);

            var remoteField = new DynamicFieldExternalDto
            {
                ExternalID = "ex_field",
                Name = localField.Name,
                Value = localField.Value,
                Type = DynamicFieldType.DATE,
            };
            localField.ExternalID = synchronizedField.ExternalID = remoteField.ExternalID;
            objectiveRemote.DynamicFields.Add(remoteField);

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(4, await Fixture.Context.DynamicFields.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteObjectiveHasNewDynamicField_AddDynamicFieldToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            var field = mapper.Map<DynamicFieldExternalDto>(MockData.DEFAULT_DYNAMIC_FIELDS[0]);
            field.ExternalID = "ex_field";
            objectiveRemote.DynamicFields = new List<DynamicFieldExternalDto> { field };

            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(2, await Fixture.Context.DynamicFields.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_DynamicFieldsHaveChanges_MergeFieldsAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            var localField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            objectiveLocal.DynamicFields = new List<DynamicField> { localField };

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            localField.SynchronizationMate = synchronizedField;
            objectiveSynchronized.DynamicFields = new List<DynamicField> { synchronizedField };

            var remoteField = new DynamicFieldExternalDto
            {
                ExternalID = "ex_field",
                Name = localField.Name,
                Value = localField.Value,
                Type = DynamicFieldType.DATE,
                UpdatedAt = DateTime.UtcNow.AddDays(1),
            };
            localField.ExternalID = synchronizedField.ExternalID = remoteField.ExternalID;
            objectiveRemote.DynamicFields = new List<DynamicFieldExternalDto> { remoteField };

            var newName = localField.Name = "New Name";
            var newValue = remoteField.Value = "New Value";
            var relevantType = (remoteField.Type = DynamicFieldType.FLOAT).ToString();
            var irrelevantType = localField.Type = DynamicFieldType.INTEGER.ToString();

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();
            var dynamicField = synchronized.DynamicFields.First();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(2, await Fixture.Context.DynamicFields.CountAsync());
            Assert.AreEqual(newName, dynamicField.Name);
            Assert.AreEqual(newValue, dynamicField.Value);
            Assert.AreEqual(relevantType, dynamicField.Type);
            Assert.AreNotEqual(irrelevantType, dynamicField.Type);
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_DynamicFieldRemovedFromLocal_RemoveDynamicFieldFromRemoteAndSynchronize()
        {
            // ( ´･･)ﾉ(._.`) TODO: Refactor it.
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            objectiveSynchronized.DynamicFields = new List<DynamicField> { synchronizedField };

            var remoteField = new DynamicFieldExternalDto
            {
                ExternalID = "ex_field",
                Name = synchronizedField.Name,
                Value = synchronizedField.Value,
                Type = DynamicFieldType.DATE,
                UpdatedAt = DateTime.UtcNow.AddDays(1),
            };
            synchronizedField.ExternalID = remoteField.ExternalID;
            objectiveRemote.DynamicFields = new List<DynamicFieldExternalDto> { remoteField };

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(0, await Fixture.Context.DynamicFields.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(ResultObjectiveExternalDto), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_DynamicFieldRemovedFromRemote_RemoveDynamicFieldFromLocalAndSynchronize()
        {
            // ( ´･･)ﾉ(._.`) TODO: Refactor it.
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();

            var localField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            objectiveLocal.DynamicFields = new List<DynamicField> { localField };

            var synchronizedField = MockData.DEFAULT_DYNAMIC_FIELDS[0];
            synchronizedField.IsSynchronized = true;
            localField.SynchronizationMate = synchronizedField;
            objectiveSynchronized.DynamicFields = new List<DynamicField> { synchronizedField };

            localField.ExternalID = synchronizedField.ExternalID = "ex_field";

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.DynamicFields.CountAsync());
            CheckObjectives(synchronized, mapper.Map<Objective>(objectiveRemote), false);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveAddedLocalWithSubobjective_AddObjectivesToRemoteAndSynchronize()
        {
            // Arrange.
            var objectivesLocal = MockData.DEFAULT_OBJECTIVES.Take(2).ToArray();
            objectivesLocal[1].Project = objectivesLocal[0].Project = Project.local;
            objectivesLocal[1].ObjectiveType = objectivesLocal[0].ObjectiveType =
                await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            objectivesLocal[1].ParentObjective = objectivesLocal[0];

            MockRemoteObjectives(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Objectives.AddRangeAsync(objectivesLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();

            var locals = await Fixture.Context.Objectives.Include(x => x.Project)
               .Unsynchronized()
               .ToListAsync();
            var synchronized = await Fixture.Context.Objectives.Include(x => x.Project)
               .Synchronized()
               .ToListAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add, Times.Exactly(2));
            CheckObjectives(locals.First(x => x.Description == objectivesLocal[0].Description), objectivesLocal[0]);
            Assert.AreEqual(2, synchronized.Count);
            Assert.AreEqual(2, locals.Count);
            Assert.AreEqual(
                synchronized.First(x => x.ParentObjectiveID == null).ExternalID,
                ResultObjectiveExternalDtos.First(x => x.ParentObjectiveExternalID != null).ParentObjectiveExternalID);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveAddedLocal_AddObjectiveToRemoteAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, _, _) = await ArrangeObjective();

            var subobjective = MockData.DEFAULT_OBJECTIVES[1];
            objectiveLocal.ChildrenObjectives = new List<Objective> { subobjective };
            subobjective.ProjectID = objectiveLocal.ProjectID;
            subobjective.ObjectiveTypeID = objectiveLocal.ObjectiveTypeID;

            Fixture.Context.Objectives.Update(objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var synchronizationResult = await Synchronize();

            var locals = await Fixture.Context.Objectives.Include(x => x.Project)
               .Unsynchronized()
               .ToListAsync();
            var synchronized = await Fixture.Context.Objectives.Include(x => x.Project)
               .Synchronized()
               .ToListAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
            CheckObjectives(locals.First(x => x.Description == subobjective.Description), subobjective);
            Assert.AreEqual(
                synchronized.First(x => x.ParentObjectiveID == null).ExternalID,
                ResultObjectiveExternalDto.ParentObjectiveExternalID);
            Assert.AreEqual(2, synchronized.Count);
            Assert.AreEqual(2, locals.Count);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_ObjectiveAddedRemoteWithSubobjective_AddObjectivesToLocalAndSynchronize()
        {
            // Arrange.
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemoteParent = new ObjectiveExternalDto
            {
                ExternalID = "external_id1",
                ProjectExternalID = Project.remote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                Title = "Title1",
            };

            var objectiveRemoteChild = new ObjectiveExternalDto
            {
                ExternalID = "external_id2",
                ProjectExternalID = Project.remote.ExternalID,
                ParentObjectiveExternalID = objectiveRemoteParent.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                Title = "Title2",
            };

            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemoteParent });

            // Act.
            var synchronizationResult = await Synchronize();

            var locals = await Fixture.Context.Objectives.Include(x => x.Project)
               .Unsynchronized()
               .ToListAsync();
            var synchronized = await Fixture.Context.Objectives.Include(x => x.Project)
               .Synchronized()
               .ToListAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(2, synchronized.Count);
            Assert.AreEqual(2, locals.Count);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID == null), mapper.Map<Objective>(objectiveRemoteParent), false);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID != null), mapper.Map<Objective>(objectiveRemoteChild), false);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveAddedRemote_AddSubjectiveToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, objectiveRemote) = await ArrangeObjective();
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemoteChild = new ObjectiveExternalDto
            {
                ExternalID = "external_id2",
                ProjectExternalID = Project.remote.ExternalID,
                ParentObjectiveExternalID = objectiveRemote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                Title = "Title2",
            };

            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemote });

            // Act.
            var synchronizationResult = await Synchronize();

            var locals = await Fixture.Context.Objectives.Include(x => x.Project)
               .Unsynchronized()
               .ToListAsync();
            var synchronized = await Fixture.Context.Objectives.Include(x => x.Project)
               .Synchronized()
               .ToListAsync();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(2, synchronized.Count);
            Assert.AreEqual(2, locals.Count);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID == null), mapper.Map<Objective>(objectiveRemote), false);
            CheckObjectives(synchronized.First(x => x.ParentObjectiveID != null), mapper.Map<Objective>(objectiveRemoteChild), false);
            CheckSynchronizedObjectives(
                locals.First(x => x.ParentObjectiveID == null),
                synchronized.First(x => x.ParentObjectiveID == null));
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveRemovedFromLocal_RemoveSubjectiveFromRemoteAndSynchronize()
        {
            // Arrange.
            var (_, objectiveSynchronized, objectiveRemote) = await ArrangeObjective();
            var objectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();

            var objectiveRemoteChild = new ObjectiveExternalDto
            {
                ExternalID = "external_id2",
                ProjectExternalID = Project.remote.ExternalID,
                ParentObjectiveExternalID = objectiveRemote.ExternalID,
                ObjectiveType = new ObjectiveTypeExternalDto { Name = objectiveType.Name },
                Title = "Title2",
            };

            objectiveSynchronized.ChildrenObjectives = new List<Objective>
            {
                new Objective
                {
                    ExternalID = objectiveRemoteChild.ExternalID,
                    Project = objectiveSynchronized.Project,
                    ObjectiveType = objectiveType,
                    Title = objectiveRemoteChild.Title,
                    IsSynchronized = true,
                },
            };

            MockRemoteObjectives(new[] { objectiveRemoteChild, objectiveRemote });
            Fixture.Context.Objectives.Update(objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Remove);
            Assert.AreEqual(2, await Fixture.Context.Objectives.CountAsync());
            Assert.AreEqual(objectiveRemoteChild.ExternalID, ResultObjectiveExternalDto.ExternalID);
            CheckSynchronizedObjectives(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_SubobjectiveRemovedFromRemote_RemoveSubjectiveFromLocalAndSynchronize()
        {
            // Arrange.
            var (objectiveLocal, objectiveSynchronized, _) = await ArrangeObjective();

            var subobjectiveLocal = MockData.DEFAULT_OBJECTIVES[1];
            objectiveLocal.ChildrenObjectives = new List<Objective> { subobjectiveLocal };
            subobjectiveLocal.ProjectID = objectiveLocal.ProjectID;
            subobjectiveLocal.ObjectiveTypeID = objectiveLocal.ObjectiveTypeID;

            var subobjectiveSynchronized = MockData.DEFAULT_OBJECTIVES[1];
            objectiveSynchronized.ChildrenObjectives = new List<Objective> { subobjectiveSynchronized };
            subobjectiveSynchronized.ProjectID = objectiveLocal.ProjectID;
            subobjectiveSynchronized.ObjectiveTypeID = objectiveLocal.ObjectiveTypeID;
            subobjectiveSynchronized.IsSynchronized = true;

            subobjectiveLocal.SynchronizationMate = subobjectiveSynchronized;
            var removingID = subobjectiveLocal.ExternalID = subobjectiveSynchronized.ExternalID = "ex_subobjective";

            Fixture.Context.Objectives.UpdateRange(objectiveLocal, objectiveSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetObjectivesAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(2, await Fixture.Context.Objectives.CountAsync());
            Assert.AreNotEqual(removingID, local.ExternalID);
            Assert.AreNotEqual(removingID, synchronized.ExternalID);
            CheckSynchronizedObjectives(local, synchronized);
        }

        private void CheckSynchronizedObjectives(Objective local, Objective synchronized)
        {
            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);

            if (local.ParentObjective != null || synchronized.ParentObjective != null)
                Assert.AreEqual(local.ParentObjective?.SynchronizationMateID, synchronized.ParentObjectiveID);

            Assert.AreEqual(local.ChildrenObjectives?.Count ?? 0, synchronized.ChildrenObjectives?.Count ?? 0);
            CheckObjectives(local, synchronized, false);

            foreach (var item in local.ChildrenObjectives ?? Enumerable.Empty<Objective>())
            {
                var synchronizedItem = synchronized.ChildrenObjectives?
                   .FirstOrDefault(x => item.SynchronizationMateID == x.ID);
                Assert.AreNotEqual(null, synchronizedItem);
                CheckSynchronizedObjectives(item, synchronizedItem);
            }

            foreach (var item in local.Items ?? Enumerable.Empty<ObjectiveItem>())
            {
                var synchronizedItem = synchronized.Items?
                   .FirstOrDefault(x => item.Item.SynchronizationMateID == x.ItemID);
                Assert.AreNotEqual(null, synchronizedItem);
                SynchronizerTestsHelper.CheckSynchronizedItems(item.Item, synchronizedItem?.Item);
            }

            foreach (var item in local.DynamicFields ?? Enumerable.Empty<DynamicField>())
            {
                var synchronizedItem = synchronized.DynamicFields?
                   .FirstOrDefault(x => item.SynchronizationMateID == x.ID);
                Assert.AreNotEqual(null, synchronizedItem);
                CheckSynchronizedDynamicFields(item, synchronizedItem);
            }
        }

        private void CheckObjectives(Objective a, Objective b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Project.ExternalID, b.Project.ExternalID);
            Assert.AreEqual(a.AuthorID, b.AuthorID);
            Assert.AreEqual(a.ObjectiveType.Name, b.ObjectiveType.Name);
            Assert.AreEqual(a.CreationDate, b.CreationDate);
            Assert.AreEqual(a.DueDate, b.DueDate);
            Assert.AreEqual(a.Title, b.Title);
            Assert.AreEqual(a.Description, b.Description);
            Assert.AreEqual(a.Status, b.Status);
            Assert.AreEqual(a.BimElements?.Count ?? 0, b.BimElements?.Count ?? 0);
            Assert.AreEqual(a.Items?.Count ?? 0, b.Items?.Count ?? 0);
            Assert.AreEqual(a.DynamicFields?.Count ?? 0, b.DynamicFields?.Count ?? 0);

            foreach (var bimElement in a.BimElements ?? Enumerable.Empty<BimElementObjective>())
            {
                var synchronizedElement = b.BimElements?.FirstOrDefault(
                    x => bimElement.BimElement.ParentName == x.BimElement.ParentName &&
                        bimElement.BimElement.GlobalID == x.BimElement.GlobalID);
                Assert.AreNotEqual(null, synchronizedElement);
                CheckSynchronizedBimElements(bimElement.BimElement, bimElement.BimElement);
            }

            if (checkIDs)
            {
                SynchronizerTestsHelper.CheckIDs(a, b);
            }
        }

        private void CheckSynchronizedBimElements(BimElement local, BimElement synchronized)
        {
            Assert.AreEqual(local.ElementName, synchronized.ElementName);
            Assert.AreEqual(local.ParentName, synchronized.ParentName);
            Assert.AreEqual(local.GlobalID, synchronized.GlobalID);
        }

        private void CheckSynchronizedDynamicFields(DynamicField local, DynamicField synchronized)
        {
            Assert.AreEqual(local.Name, synchronized.Name);
            Assert.AreEqual(local.Type, synchronized.Type);
            Assert.AreEqual(local.Value, synchronized.Value);

            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(local.ChildrenDynamicFields?.Count ?? 0, synchronized.ChildrenDynamicFields?.Count ?? 0);

            foreach (var item in local.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>())
            {
                var synchronizedItem = synchronized.ChildrenDynamicFields?
                   .FirstOrDefault(x => item.SynchronizationMateID == x.ID);
                Assert.AreNotEqual(null, synchronizedItem);
                CheckSynchronizedDynamicFields(item, synchronizedItem);
            }
        }

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(ObjectiveSynchronizer, call, times);

        private async Task<(Objective local, Objective synchronized, ObjectiveExternalDto remote)> ArrangeObjective(bool dontSetupRemote = false)
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

            if (!dontSetupRemote)
                MockRemoteObjectives(new[] { objectiveRemote });

            objectiveSynchronized.IsSynchronized = true;
            objectiveLocal.SynchronizationMate = objectiveSynchronized;
            await Fixture.Context.Objectives.AddRangeAsync(objectiveSynchronized, objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            return (objectiveLocal, objectiveSynchronized, objectiveRemote);
        }

        private static async
            Task<(Objective local, Objective synchronized, ICollection<SynchronizingResult> synchronizationResult)>
            GetObjectivesAfterSynchronize(bool ignoreProjects = false)
        {
            var synchronizationResult = await Synchronize(ignoreProjects);
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstOrDefaultAsync();
            return (local, synchronized, synchronizationResult);
        }

        private static async Task<ICollection<SynchronizingResult>> Synchronize(bool ignoreProjects = false)
        {
            var data = new SynchronizingData
            {
                Context = Fixture.Context,
                User = await Fixture.Context.Users.FirstAsync(),
                Mapper = mapper,
            };

            if (ignoreProjects)
                data.ProjectsFilter = x => false;

            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfoExternalDto());
            return synchronizationResult;
        }

        private void MockRemoteObjectives(IReadOnlyCollection<ObjectiveExternalDto> array)
            => SynchronizerTestsHelper.MockGetRemote(ObjectiveSynchronizer, array, x => x.ExternalID);
    }
}
