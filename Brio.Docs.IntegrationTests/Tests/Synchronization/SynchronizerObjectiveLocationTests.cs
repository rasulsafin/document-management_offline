using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Tests.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests.Synchronization
{
    [TestClass]
    public class SynchronizerObjectiveLocationTests : IDisposable
    {
        private SharedDatabaseFixture fixture;
        private IMapper mapper;
        private ServiceProvider serviceProvider;
        private Synchronizer synchronizer;
        private Mock<IConnection> connection;

        [TestInitialize]
        public async Task Setup()
        {
            fixture = SynchronizerTestsHelper.CreateFixture();
            serviceProvider = SynchronizerTestsHelper.CreateServiceProvider(fixture.Context);
            synchronizer = serviceProvider.GetService<Synchronizer>();
            mapper = serviceProvider.GetService<IMapper>();

            connection = new Mock<IConnection>();
            var projectSynchronizer = SynchronizerTestsHelper.CreateSynchronizerStub<ProjectExternalDto>();
            var objectiveSynchronizer = SynchronizerTestsHelper.CreateSynchronizerStub<ObjectiveExternalDto>();
            var context = SynchronizerTestsHelper.CreateConnectionContextStub(projectSynchronizer.Object, objectiveSynchronizer.Object);
            connection.Setup(x => x.GetContext(It.IsAny<ConnectionInfoExternalDto>())).ReturnsAsync(context.Object);
            var projects = await SynchronizerTestsHelper.ArrangeProject(projectSynchronizer, fixture);
        }

        [TestMethod]
        public async Task Synchronize_NewLocalObjectiveWithoutLocation_AddNewObjectiveToRemote()
        {
            // Arrange.

            // Act.
            await Synchronize();

            // Assert.
        }

        [TestCleanup]
        public void Cleanup()
            => Dispose();

        public void Dispose()
        {
            fixture.Dispose();
            serviceProvider.Dispose();
        }

        private async Task Synchronize()
        {
            await synchronizer.Synchronize(
                new SynchronizingData(),
                connection.Object,
                new ConnectionInfoExternalDto(),
                new Progress<double>(),
                CancellationToken.None);
        }
    }
}
