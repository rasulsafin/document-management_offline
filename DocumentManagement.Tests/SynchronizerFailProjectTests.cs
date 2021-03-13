using System;
using System.Collections.Generic;
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
    public class SynchronizerFailProjectTests
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
            synchronizer = new Synchronizer();
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

            ProjectSynchronizer = new Mock<ISynchronizer<ProjectExternalDto>>();
            ObjectiveSynchronizer = new Mock<ISynchronizer<ObjectiveExternalDto>>();

            Context.Setup(x => x.ObjectivesSynchronizer).Returns(ObjectiveSynchronizer.Object);
            Context.Setup(x => x.ProjectsSynchronizer).Returns(ProjectSynchronizer.Object);
            ObjectiveSynchronizer.Setup(x => x.Get(It.IsAny<List<string>>()))
               .ReturnsAsync(ArraySegment<ObjectiveExternalDto>.Empty);
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task SynchronizeFail_ProjectAddedLocal_ButContextFail_DoNothing()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            ProjectSynchronizer
               .Setup(x => x.GetUpdatedIDs(It.IsAny<DateTime>()))
               .ReturnsAsync(new[] { "id" });
            ProjectSynchronizer
               .Setup(x => x.Get(It.IsAny<IReadOnlyCollection<string>>()))
               .Throws(new Exception());
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await Fixture.Context.SaveChangesAsync();

            // Act.
            var (local, synchronized, result) = await SynchronizingResults();

            // Assert.
            CheckSynchronizer();
            CheckProjects(local, projectLocal);
            Assert.IsNull(synchronized);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task SynchronizeFail_ProjectAddedLocal_ButCouldNotAddToRemote_DoNothing()
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            ProjectSynchronizer
               .Setup(x => x.GetUpdatedIDs(It.IsAny<DateTime>()))
               .ReturnsAsync(ArraySegment<string>.Empty);
            ProjectSynchronizer
               .Setup(x => x.Get(It.IsAny<IReadOnlyCollection<string>>()))
               .ReturnsAsync(ArraySegment<ProjectExternalDto>.Empty);
            await Fixture.Context.Projects.AddAsync(projectLocal);
            await Fixture.Context.SaveChangesAsync();
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>()))
               .Throws(new Exception());

            // Act.
            var (local, synchronized, result) = await SynchronizingResults();

            // Assert.
            CheckSynchronizer();
            CheckProjects(local, projectLocal);
            Assert.IsNull(synchronized);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task SynchronizeFail_ProjectRemovedLocal_ButCouldNotRemoveFromRemote_DoNothing()
        {
            // Arrange.
            var (projectLocal, _, _) = await SynchronizerTestsHelper.ArrangeProject(ProjectSynchronizer, Fixture);
            Fixture.Context.Remove(projectLocal);
            await Fixture.Context.SaveChangesAsync();
            ProjectSynchronizer.Setup(x => x.Add(It.IsAny<ProjectExternalDto>()))
                .Throws(new Exception());

            // Act.
            await SynchronizingResults();

            // Assert.
            CheckSynchronizer();
            Assert.AreEqual(1, await Fixture.Context.Projects.Synchronized().CountAsync());
            Assert.AreEqual(1, await Fixture.Context.Projects.Synchronized().CountAsync());
        }

        private static async Task<(Project local, Project synchronized, ICollection<SynchronizingResult> result)> SynchronizingResults()
        {
            var result = await synchronizer.Synchronize(
                new SynchronizingData
                {
                    Context = Fixture.Context,
                    User = await Fixture.Context.Users.FirstAsync(),
                },
                Connection.Object,
                new ConnectionInfo());
            var local = await Fixture.Context.Projects.Unsynchronized().FirstOrDefaultAsync();
            var synchronized = await Fixture.Context.Projects.Synchronized().FirstOrDefaultAsync();
            return (local, synchronized, result);
        }

        private void CheckProjects(Project a, Project b, bool checkIDs = true)
        {
            Assert.AreEqual(a.Title, b.Title);

            if (checkIDs)
                SynchronizerTestsHelper.CheckIDs(a, b);
        }

        private void CheckSynchronizer()
        {
            ProjectSynchronizer.Verify(x => x.Add(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Remove(It.IsAny<ProjectExternalDto>()), Times.Never);
            ProjectSynchronizer.Verify(x => x.Update(It.IsAny<ProjectExternalDto>()), Times.Never);
        }
    }
}
