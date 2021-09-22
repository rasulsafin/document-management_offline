using Brio.Docs.Connection.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connection.Bim360.Forge.Utils;
using Brio.Docs.Connection.Bim360.Synchronization.Utilities;
using Brio.Docs.Interface;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Brio.Docs.Connection.Bim360.Synchronization.Extensions;

namespace Brio.Docs.Connection.Bim360.Synchronization.Converters
{
    internal class IssueObjectiveConverter : IConverter<Issue, ObjectiveExternalDto>
    {
        private readonly IConverter<Status, ObjectiveStatus> statusConverter;

        public IssueObjectiveConverter(IConverter<Status, ObjectiveStatus> statusConverter)
        {
            this.statusConverter = statusConverter;
        }

        public async Task<ObjectiveExternalDto> Convert(Issue issue)
        {
            var resultDto = new ObjectiveExternalDto
            {
                ExternalID = issue.ID,
                AuthorExternalID = issue.Attributes.CreatedBy,
                ObjectiveType = new ObjectiveTypeExternalDto { ExternalId = issue.Type },
                Title = issue.Attributes.Title,
                Description = issue.Attributes.Description,
                Status = await statusConverter.Convert(issue.Attributes.Status),
                DynamicFields = GetDynamicFields(issue),
                Items = new List<ItemExternalDto>(),
                BimElements = GetBimElements(issue),
            };

            if (issue.Attributes.PushpinAttributes != null)
                resultDto.Location = GetLocation(issue);

            if (issue.Attributes.CreatedAt.HasValue)
                resultDto.CreationDate = issue.Attributes.CreatedAt.Value;

            if (issue.Attributes.DueDate.HasValue)
                resultDto.DueDate = issue.Attributes.DueDate.Value;

            if (issue.Attributes.UpdatedAt.HasValue)
                resultDto.UpdatedAt = issue.Attributes.UpdatedAt.Value;

            return resultDto;
        }

        private static ICollection<DynamicFieldExternalDto> GetDynamicFields(Issue issue)
        {
            var result = new List<DynamicFieldExternalDto>
            {
                new ()
                {
                    ExternalID = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.LocationDescription),
                    Type = DynamicFieldType.STRING,
                    Name = MrsConstants.LOCATION_DETAILS_FIELD_NAME,
                    Value = issue.Attributes.LocationDescription,
                    UpdatedAt = issue.Attributes.UpdatedAt ?? default,
                },
                new ()
                {
                    ExternalID = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.Answer),
                    Type = DynamicFieldType.STRING,
                    Name = MrsConstants.RESPONSE_FIELD_NAME,
                    Value = issue.Attributes.Answer,
                    UpdatedAt = issue.Attributes.UpdatedAt ?? default,
                },
            };
            return result;
        }

        private static ICollection<BimElementExternalDto> GetBimElements(Issue issue)
            => issue.GetOtherInfo()?.BimElements ?? ArraySegment<BimElementExternalDto>.Empty;

        private static LocationExternalDto GetLocation(Issue issue)
        {
            var pushpinAttributes = issue.Attributes.PushpinAttributes;

            if (pushpinAttributes?.Location == null && pushpinAttributes?.ViewerState?.GlobalOffset == null &&
                pushpinAttributes?.ViewerState?.Viewport?.Eye == null)
                return null;

            var location = pushpinAttributes.Location ?? Vector3.Zero;
            var offset = pushpinAttributes.ViewerState?.GlobalOffset ?? Vector3.Zero;
            var eye = pushpinAttributes.ViewerState?.Viewport?.Eye ?? Vector3.Zero;

            return new LocationExternalDto
            {
                Location = (location + offset).ToMeters().ToXZY().ToTuple(),
                CameraPosition = eye.ToMeters().ToXZY().ToTuple(),
            };
        }
    }
}
