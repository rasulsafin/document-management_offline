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
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult);
            ProjectSynchronizer.Setup(x => x.Update(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult);
            ProjectSynchronizer.Setup(x => x.Remove(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult);
            ObjectiveSynchronizer.Setup(x => x.Add(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult);
            ObjectiveSynchronizer.Setup(x => x.Update(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult);
            ObjectiveSynchronizer.Setup(x => x.Remove(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(Task.FromResult);
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
            // Arrange.
            var (projectLocal, projectSynchronized, projectRemote) = await ArrangeProject();
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, projectSynchronized);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteProjectsSame_Synchronize()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, projectRemote) = await ArrangeProject();
            projectRemote.Title = projectLocal.Title = "New same title";
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            Fixture.Context.Projects.Update(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
            CheckProjects(local, projectLocal);
            CheckProjects(synchronized, projectSynchronized);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedLocal_AddProjectToRemoteAndSynchronize()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            Context.Setup(x => x.Projects).ReturnsAsync(ArraySegment<ProjectExternalDto>.Empty);
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Once);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
            CheckProjects(local, projectLocal);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectAddedRemote_AddProjectToLocalAndSynchronize()
        {
            // Arrange.
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = "Title",
                UpdatedAt = DateTime.Now,
            };
            Context.Setup(x => x.Projects).ReturnsAsync(new[] { projectRemote });
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedLocal_RenameRemoteProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await ArrangeProject();
            projectLocal.Title = "New title";
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            Fixture.Context.Projects.Update(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Once);
            CheckProjects(local, projectLocal);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedRemote_RenameLocalProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await ArrangeProject();
            var oldTitle = projectLocal.Title;
            projectRemote.Title = "New title";
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
            Assert.AreNotEqual(oldTitle, local.Title);
            Assert.AreNotEqual(oldTitle, synchronized.Title);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedLocalThenRemote_RenameLocalProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await ArrangeProject();
            projectLocal.Title = "Local title";
            var oldLocalTitle = projectLocal.Title;
            projectRemote.Title = "Remote title";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(1);
            var oldRemoteTitle = projectRemote.Title;
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            Fixture.Context.Projects.Update(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
            Assert.AreNotEqual(oldLocalTitle, local.Title);
            Assert.AreEqual(oldRemoteTitle, synchronized.Title);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedRemoteThenLocal_RenameRemoteProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await ArrangeProject();
            projectLocal.Title = "Local title";
            var oldLocalTitle = projectLocal.Title;
            projectRemote.Title = "Remote title";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            var oldRemoteTitle = projectRemote.Title;
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            Fixture.Context.Projects.Update(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Once);
            Assert.AreEqual(oldLocalTitle, local.Title);
            Assert.AreNotEqual(oldRemoteTitle, synchronized.Title);
            Assert.AreNotEqual(projectRemote.Title, synchronized.Title);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalProjectHasNewItem_AddItemToRemoteAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await ArrangeProject();
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.SaveChangesAsync();

            // Act.
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

            // Assert.
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Once);
            Assert.AreEqual(await Fixture.Context.Items.Synchronized().CountAsync(), 1);
            Assert.AreEqual(await Fixture.Context.Items.Unsynchronized().CountAsync(), 1);
            CheckSynchronizedProjects(local, synchronized);
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
            // Arrange.
            var project = await ArrangeProject();
            var objectiveType = MockData.DEFAULT_OBJECTIVE_TYPES[0];

            var objectiveLocal = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveSynchronized = MockData.DEFAULT_OBJECTIVES[0];
            var objectiveRemote = new ObjectiveExternalDto
            {
                ExternalID = "external_id",
                ProjectExternalID = "project_external_id",
                ParentObjectiveExternalID = string.Empty,
                AuthorExternalID = "author_external_id",
                ObjectiveType = new ObjectiveTypeExternalDto() { Name = objectiveType.Name },
                UpdatedAt = DateTime.Now,
                CreationDate = objectiveLocal.CreationDate,
                DueDate = objectiveLocal.DueDate,
                Title = objectiveLocal.Title,
                Description = objectiveLocal.Description,
                Status = (ObjectiveStatus)objectiveLocal.Status,
            };

            objectiveLocal.ExternalID = objectiveSynchronized.ExternalID = objectiveRemote.ExternalID;
            objectiveLocal.Project = project.local;
            objectiveSynchronized.Project = project.synchronized;
            Context.Setup(x => x.Objectives).ReturnsAsync(new[] { objectiveRemote });
            objectiveSynchronized.IsSynchronized = true;
            objectiveLocal.SynchronizationMate = objectiveSynchronized;
            await Fixture.Context.Objectives.AddRangeAsync(objectiveSynchronized, objectiveLocal);
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
            var local = await Fixture.Context.Objectives.Unsynchronized().FirstAsync();
            var synchronized = await Fixture.Context.Objectives.Synchronized().FirstAsync();

            // Assert.
            ObjectiveSynchronizer.Verify(x => x.Add(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Remove(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            ObjectiveSynchronizer.Verify(x => x.Update(It.IsAny<ObjectiveExternalDto>()), Times.Never);
            CheckProjects(local, objectiveLocal);
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

        private void CheckProjects(Project a, Project b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Title, b.Title);

            if (checkIDs)
            {
                Assert.AreEqual(a.SynchronizationMateID, b.SynchronizationMateID);
                Assert.AreEqual(a.IsSynchronized, b.IsSynchronized);
            }
        }

        private void CheckSynchronizedProjects(Project local, Project synchronized)
        {
            CheckProjects(local, synchronized, false);

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

        private async Task<(Project local, Project synchronized, ProjectExternalDto remote)> ArrangeProject()
        {
            // Arrange.
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
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await Fixture.Context.Projects.AddRangeAsync(projectSynchronized, projectLocal);
            await Fixture.Context.SaveChangesAsync();
            return (projectLocal, projectSynchronized, projectRemote);
        }
    }
}
