using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Converters;
using Brio.Docs.Connections.Bim360.Synchronization.Models;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.UnitTests.Dummy;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Connections.Bim360.UnitTests
{
    [TestClass]
    public class IssueSnapshotObjectiveConverterTests
    {
        private readonly Mock<IConverter<Issue, ObjectiveExternalDto>> stubConverterToDto = new ();
        private readonly Mock<IConverter<IssueSnapshot, ObjectiveStatus>> stubStatusConverter = new ();
        private readonly Mock<IEnumIdentification<IssueTypeSnapshot>> stubSubtypeEnumCreator = new ();
        private readonly Mock<IEnumIdentification<RootCauseSnapshot>> stubRootCauseEnumCreator = new ();
        private readonly Mock<IEnumIdentification<LocationSnapshot>> stubLocationEnumCreator = new ();
        private readonly Mock<IEnumIdentification<AssignToVariant>> stubAssignToEnumCreator = new ();
        private readonly Mock<IEnumIdentification<StatusSnapshot>> stubStatusEnumCreator = new ();
        private readonly Mock<IConverter<IEnumerable<Comment>, IEnumerable<BimElementExternalDto>>>
            stubConverterCommentsToBimElements = new ();

        private readonly Mock<IConverter<IEnumerable<Comment>, LinkedInfo>> stubConvertToLinkedInfo = new ();

        private readonly Mock<MetaCommentHelper> mockCommentHelper = new ();
        private IssueSnapshotObjectiveConverter converter;
        private IssueSnapshot issueSnapshot;
        private ObjectiveExternalDto dto;

        [TestInitialize]
        public void Setup()
        {
            converter = new IssueSnapshotObjectiveConverter(
                stubConverterToDto.Object,
                stubStatusConverter.Object,
                stubConverterCommentsToBimElements.Object,
                stubConvertToLinkedInfo.Object,
                stubSubtypeEnumCreator.Object,
                stubRootCauseEnumCreator.Object,
                stubLocationEnumCreator.Object,
                stubAssignToEnumCreator.Object,
                stubStatusEnumCreator.Object);

            dto = new ObjectiveExternalDto
            {
                DynamicFields = new List<DynamicFieldExternalDto>(),
            };
            stubConverterToDto
               .Setup(x => x.Convert(It.IsAny<Issue>()))
               .Returns<Issue>(_ => Task.FromResult(dto));

            var projectSnapshot = new ProjectSnapshot(new Project(), new HubSnapshot(new Hub()))
            {
                Items = new Dictionary<string, ItemSnapshot>(),
            };
            var subtypeID = $"subtype-{Guid.NewGuid()}";
            projectSnapshot.IssueTypes = new Dictionary<string, IssueTypeSnapshot>
            {
                [subtypeID] = new (new IssueType(), new IssueSubtype { ID = subtypeID }, projectSnapshot),
            };
            projectSnapshot.Statuses = new Dictionary<string, StatusSnapshot>
            {
                [Status.Open.GetEnumMemberValue()] = new (Status.Open, projectSnapshot),
            };
            var issue = new Issue
            {
                Attributes = new Issue.IssueAttributes
                {
                    NgIssueSubtypeID = projectSnapshot.IssueTypes.First().Value.SubtypeID,
                    Status = projectSnapshot.Statuses.First().Value.Entity,
                },
            };
            issueSnapshot = new IssueSnapshot(issue, projectSnapshot);
        }

        [TestMethod]
        public async Task Convert_ProjectItemCollectionIsEmptyIssueIsPushpin_ReturnsObjectiveWithEmptyLocation()
        {
            // Arrange.
            var attributes = issueSnapshot.Entity.Attributes;
            attributes.TargetUrn = $"file-{Guid.NewGuid()}";
            attributes.PushpinAttributes = new Issue.PushpinAttributes
            {
                Location = new Vector3d(),
                ViewerState = new Issue.ViewerState
                {
                    Viewport = new Issue.Viewport
                    {
                        Eye = new Vector3d(),
                        Target = new Vector3d(),
                    },
                    GlobalOffset = new Vector3d(),
                },
            };
            dto.Location = new LocationExternalDto
            {
                Location = (0, 0, 0),
                CameraPosition = (0, 0, 0),
                Guid = null,
                Item = null,
            };

            // Act.
            var result = await converter.Convert(issueSnapshot);

            // Assert.
            Assert.IsNull(result.Location);
        }

        [TestMethod]
        public async Task Convert_IssueHasSimpleComment_ObjectiveHasCommentDynamicField()
        {
            // Arrange.
            var comment = DummyModels.Comment;
            comment.Attributes.IssueId = issueSnapshot.ID;
            issueSnapshot.Comments = new List<CommentSnapshot> { new (comment) };

            // Act.
            var result = await converter.Convert(issueSnapshot);
            var dfs = ObjectiveUtilities.EnumerateAll(result.DynamicFields).ToArray();
            var commentDynamicField = dfs.FirstOrDefault(x => x.ExternalID == comment.ID);
            var bodyDynamicField = dfs.FirstOrDefault(x => x.Value == comment.Attributes.Body);

            // Assert.
            Assert.IsNotNull(result.DynamicFields);
            Assert.IsNotNull(commentDynamicField);
            Assert.IsNotNull(bodyDynamicField);
        }

        [TestMethod]
        public async Task Convert_IssueHasTaggedComment_ObjectiveHasNoCommentDynamicField()
        {
            // Arrange.
            var comment = DummyModels.Comment;
            comment.Attributes.Body = $"{MrsConstants.META_COMMENT_TAG} {comment.Attributes.Body}";
            comment.Attributes.IssueId = issueSnapshot.ID;
            issueSnapshot.Comments = new List<CommentSnapshot> { new (comment) };

            // Act.
            var result = await converter.Convert(issueSnapshot);
            var dfs = ObjectiveUtilities.EnumerateAll(result.DynamicFields).ToArray();
            var commentDynamicField = dfs.FirstOrDefault(x => x.ExternalID == comment.ID);
            var bodyDynamicField = dfs.FirstOrDefault(x => x.Value == comment.Attributes.Body);

            // Assert.
            Assert.IsNotNull(result.DynamicFields);
            Assert.IsNull(commentDynamicField);
            Assert.IsNull(bodyDynamicField);
        }
    }
}