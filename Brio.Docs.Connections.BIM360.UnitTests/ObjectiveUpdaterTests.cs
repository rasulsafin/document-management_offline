using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Interfaces;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Synchronization.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities.Objective;
using Brio.Docs.Connections.Bim360.UnitTests.Dummy;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Connections.Bim360.UnitTests
{
    [TestClass]
    public class ObjectiveUpdaterTests
    {
        private ObjectiveUpdater objectiveUpdater;
        private Mock<IIssuesService> mockIssuesService;
        private Mock<IConverter<ObjectiveExternalDto, Issue>> stubConverterToIssue;
        private Mock<IConverter<IssueSnapshot, ObjectiveExternalDto>> stubCoverterToDto;

        [TestInitialize]
        public void Setup()
        {
            var bim360Snapshot = DummySnapshots.Bim360Snapshot;
            var snapshotGetter = new SnapshotGetter(bim360Snapshot);
            var snapshotUpdater = new SnapshotUpdater(snapshotGetter);
            mockIssuesService = new Mock<IIssuesService>();
            var stubItemsSyncHelper = new Mock<IItemsUpdater>();
            var stubUserReader = new Mock<IUsersGetter>();
            stubConverterToIssue = new Mock<IConverter<ObjectiveExternalDto, Issue>>();
            stubCoverterToDto = new Mock<IConverter<IssueSnapshot, ObjectiveExternalDto>>();
            objectiveUpdater = new ObjectiveUpdater(
                snapshotGetter,
                snapshotUpdater,
                mockIssuesService.Object,
                stubItemsSyncHelper.Object,
                new IssueSnapshotUtilities(mockIssuesService.Object, stubUserReader.Object),
                new MetaCommentHelper(),
                stubConverterToIssue.Object,
                stubCoverterToDto.Object);
        }

        [TestMethod]
        public async Task Put()
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.BimElements = new List<BimElementExternalDto> { DummyDtos.BimElement };
            stubConverterToIssue.Setup(x => x.Convert(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(_ => Task.FromResult(DummyModels.Issue));
            mockIssuesService.Setup(x => x.PatchIssueAsync(It.IsAny<string>(), It.IsAny<Issue>()))
               .Returns<string, Issue>((_, issue) => Task.FromResult(issue));
            mockIssuesService.Setup(x => x.GetCommentsAsync(It.IsAny<string>(), It.IsAny<string>()))
               .Returns<string, string>((_, _) => Task.FromResult(new List<Comment>()));
            stubCoverterToDto.Setup(x => x.Convert(It.IsAny<IssueSnapshot>()))
               .Returns<IssueSnapshot>(_ => Task.FromResult(dto));

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
        }
    }
}
