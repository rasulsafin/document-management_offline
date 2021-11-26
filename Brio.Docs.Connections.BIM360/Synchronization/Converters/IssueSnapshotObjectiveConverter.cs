using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Interfaces;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Synchronization.Utilities;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class IssueSnapshotObjectiveConverter : IConverter<IssueSnapshot, ObjectiveExternalDto>
    {
        private readonly IConverter<Issue, ObjectiveExternalDto> converterToDto;
        private readonly IConverter<IssueSnapshot, ObjectiveStatus> statusConverter;
        private readonly IEnumIdentification<IssueTypeSnapshot> subtypeEnumCreator;
        private readonly IEnumIdentification<RootCauseSnapshot> rootCauseEnumCreator;
        private readonly IEnumIdentification<LocationSnapshot> locationEnumCreator;
        private readonly IEnumIdentification<AssignToVariant> assignToEnumCreator;
        private readonly IEnumIdentification<StatusSnapshot> statusEnumCreator;
        private readonly MetaCommentHelper metaCommentHelper;

        public IssueSnapshotObjectiveConverter(
            IConverter<Issue, ObjectiveExternalDto> converterToDto,
            IConverter<IssueSnapshot, ObjectiveStatus> statusConverter,
            IEnumIdentification<IssueTypeSnapshot> subtypeEnumCreator,
            IEnumIdentification<RootCauseSnapshot> rootCauseEnumCreator,
            IEnumIdentification<LocationSnapshot> locationEnumCreator,
            IEnumIdentification<AssignToVariant> assignToEnumCreator,
            IEnumIdentification<StatusSnapshot> statusEnumCreator,
            MetaCommentHelper metaCommentHelper)
        {
            this.converterToDto = converterToDto;
            this.statusConverter = statusConverter;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
            this.statusEnumCreator = statusEnumCreator;
            this.metaCommentHelper = metaCommentHelper;
        }

        public async Task<ObjectiveExternalDto> Convert(IssueSnapshot snapshot)
        {
            var parsedToDto = await converterToDto.Convert(snapshot.Entity);
            parsedToDto.Status = await statusConverter.Convert(snapshot);

            var typeField = DynamicFieldUtilities.CreateField(
                snapshot.ProjectSnapshot.IssueTypes[snapshot.Entity.Attributes.NgIssueSubtypeID].ID,
                subtypeEnumCreator);
            var rootCause = snapshot.Entity.Attributes.RootCauseID == null
                ? DynamicFieldUtilities.CreateField(rootCauseEnumCreator.NullID, rootCauseEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.RootCauses[snapshot.Entity.Attributes.RootCauseID].ID,
                    rootCauseEnumCreator);
            var locations = snapshot.Entity.Attributes.LbsLocation == null
                ? DynamicFieldUtilities.CreateField(locationEnumCreator.NullID, locationEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.Locations[snapshot.Entity.Attributes.LbsLocation].ID,
                    locationEnumCreator);
            var assignedTo = snapshot.Entity.Attributes.AssignedTo == null
                ? DynamicFieldUtilities.CreateField(assignToEnumCreator.NullID, assignToEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.AssignToVariants.ContainsKey(snapshot.Entity.Attributes.AssignedTo)
                        ? snapshot.ProjectSnapshot.AssignToVariants[snapshot.Entity.Attributes.AssignedTo].ID
                        : assignToEnumCreator.NullID,
                    assignToEnumCreator);
            var status = DynamicFieldUtilities.CreateField(
                snapshot.ProjectSnapshot.Statuses[snapshot.Entity.Attributes.Status.GetEnumMemberValue()].ID,
                statusEnumCreator);

            var comments =
                snapshot.Comments?.Where(x => !x.Entity.Attributes.Body.Contains(MrsConstants.META_COMMENT_TAG)) ??
                Enumerable.Empty<CommentSnapshot>();

            foreach (var commentSnapshot in comments)
            {
                var comment = new DynamicFieldExternalDto
                {
                    ExternalID = commentSnapshot.ID,
                    Type = DynamicFieldType.OBJECT,
                    Name = MrsConstants.COMMENT_FIELD_NAME,
                    UpdatedAt = commentSnapshot.Entity.Attributes.UpdatedAt ?? default,
                };

                var author = new DynamicFieldExternalDto
                {
                    ExternalID = commentSnapshot.Entity.Attributes.CreatedBy,
                    Type = DynamicFieldType.STRING,
                    Name = MrsConstants.AUTHOR_FIELD_NAME,
                    Value = commentSnapshot.Author,
                    UpdatedAt = commentSnapshot.Entity.Attributes.UpdatedAt ?? default,
                };

                var date = new DynamicFieldExternalDto
                {
                    ExternalID = DataMemberUtilities.GetPath<Comment.CommentAttributes>(x => x.CreatedAt),
                    Type = DynamicFieldType.DATE,
                    Name = MrsConstants.DATE_FIELD_NAME,
                    Value = (commentSnapshot.Entity.Attributes.CreatedAt ?? default).ToString(),
                    UpdatedAt = commentSnapshot.Entity.Attributes.UpdatedAt ?? default,
                };

                var text = new DynamicFieldExternalDto
                {
                    ExternalID = DataMemberUtilities.GetPath<Comment.CommentAttributes>(x => x.Body),
                    Type = DynamicFieldType.STRING,
                    Name = MrsConstants.COMMENT_FIELD_NAME,
                    Value = commentSnapshot.Entity.Attributes.Body,
                    UpdatedAt = commentSnapshot.Entity.Attributes.UpdatedAt ?? default,
                };

                comment.ChildrenDynamicFields = new List<DynamicFieldExternalDto>
                {
                    author,
                    date,
                    text,
                };
                parsedToDto.DynamicFields.Add(comment);
            }

            var newComment = new DynamicFieldExternalDto
            {
                ExternalID = MrsConstants.NEW_COMMENT_ID,
                Type = DynamicFieldType.STRING,
                Name = MrsConstants.NEW_COMMENT_FIELD_NAME,
                Value = string.Empty,
                UpdatedAt = DateTime.Now,
            };

            parsedToDto.DynamicFields.Add(status);
            parsedToDto.DynamicFields.Add(newComment);
            parsedToDto.DynamicFields.Add(typeField);
            parsedToDto.DynamicFields.Add(rootCause);
            parsedToDto.DynamicFields.Add(locations);
            parsedToDto.DynamicFields.Add(assignedTo);
            parsedToDto.ProjectExternalID = snapshot.ProjectSnapshot.Entity.ID;

            if (snapshot.Attachments != null)
            {
                parsedToDto.Items ??= new List<ItemExternalDto>();

                foreach (var attachment in snapshot.Attachments.Values)
                    parsedToDto.Items.Add(attachment.ToDto());
            }

            if (parsedToDto.Location != null &&
                snapshot.Entity.Attributes.TargetUrn != null)
            {
                if (snapshot.ProjectSnapshot.Items.TryGetValue(snapshot.Entity.Attributes.TargetUrn, out var target))
                {
                    if (!TryRedirect(snapshot, parsedToDto))
                        parsedToDto.Location.Item = target.Entity.ToDto();
                }
                else
                {
                    parsedToDto.Location = null;
                }
            }

            if (snapshot.Comments != null)
            {
                parsedToDto.BimElements = metaCommentHelper.GetBimElements(snapshot.Comments.Select(x => x.Entity)) ??
                    ArraySegment<BimElementExternalDto>.Empty;
            }
            else
            {
                parsedToDto.BimElements = ArraySegment<BimElementExternalDto>.Empty;
            }

            return parsedToDto;
        }

        private bool TryRedirect(IssueSnapshot snapshot, ObjectiveExternalDto parsedToDto)
        {
            if (snapshot.Comments == null)
                return false;

            var linkedInfo = metaCommentHelper.GetLinkedInfo(snapshot.Comments.Select(x => x.Entity));

            if (linkedInfo == null ||
                !snapshot.ProjectSnapshot.Items.TryGetValue(linkedInfo.Urn, out var originalTarget))
                return false;

            parsedToDto.Location.Item = originalTarget.Entity.ToDto();
            var location = parsedToDto.Location.Location.ToVector();
            var camera = parsedToDto.Location.CameraPosition.ToVector();
            location += linkedInfo.Offset;
            camera += linkedInfo.Offset;
            parsedToDto.Location.Location = location.ToTuple();
            parsedToDto.Location.CameraPosition = camera.ToTuple();
            return true;
        }
    }
}
