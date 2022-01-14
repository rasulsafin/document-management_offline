using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly SnapshotGetter snapshot;
        private readonly IConverter<ObjectiveExternalDto, Status> statusConverter;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly LocationEnumCreator locationEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;

        public ObjectiveIssueConverter(
            SnapshotGetter snapshot,
            IConverter<ObjectiveExternalDto, Status> statusConverter,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            LocationEnumCreator locationEnumCreator,
            AssignToEnumCreator assignToEnumCreator)
        {
            this.snapshot = snapshot;
            this.statusConverter = statusConverter;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto objective)
        {
            Issue exist = null;
            var project = snapshot.GetProject(objective.ProjectExternalID);
            if (objective.ExternalID != null && project.Issues.TryGetValue(objective.ExternalID, out var issueSnapshot))
                exist = issueSnapshot.Entity;

            string type;
            string subtype;
            string[] permittedAttributes = null;
            Status[] permittedStatuses = null;
            var typeSnapshot = GetIssueTypes(project, objective);

            if (exist == null)
            {
                (type, subtype) = (typeSnapshot.ParentTypeID, typeSnapshot.SubtypeID);
            }
            else
            {
                type = exist.Attributes.NgIssueTypeID;
                subtype = exist.Attributes.NgIssueSubtypeID;

                if (typeSnapshot.ParentTypeID == type)
                    subtype = typeSnapshot.SubtypeID;

                permittedAttributes = exist.Attributes.PermittedAttributes;
                permittedStatuses = exist.Attributes.PermittedStatuses;
            }

            var assignToVariant = DynamicFieldUtilities.GetValue(
                assignToEnumCreator,
                project,
                objective,
                (ids, s) => ids.Contains(s.Entity),
                out _);
            var result = new Issue
            {
                ID = objective.ExternalID,
                Attributes = new Issue.IssueAttributes
                {
                    Title = objective.Title,
                    Description = objective.Description,
                    Status = await statusConverter.Convert(objective),
                    AssignedTo = assignToVariant?.Entity,
                    AssignedToType = assignToVariant?.Type,
                    CreatedAt = ConvertToNullable(objective.CreationDate),
                    DueDate = ConvertToNullable(objective.DueDate),
                    LocationDescription = GetDynamicField(objective.DynamicFields, x => x.LocationDescription),
                    RootCauseID =
                        DynamicFieldUtilities.GetValue(
                                rootCauseEnumCreator,
                                project,
                                objective,
                                (ids, s) => ids.Contains(s.Entity.ID),
                                out _)
                          ?.Entity.ID,
                    LbsLocation =
                        DynamicFieldUtilities.GetValue(
                                locationEnumCreator,
                                project,
                                objective,
                                (ids, s) => ids.Contains(s.Entity.ID),
                                out _)
                          ?.Entity.ID,
                    Answer = GetDynamicField(objective.DynamicFields, x => x.Answer),
                    NgIssueTypeID = type,
                    NgIssueSubtypeID = subtype,
                    PermittedAttributes = permittedAttributes,
                    PermittedStatuses = permittedStatuses,
                },
            };

            return result;
        }

        private static T? ConvertToNullable<T>(T value)
            where T : struct
            => EqualityComparer<T>.Default.Equals(value, default) ? null : value;

        private static string GetDynamicField(
            IEnumerable<DynamicFieldExternalDto> dynamicFields,
            Expression<Func<Issue.IssueAttributes, object>> property)
        {
            var fieldID = DataMemberUtilities.GetPath(property);
            var field = dynamicFields?.FirstOrDefault(f => f.ExternalID == fieldID);
            return field?.Value;
        }

        private IssueTypeSnapshot GetIssueTypes(ProjectSnapshot projectSnapshot, ObjectiveExternalDto obj)
        {
            var type = DynamicFieldUtilities.GetValue(
                subtypeEnumCreator,
                projectSnapshot,
                obj,
                (ids, s) => ids.Any(x => x.subtypeID == s.SubtypeID),
                out var deserializedIDs);

            if (type == null)
            {
                var typeLookup = deserializedIDs.ToLookup(x => x.parentTypeID);
                type = projectSnapshot.IssueTypes.FirstOrDefault(
                            x => x.Value.SubTypeIsType && typeLookup.Contains(x.Value.ParentTypeID))
                       .Value ??
                    projectSnapshot.IssueTypes
                       .FirstOrDefault(x => typeLookup.Contains(x.Value.ParentTypeID))
                       .Value ??
                    projectSnapshot.IssueTypes.First().Value;
            }

            return type;
        }
    }
}
