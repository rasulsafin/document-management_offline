using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly Mock<IIssuesService> mockIssuesService = new ();
        private readonly Mock<IConverter<ObjectiveExternalDto, Issue>> stubConverterToIssue = new ();
        private readonly Mock<IConverter<IssueSnapshot, ObjectiveExternalDto>> stubConverterToDto = new ();
        private readonly Mock<IConverter<CommentCreatingData, IEnumerable<Comment>>>
            stubConverterBimElementsToComments = new ();

        private ObjectiveUpdater objectiveUpdater;
        private SnapshotUpdater snapshotUpdater;
        private SnapshotGetter snapshotGetter;
        private readonly Mock<IConverter<IEnumerable<Comment>, IEnumerable<BimElementExternalDto>>> stubConverterCommentsToBimElements = new Mock<IConverter<IEnumerable<Comment>, IEnumerable<BimElementExternalDto>>>();

        [TestInitialize]
        public void Setup()
        {
            var bim360Snapshot = DummySnapshots.Bim360Snapshot;
            var hub = DummySnapshots.Hub;
            bim360Snapshot.Hubs.Add(hub.ID, hub);
            var project = DummySnapshots.CreateProject(hub);
            hub.Projects.Add(project.ID, project);

            var stubItemsSyncHelper = new Mock<IItemsUpdater>();
            var stubUserReader = new Mock<IUsersGetter>();

            snapshotGetter = new SnapshotGetter(bim360Snapshot);
            snapshotUpdater = new SnapshotUpdater(snapshotGetter);
            var issueSnapshotUtilities = new IssueSnapshotUtilities(mockIssuesService.Object, stubUserReader.Object);

            objectiveUpdater = new ObjectiveUpdater(
                snapshotGetter,
                snapshotUpdater,
                mockIssuesService.Object,
                stubItemsSyncHelper.Object,
                issueSnapshotUtilities,
                stubConverterBimElementsToComments.Object,
                stubConverterCommentsToBimElements.Object,
                stubConverterToIssue.Object,
                stubConverterToDto.Object);
        }

        [TestMethod]
        public async Task Put_ObjectiveWithExternalIdAndBimElement_PostComment()
        {
            // Arrange.
            var issue = DummyModels.Issue;
            var dto = DummyDtos.Objective;
            snapshotUpdater.CreateIssue(snapshotGetter.GetProject(dto.ProjectExternalID), issue);
            dto.BimElements = new List<BimElementExternalDto> { DummyDtos.BimElement };
            SetupConvertingToIssue(_ => issue);
            SetupConvertingToDto(_ => dto);
            SetupGettingEmptyCommentsList();
            SetupConvertingBimElementsToComments();
            mockIssuesService.Setup(x => x.PatchIssueAsync(It.IsAny<string>(), It.IsAny<Issue>()))
               .Returns<string, Issue>((_, patchingIssue) => Task.FromResult(patchingIssue));

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(1, result.BimElements.Count);
        }

        [TestMethod]
        public async Task Put_ObjectiveWithoutExternalIdAndWithBimElement_PostComment()
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.ExternalID = null;
            dto.BimElements = new List<BimElementExternalDto> { DummyDtos.BimElement };
            SetupConvertingToIssue(
                _ =>
                {
                    var issue = DummyModels.Issue;
                    issue.ID = null;
                    return issue;
                });
            SetupConvertingToDto(_ => dto);
            SetupGettingEmptyCommentsList();
            SetupConvertingBimElementsToComments();
            mockIssuesService.Setup(x => x.PostIssueAsync(It.IsAny<string>(), It.IsAny<Issue>()))
               .Returns<string, Issue>((_, issue) =>
                {
                    issue.ID = DummyStrings.ISSUE_ID;
                    return Task.FromResult(issue);
                });

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(1, result.BimElements.Count);
        }

        [TestMethod]
        public async Task Put_ObjectiveHasNewBimElement_PostComment()
        {
            // Arrange.
            var issue = DummyModels.Issue;
            var dto = DummyDtos.Objective;
            snapshotUpdater.CreateIssue(snapshotGetter.GetProject(dto.ProjectExternalID), issue);
            var bimElementFirst = DummyDtos.BimElement;
            var bimElementNew = DummyDtos.BimElement;
            bimElementNew.GlobalID = DummyStrings.GetBimElementGlobalId();
            dto.BimElements = new List<BimElementExternalDto> { bimElementFirst, bimElementNew };
            SetupConvertingToIssue(_ => issue);
            SetupConvertingToDto(_ => dto);
            SetupGettingEmptyCommentsList();
            SetupConvertingBimElementsToComments();

            stubConverterCommentsToBimElements.Setup(x => x.Convert(It.IsAny<IEnumerable<Comment>>()))
               .Returns<IEnumerable<Comment>>(
                    _ => Task.FromResult<IEnumerable<BimElementExternalDto>>(new[] { bimElementFirst }));

            mockIssuesService.Setup(x => x.PatchIssueAsync(It.IsAny<string>(), It.IsAny<Issue>()))
               .Returns<string, Issue>((_, patchingIssue) => Task.FromResult(patchingIssue));

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(2, result.BimElements.Count);
        }

        private void SetupConvertingBimElementsToComments()
            => stubConverterBimElementsToComments.Setup(x => x.Convert(It.IsAny<CommentCreatingData>()))
               .Returns<CommentCreatingData>(_ => Task.FromResult<IEnumerable<Comment>>(new[] { DummyModels.Comment }));

        private void SetupGettingEmptyCommentsList()
            => mockIssuesService.Setup(x => x.GetCommentsAsync(It.IsAny<string>(), It.IsAny<string>()))
               .Returns<string, string>((_, _) => Task.FromResult(new List<Comment>()));

        private void SetupConvertingToDto(Func<IssueSnapshot, ObjectiveExternalDto> valueFunction)
            => stubConverterToDto.Setup(x => x.Convert(It.IsAny<IssueSnapshot>()))
               .Returns<IssueSnapshot>(snapshot => Task.FromResult(valueFunction(snapshot)));

        private void SetupConvertingToIssue(
            Func<ObjectiveExternalDto, Issue> valueFunction)
            => stubConverterToIssue.Setup(x => x.Convert(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(dto => Task.FromResult(valueFunction(dto)));
    }
}
