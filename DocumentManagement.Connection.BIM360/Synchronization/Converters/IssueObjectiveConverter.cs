using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueObjectiveConverter : IConverter<Issue, ObjectiveExternalDto>
    {
        private readonly Bim360ConnectionContext context;
        private readonly ConverterAsync<Status, ObjectiveStatus> statusConvertAsync;

        public IssueObjectiveConverter(
            Bim360ConnectionContext context,
            ConverterAsync<Status, ObjectiveStatus> statusConvertAsync)
        {
            this.context = context;
            this.statusConvertAsync = statusConvertAsync;
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
                Status = await statusConvertAsync(issue.Attributes.Status),
                DynamicFields = GetDynamicFields(),
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

        private static ICollection<DynamicFieldExternalDto> GetDynamicFields()
        {
            return new List<DynamicFieldExternalDto>();
        }

        private static ICollection<BimElementExternalDto> GetBimElements(Issue issue)
        {
            try
            {
                return string.IsNullOrWhiteSpace(issue.Attributes.LocationDescription)
                    ? ArraySegment<BimElementExternalDto>.Empty
                    : JsonConvert.DeserializeObject<ICollection<BimElementExternalDto>>(
                        issue.Attributes.LocationDescription);
            }
            catch
            {
                return ArraySegment<BimElementExternalDto>.Empty;
            }
        }

        private static LocationExternalDto GetLocation(Issue issue)
        {
            var pushpinAttributes = issue.Attributes.PushpinAttributes;
            var location = pushpinAttributes.Location ?? Vector3.Zero;
            var offset = pushpinAttributes.ViewerState?.GlobalOffset ?? Vector3.Zero;
            var eye = pushpinAttributes.ViewerState?.Viewport?.Eye ?? Vector3.Zero;
            var name = issue.Attributes.SheetMetadata?.Name;

            return new LocationExternalDto
            {
                Location = (location - offset).ToMeters().ToXZY().ToTuple(),
                CameraPosition = eye.ToMeters().ToXZY().ToTuple(),
                ModelName = name,
            };
        }
    }
}
