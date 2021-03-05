using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
    public class SynchronizerProjectTests
    {
        private static Synchronizer synchronizer;
        private static IMapper mapper;

        private static Mock<ISynchronizer<ObjectiveExternalDto>> ObjectiveSynchronizer { get; set; }

        private static Mock<ISynchronizer<ProjectExternalDto>> ProjectSynchronizer { get; set; }

        private static SharedDatabaseFixture Fixture { get; set; }

        private static Mock<IConnection> Connection { get; set; }

        private static Mock<IConnectionContext> Context { get; set; }

        private static ProjectExternalDto ResultProjectExternalDto { get; set; }

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            mapper = mapperConfig.CreateMapper();
            synchronizer = new Synchronizer(mapper);
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
               .Returns<ProjectExternalDto>(Task.FromResult)
               .Callback<ProjectExternalDto>(x => ResultProjectExternalDto = x);
            ProjectSynchronizer.Setup(x => x.Update(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult)
               .Callback<ProjectExternalDto>(x => ResultProjectExternalDto = x);
            ProjectSynchronizer.Setup(x => x.Remove(It.IsAny<ProjectExternalDto>()))
               .Returns<ProjectExternalDto>(Task.FromResult)
               .Callback<ProjectExternalDto>(x => ResultProjectExternalDto = x);
            Context.Setup(x => x.ObjectivesSynchronizer).Returns(ObjectiveSynchronizer.Object);
            Context.Setup(x => x.ProjectsSynchronizer).Returns(ProjectSynchronizer.Object);
            Context.Setup(x => x.Objectives).ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Synchronize_ProjectUnchanged_DoNothing()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);

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
            var (projectLocal, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            projectRemote.Title = projectLocal.Title = "New same title";
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
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
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
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            projectLocal.Title = "New title";
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
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRenamedRemote_RenameLocalProjectAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            var oldTitle = projectLocal.Title;
            projectRemote.Title = "New title";
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
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            projectLocal.Title = "Local title";
            var oldLocalTitle = projectLocal.Title;
            projectRemote.Title = "Remote title";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(1);
            var oldRemoteTitle = projectRemote.Title;
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
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            projectLocal.Title = "Local title";
            var oldLocalTitle = projectLocal.Title;
            projectRemote.Title = "Remote title";
            projectRemote.UpdatedAt = DateTime.UtcNow.AddDays(-1);
            var oldRemoteTitle = projectRemote.Title;
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
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalProjectHasNewItem_AddItemToRemoteAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
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
            Assert.AreEqual(1, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteProjectHasNewItem_AddItemToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, projectExternal) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            projectExternal.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    ExternalID = "item_external_id",
                    Name = "item_name",
                    ItemType = ItemTypeDto.File,
                    UpdatedAt = DateTime.UtcNow,
                },
            };

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
            Assert.AreEqual(1, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
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

        private void CheckProjects(Project a, Project b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Title, b.Title);

            if (checkIDs)
            {
                SynchronizerTestsHelper.CheckIDs(a, b);
            }
        }

        private void CheckSynchronizedProjects(Project local, Project synchronized)
        {
            CheckProjects(local, synchronized, false);

            SynchronizerTestsHelper.CheckSynchronized(local, synchronized);

            Assert.AreEqual(local.Items?.Count ?? 0, synchronized.Items?.Count ?? 0);
            Assert.AreEqual(local.Objectives?.Count ?? 0, synchronized.Objectives?.Count ?? 0);
            Assert.AreEqual(local.Users?.Count ?? 0, synchronized.Users?.Count ?? 0);
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);

            foreach (var item in local.Items ?? Enumerable.Empty<Item>())
            {
                var synchronizedItem = synchronized.Items?
                   .FirstOrDefault(x => item.ExternalID == x.ExternalID || item.SynchronizationMateID == x.ID);
                SynchronizerTestsHelper.CheckSynchronizedItems(item, synchronizedItem);
            }
        }
    }
}
