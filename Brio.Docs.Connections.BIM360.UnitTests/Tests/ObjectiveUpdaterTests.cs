using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Mock<IConverter<IEnumerable<Comment>, IEnumerable<BimElementExternalDto>>>
            stubConverterCommentsToBimElements = new ();

        private ObjectiveUpdater objectiveUpdater;
        private SnapshotUpdater snapshotUpdater;
        private SnapshotGetter snapshotGetter;

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
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task Put_ObjectiveWithExternalIdAndBimElements_PostComment(int bimElementsCount)
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.BimElements = CreateBimElements(bimElementsCount).ToArray();
            var convertedObjectiveToIssue = CreateExistingIssue();
            SetupStubs(convertedObjectiveToIssue, dto);

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(bimElementsCount, result.BimElements.Count);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task Put_ObjectiveWithoutExternalIdAndWithBimElements_PostComment(int bimElementsCount)
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.ExternalID = null;
            dto.BimElements = CreateBimElements(bimElementsCount).ToArray();
            var convertedObjectiveToIssue = DummyModels.Issue;
            convertedObjectiveToIssue.ID = null;
            SetupStubs(convertedObjectiveToIssue, dto);

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(bimElementsCount, result.BimElements.Count);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task Put_ObjectiveHasNewBimElements_PostComment(int newBimElementsCount)
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            var bimElements = CreateBimElements(newBimElementsCount + 1).ToArray();
            dto.BimElements = bimElements;
            var convertedObjectiveToIssue = CreateExistingIssue();
            SetupStubs(convertedObjectiveToIssue, dto, new[] { bimElements[0] });

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(newBimElementsCount + 1, result.BimElements.Count);
        }

        [TestMethod]
        public async Task Put_ObjectiveHasChangedBimElement_PostComment()
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            var bimElements = CreateBimElements(3).ToArray();
            dto.BimElements = new List<BimElementExternalDto> { bimElements[0], bimElements[2] };
            var convertedObjectiveToIssue = CreateExistingIssue();
            SetupStubs(convertedObjectiveToIssue, dto, new[] { bimElements[0], bimElements[1] });

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(2, result.BimElements.Count);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task Put_ObjectiveRemovedBimElements_PostComment(int wasBimElementsCount)
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.BimElements = Array.Empty<BimElementExternalDto>();
            var convertedObjectiveToIssue = CreateExistingIssue();
            SetupStubs(convertedObjectiveToIssue, dto, CreateBimElements(wasBimElementsCount).ToArray());

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Once);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(0, result.BimElements.Count);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task Put_ObjectiveBimElementsCollectionDoesNotChanged_DoNotPostComment(int bimElementsCount)
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            var collection = CreateBimElements(bimElementsCount).ToArray();
            dto.BimElements = collection;
            var convertedObjectiveToIssue = CreateExistingIssue();
            SetupStubs(convertedObjectiveToIssue, dto, collection);

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Never);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(bimElementsCount, result.BimElements.Count);
        }

        [TestMethod]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(10)]
        public async Task Put_BimElementsConvertedToMultipleComments_PostMultipleComment(int createdCommentsCount)
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.BimElements = CreateBimElements(1).ToArray();
            var convertedObjectiveToIssue = CreateExistingIssue();
            SetupStubs(convertedObjectiveToIssue, dto, createdCommentsCount: createdCommentsCount);

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Exactly(createdCommentsCount));
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(1, result.BimElements.Count);
        }

        [TestMethod]
        public async Task Put_ObjectiveWithoutExternalIdAndWithoutBimElements_DoNotPostComment()
        {
            // Arrange.
            var dto = DummyDtos.Objective;
            dto.ExternalID = null;
            dto.BimElements = Array.Empty<BimElementExternalDto>();
            var convertedObjectiveToIssue = DummyModels.Issue;
            convertedObjectiveToIssue.ID = null;
            SetupStubs(convertedObjectiveToIssue, dto);

            // Act.
            var result = await objectiveUpdater.Put(dto);

            // Assert.
            mockIssuesService.Verify(
                x => x.PostIssuesCommentsAsync(It.IsAny<string>(), It.IsAny<Comment>()),
                Times.Never);
            Assert.IsNotNull(result.BimElements);
            Assert.AreEqual(0, result.BimElements.Count);
        }

        private void SetupStubs(
            Issue postingIssue = null,
            ObjectiveExternalDto gottenDto = null,
            BimElementExternalDto[] currentBimElements = null,
            int createdCommentsCount = 1)
        {
            SetupConvertingToIssue(_ => postingIssue);
            SetupConvertingToDto(_ => gottenDto);
            SetupGettingEmptyCommentsList();
            SetupConvertingBimElementsToComments(createdCommentsCount);
            SetupConvertingCommentsToBimElements(currentBimElements);
            SetupPatchingIssue();
            SetupPostingIssue();
        }

        private void SetupPostingIssue()
            => mockIssuesService.Setup(x => x.PostIssueAsync(It.IsAny<string>(), It.IsAny<Issue>()))
               .Returns<string, Issue>(
                    (_, posting) =>
                    {
                        posting.ID = DummyStrings.ISSUE_ID;
                        return Task.FromResult(posting);
                    });

        private void SetupPatchingIssue()
            => mockIssuesService.Setup(x => x.PatchIssueAsync(It.IsAny<string>(), It.IsAny<Issue>()))
               .Returns<string, Issue>((_, patchingIssue) => Task.FromResult(patchingIssue));

        private void SetupConvertingBimElementsToComments(int count)
            => stubConverterBimElementsToComments.Setup(x => x.Convert(It.IsAny<CommentCreatingData>()))
               .Returns<CommentCreatingData>(_ => Task.FromResult(Enumerable.Repeat(DummyModels.Comment, count)));

        private void SetupGettingEmptyCommentsList()
            => mockIssuesService.Setup(x => x.GetCommentsAsync(It.IsAny<string>(), It.IsAny<string>()))
               .Returns<string, string>((_, _) => Task.FromResult(new List<Comment>()));

        private void SetupConvertingToDto(Func<IssueSnapshot, ObjectiveExternalDto> valueFunction)
            => stubConverterToDto.Setup(x => x.Convert(It.IsAny<IssueSnapshot>()))
               .Returns<IssueSnapshot>(snapshot => Task.FromResult(valueFunction(snapshot)));

        private void SetupConvertingToIssue(Func<ObjectiveExternalDto, Issue> valueFunction)
            => stubConverterToIssue.Setup(x => x.Convert(It.IsAny<ObjectiveExternalDto>()))
               .Returns<ObjectiveExternalDto>(dto => Task.FromResult(valueFunction(dto)));

        private void SetupConvertingCommentsToBimElements(params BimElementExternalDto[] bimElements)
            => stubConverterCommentsToBimElements.Setup(x => x.Convert(It.IsAny<IEnumerable<Comment>>()))
               .Returns<IEnumerable<Comment>>(_ => Task.FromResult<IEnumerable<BimElementExternalDto>>(bimElements));

        private Issue CreateExistingIssue(string projectId = null)
        {
            projectId ??= DummyStrings.PROJECT_ID;
            var issue = DummyModels.Issue;
            var project = snapshotGetter.GetProject(projectId);
            snapshotUpdater.CreateIssue(project, issue);
            return issue;
        }

        private IEnumerable<BimElementExternalDto> CreateBimElements(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = DummyDtos.BimElement;
                element.GlobalID = DummyStrings.GetBimElementGlobalId();
                yield return element;
            }
        }
    }
}
