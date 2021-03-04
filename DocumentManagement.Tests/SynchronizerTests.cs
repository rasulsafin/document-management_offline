using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Connection;
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
    public class SynchronizerTests
    {
        private static Synchronizer synchronizer;
        private static IMapper mapper;

        private static Mock<ISynchronizer<ObjectiveExternalDto>> ObjectiveSynchronizer { get; set; }

        private static Mock<ISynchronizer<ProjectExternalDto>> ProjectSynchronizer { get; set; }

        private static SharedDatabaseFixture Fixture { get; set; }

        private static Mock<IConnection> Connection { get; set; }

        private static Mock<IConnectionContext> Context { get; set; }

        [ClassInitialize]
        public static void ClassSetup(TestContext unused)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            mapper = mapperConfig.CreateMapper();
        }

        [TestInitialize]
        public void Setup()
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
            Context.Setup(x => x.ObjectivesSynchronizer).Returns(ObjectiveSynchronizer.Object);
            Context.Setup(x => x.ProjectsSynchronizer).Returns(ProjectSynchronizer.Object);

            synchronizer = new Synchronizer(mapper);
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Synchronize_ProjectUnchanged_DoNothing()
        {
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var projectSynchronized = MockData.DEFAULT_PROJECTS[0];
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = projectLocal.Title,
                UpdatedAt = DateTime.Now,
            };
            projectLocal.ExternalID = projectSynchronized.ExternalID = projectRemote.ExternalID;
            Context.Setup(x => x.Projects).ReturnsAsync(new[] { projectRemote });
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await Fixture.Context.Projects.AddAsync(projectSynchronized);
            await Fixture.Context.SaveChangesAsync();
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User =  await Fixture.Context.Users.FirstAsync(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = project => true,
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Projects.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Projects.Synchronized().FirstAsync();

            ProjectSynchronizer.Verify(x => x.Add(new ProjectExternalDto()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(new ProjectExternalDto()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(new ProjectExternalDto()), Times.Never);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, projectSynchronized);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteProjectsSame_Synchronize()
        {
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            projectLocal.Title = "New same title";
            var projectSynchronized = MockData.DEFAULT_PROJECTS[0];
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = projectLocal.Title,
                UpdatedAt = DateTime.Now,
            };
            projectLocal.ExternalID = projectSynchronized.ExternalID = projectRemote.ExternalID;
            Context.Setup(x => x.Projects).ReturnsAsync(new[] { projectRemote });
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await Fixture.Context.Projects.AddAsync(projectSynchronized);
            await Fixture.Context.SaveChangesAsync();
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User =  await Fixture.Context.Users.FirstAsync(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = project => true,
                },
                Connection.Object,
                new ConnectionInfoDto());

            var local = await Fixture.Context.Projects.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Projects.Synchronized().FirstAsync();

            ProjectSynchronizer.Verify(x => x.Add(new ProjectExternalDto()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(new ProjectExternalDto()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(new ProjectExternalDto()), Times.Never);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, projectSynchronized);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedLocal_AddProjectToRemoteAndSynchronize()
        {
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = projectLocal.Title,
                UpdatedAt = DateTime.Now,
            };
            Context.Setup(x => x.Projects).ReturnsAsync(ArraySegment<ProjectExternalDto>.Empty);
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>())).ReturnsAsync(projectRemote);
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                    ObjectivesFilter = objective => true,
                    ProjectsFilter = project => true,
                },
                Connection.Object,
                new ConnectionInfoDto());
            var local = await Fixture.Context.Projects.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Projects.Synchronized().FirstAsync();

            ProjectSynchronizer.Verify(x => x.Add(new ProjectExternalDto()), Times.Once);
            ProjectSynchronizer.Verify(x => x.Remove(new ProjectExternalDto()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(new ProjectExternalDto()), Times.Never);
            CheckProjects(local, projectLocal);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedRemote_AddProjectToLocalAndSynchronize()
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
        public async Task Synchronize_ProjectRenamedLocal_RenameLocalProjectAndSynchronize()
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
        public async Task Synchronize_ProjectRenamedRemote_RenameRemoteProjectAndSynchronize()
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
        public async Task Synchronize_ProjectRenamedLocalAndRemote_RenameRemoteProjectAndSynchronize()
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
        public async Task Synchronize_ProjectRenamedLocalAndRemote_RenameLocalProjectAndSynchronize()
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
        public async Task Synchronize_LocalProjectHasNewItem_AddItemToRemoteAndSynchronize()
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
        public async Task Synchronize_RemoteProjectHasNewItem_AddItemToLocalAndSynchronize()
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
        public async Task Synchronize_LocalAndRemoteProjectsHaveNewItems_AddItemsAndSynchronize()
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
        public async Task Synchronize_ItemRemovedFromLocalProject_RemoveItemFromRemoteAndSynchronize()
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
        public async Task Synchronize_ItemRemovedFromRemoteProjectItemIsNeeded_UnlinkLocalItemAndSynchronize()
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
        public async Task Synchronize_ItemRemovedFromRemoteProjectItemIsNotNeeded_RemoveItemAndSynchronize()
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
        public async Task Synchronize_ObjectivesUnchanged_DoNothing()
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

        private void CheckProjects(Project a, Project b, bool isSynchronized = false)
        {
            Assert.AreEqual(a.Title, b.Title);

            if (!isSynchronized)
            {
                Assert.AreEqual(a.SynchronizationMateID, b.SynchronizationMateID);
                Assert.AreEqual(a.IsSynchronized, b.IsSynchronized);
            }
        }

        private void CheckSynchronizedProjects(Project local, Project synchronized)
        {
            CheckProjects(local, synchronized, true);

            Assert.AreEqual(local.SynchronizationMateID, synchronized.ID);
            Assert.IsFalse(local.IsSynchronized);
            Assert.IsTrue(synchronized.IsSynchronized);
            Assert.AreEqual(local.Items?.Count ?? 0, synchronized.Items?.Count ?? 0);
            Assert.AreEqual(local.Objectives?.Count ?? 0, synchronized.Objectives?.Count ?? 0);
            Assert.AreEqual(local.Users?.Count ?? 0, synchronized.Users?.Count ?? 0);
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);

            foreach (var item in local.Items ?? Enumerable.Empty<Item>())
            {
                var synchronizedItem = synchronized.Items?
                   .FirstOrDefault(x => item.ExternalID == x.ExternalID || item.SynchronizationMateID == x.ID);
                CheckSynchronizedItems(item, synchronizedItem);
            }
        }

        private void CheckSynchronizedItems(Item local, Item synchronized)
        {
            Assert.AreEqual(local.Name, synchronized.Name);
            Assert.AreEqual(local.Project.SynchronizationMateID, synchronized.Project.ID);
            Assert.AreEqual(local.SynchronizationMateID, synchronized.ID);
            Assert.IsFalse(local.IsSynchronized);
            Assert.IsTrue(synchronized.IsSynchronized);
            Assert.AreEqual(local.ItemType, synchronized.ItemType);
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);
        }
    }
}
