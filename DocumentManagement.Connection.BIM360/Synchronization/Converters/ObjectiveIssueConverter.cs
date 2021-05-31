using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly Bim360ConnectionContext context;
        private readonly ConverterAsync<ObjectiveStatus, Status> statusConvert;
        private readonly IssuesService issuesService;

        public ObjectiveIssueConverter(
            Bim360ConnectionContext context,
            ConverterAsync<ObjectiveStatus, Status> statusConvert,
            IssuesService issuesService)
        {
            this.context = context;
            this.statusConvert = statusConvert;
            this.issuesService = issuesService;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto objective)
        {
            Issue exist = null;
            if (objective.ExternalID != null)
                exist = await issuesService.GetIssueAsync(GetContainerId(objective), objective.ExternalID);

            string type, subtype;
            string[] permittedAttributes = null;
            Status[] permittedStatuses = null;

            if (exist == null)
            {
                var issueType = await GetIssueType(objective);
                (type, subtype) = (issueType.ID, issueType.Subtypes[0].ID);
            }
            else
            {
                type = exist.Attributes.NgIssueTypeID;
                subtype = exist.Attributes.NgIssueSubtypeID;
                permittedAttributes = exist.Attributes.PermittedAttributes;
                permittedStatuses = exist.Attributes.PermittedStatuses;
            }

            var result = new Issue
            {
                ID = objective.ExternalID,
                Attributes = new Issue.IssueAttributes
                {
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = await statusConvert(objective.Status),
                    AssignedTo = GetDynamicField(objective.DynamicFields, nameof(Issue.Attributes.AssignedTo)),
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    LocationDescription = GetBimElements(objective),
                    PushpinAttributes = GetPushpinAttributes(objective.Location, Vector3.Zero),
                    NgIssueTypeID = type,
                    NgIssueSubtypeID = subtype,
                    PermittedAttributes = permittedAttributes,
                    PermittedStatuses = permittedStatuses,
                },
            };
            return result;
        }

        private static DateTime? ConvertToNullable(DateTime dateTime)
            => dateTime == default ? (DateTime?)null : dateTime;

        private static string GetDynamicField(ICollection<DynamicFieldExternalDto> dynamicFields, string fieldName)
        {
            var field = dynamicFields?.FirstOrDefault(f => f.ExternalID == fieldName);
            return field?.Value;
        }

        private static Issue.PushpinAttributes GetPushpinAttributes(LocationExternalDto locationDto, Vector3 offset = default)
        {
            var target = locationDto.Location.ToVector().ToFeet().ToXZY();
            var eye = locationDto.CameraPosition.ToVector().ToFeet().ToXZY();

            return new Issue.PushpinAttributes
            {
                Location = target - offset,
                ViewerState = new Issue.ViewerState
                {
                    GlobalOffset = offset,
                    Viewport = new Issue.Viewport
                    {
                        AspectRatio = 50,
                        Eye = eye,
                        Up = (target - eye).GetUpwardVector(),
                        Target = target,
                    },
                },
            };
        }

        private static string GetBimElements(ObjectiveExternalDto objectiveExternalDto)
            => objectiveExternalDto.BimElements == null
                ? null
                : JsonConvert.SerializeObject(objectiveExternalDto.BimElements);

        private async Task<IssueType> GetIssueType(ObjectiveExternalDto obj)
        {
            var types = await issuesService.GetIssueTypesAsync(GetContainerId(obj));
            var dynamicFieldID = typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.NgIssueTypeID));
            var dynamicField = obj.DynamicFields.First(d => d.ExternalID == dynamicFieldID);
            var type = types.FirstOrDefault(x => x.Title == dynamicField.Value) ?? types[0];
            return type;
        }

        private string GetContainerId(ObjectiveExternalDto obj)
            => context.Snapshot.Hubs.SelectMany(x => x.Value.Projects)
               .First(x => x.Key == obj.ProjectExternalID)
               .Value.IssueContainer;
    }
}
