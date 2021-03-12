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
        public static void ClassSetup(TestContext unused)
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
            Connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>(), It.IsAny<DateTime>()))
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Add);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ProjectRemovedLocal_RemoveProjectFromRemoteAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            Fixture.Context.Remove(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Remove);
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ProjectRemovedRemote_RemoveProjectFromLocalAndSynchronize()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var projectSynchronized = MockData.DEFAULT_PROJECTS[0];
            projectLocal.ExternalID = projectSynchronized.ExternalID = "external_id";
            Context.Setup(x => x.Projects).ReturnsAsync(ArraySegment<ProjectExternalDto>.Empty);
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await Fixture.Context.Projects.AddRangeAsync(projectSynchronized, projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Projects.Synchronized().CountAsync());
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
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
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(1, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_RemoteProjectHasNewItem_AddItemToLocalAndSynchronize()
        {
            // Arrange.
            var (_, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            projectRemote.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    ExternalID = "item_external_id",
                    FileName = "item_name",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                },
            };

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(1, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckProjects(synchronized, mapper.Map<Project>(projectRemote), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_LocalAndRemoteProjectsHaveNewItems_AddItemsAndSynchronize()
        {
            // Arrange.
            var (projectLocal, _, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.SaveChangesAsync();
            projectRemote.Items = new List<ItemExternalDto>
            {
                new ItemExternalDto
                {
                    ExternalID = "item_external_id",
                    FileName = "item_name",
                    ItemType = ItemType.File,
                    UpdatedAt = DateTime.UtcNow,
                },
            };

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(2, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(2, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromLocalProject_RemoveItemFromRemoteAndSynchronize()
        {
            // Arrange.
            var (_, projectSynchronized, projectRemote) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            var itemExternal = new ItemExternalDto
            {
                ExternalID = "item_external_id",
                FileName = "item_name",
                ItemType = ItemType.File,
                UpdatedAt = DateTime.UtcNow,
            };
            projectRemote.Items = new List<ItemExternalDto> { itemExternal };
            var item = MockData.DEFAULT_ITEMS[0];
            item.IsSynchronized = true;
            item.ProjectID = projectSynchronized.ID;
            item.ExternalID = itemExternal.ExternalID;
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Update);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Items.Unsynchronized().CountAsync());
            CheckProjects(synchronized, mapper.Map<Project>(ResultProjectExternalDto), false);
            CheckSynchronizedProjects(local, synchronized);
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteProjectAndItemIsNeeded_UnlinkLocalItemAndSynchronize()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, _) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            item.Objectives ??= new List<ObjectiveItem>();
            var objective = MockData.DEFAULT_OBJECTIVES[0];
            item.Objectives.Add(new ObjectiveItem
            {
                Item = item,
                Objective = objective,
            });
            objective.Project = projectLocal;
            objective.ObjectiveType = await Fixture.Context.ObjectiveTypes.FirstOrDefaultAsync();
            var itemSynchronized = MockData.DEFAULT_ITEMS[0];
            itemSynchronized.IsSynchronized = true;
            itemSynchronized.Project = projectSynchronized;
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.Items.AddAsync(itemSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize(true);

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Items.Unsynchronized().CountAsync());
        }

        [TestMethod]
        public async Task Synchronize_ItemRemovedFromRemoteProjectAndItemIsNotNeeded_RemoveItemAndSynchronize()
        {
            // Arrange.
            var (projectLocal, projectSynchronized, _) = await SynchronizerTestsHelper.ArrangeProject(Context, Fixture);
            var item = MockData.DEFAULT_ITEMS[0];
            item.Project = projectLocal;
            var itemSynchronized = MockData.DEFAULT_ITEMS[0];
            itemSynchronized.IsSynchronized = true;
            itemSynchronized.Project = projectSynchronized;
            await Fixture.Context.Items.AddAsync(item);
            await Fixture.Context.Items.AddAsync(itemSynchronized);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (_, _, synchronizationResult) = await GetProjectsAfterSynchronize();

            // Assert.
            Assert.AreEqual(0, synchronizationResult.Count);
            CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall.Nothing);
            Assert.AreEqual(0, await Fixture.Context.Items.Synchronized().CountAsync());
            Assert.AreEqual(0, await Fixture.Context.Items.Unsynchronized().CountAsync());
        }

        private void CheckProjects(Project a, Project b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Title, b.Title);

            if (checkIDs)
                SynchronizerTestsHelper.CheckIDs(a, b);
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

        private void CheckSynchronizerCalls(SynchronizerTestsHelper.SynchronizerCall call, Times times = default)
            => SynchronizerTestsHelper.CheckSynchronizerCalls(ProjectSynchronizer, call, times);

        private static async
            Task<(Project local, Project synchronized, ICollection<SynchronizingResult> synchronizationResult)>
            GetProjectsAfterSynchronize(bool ignoreObjectives = false)
        {
            var data = new SynchronizingData
            {
                Context = Fixture.Context,
                User = await Fixture.Context.Users.FirstAsync(),
            };

            if (ignoreObjectives)
                data.ObjectivesFilter = x => false;

            var synchronizationResult = await synchronizer.Synchronize(
                data,
                Connection.Object,
                new ConnectionInfo());

            var local = await Fixture.Context.Projects.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await Fixture.Context.Projects.Synchronized().FirstOrDefaultAsync();
            return (local, synchronized, synchronizationResult);
        }
    }
}
