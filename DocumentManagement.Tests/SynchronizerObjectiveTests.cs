using System;
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
            var resolver = new ObjectiveExternalDtoProjectIdResolver(Fixture.Context);
            services.AddTransient<ObjectiveExternalDtoProjectIdResolver>(x => resolver);
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
                Status = (ObjectiveStatus)objectiveLocal.Status,
            };

            objectiveLocal.ExternalID = objectiveSynchronized.ExternalID = objectiveRemote.ExternalID;
            objectiveLocal.Project = Project.local;
            objectiveLocal.ObjectiveType = objectiveType;
            objectiveSynchronized.ObjectiveType = objectiveType;
            objectiveSynchronized.Project = Project.synchronized;
            Context.Setup(x => x.Objectives).ReturnsAsync(new[] { objectiveRemote });
            objectiveSynchronized.IsSynchronized = true;
            objectiveLocal.SynchronizationMate = objectiveSynchronized;
            await Fixture.Context.Objectives.AddRangeAsync(objectiveSynchronized, objectiveLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = p => true,
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
        public async Task Synchronize_ObjectiveAddedLocal_AddProjectToRemoteAndSynchronize()
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
        public async Task Synchronize_ObjectiveAddedRemote_AddProjectToLocalAndSynchronize()
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
        public async Task Synchronize_ObjectiveHasLocalChanges_ApplyLocalChangesToRemoteAndSynchronize()
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
        public async Task Synchronize_ObjectiveHasRemoteChanges_ApplyRemoteChangesToRemoteAndSynchronize()
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
        public async Task Synchronize_ObjectivesHaveChanges_MergeObjectivesAndSynchronize()
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
        public async Task Synchronize_LocalObjectiveHasNewItem_AddItemToRemoteAndSynchronize()
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
        public async Task Synchronize_RemoteObjectiveHasNewItem_AddItemToLocalAndSynchronize()
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
        public async Task Synchronize_LocalAndRemoteObjectiveHaveNewItems_AddItemsAndSynchronize()
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
        public async Task Synchronize_ItemRemovedFromLocalObjective_RemoveItemFromRemoteAndSynchronize()
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
        public async Task Synchronize_ItemRemovedFromRemoteObjectiveAndItemIsNeeded_UnlinkLocalItemAndSynchronize()
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
        public async Task Synchronize_ItemRemovedFromRemoteObjectiveAndItemIsNotNeeded_RemoveItemAndSynchronize()
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
        public async Task Synchronize_LocalAndRemoteObjectiveChangeParent_ChangeParentAtRemoteAndSynchronizedObjectives()
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
                   .FirstOrDefault(x => item.ItemID == x.ItemID);
                SynchronizerTestsHelper.CheckSynchronizedItems(item.Item, synchronizedItem.Item);
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
    }
}
