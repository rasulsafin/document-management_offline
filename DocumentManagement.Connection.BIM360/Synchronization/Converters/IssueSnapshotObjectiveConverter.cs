using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueSnapshotObjectiveConverter : IConverter<IssueSnapshot, ObjectiveExternalDto>
    {
        private readonly IConverter<Issue, ObjectiveExternalDto> converterToDto;
        private readonly TypeSubtypeEnumCreator subtypeEnumCreator;
        private readonly RootCauseEnumCreator rootCauseEnumCreator;
        private readonly LocationEnumCreator locationEnumCreator;
        private readonly AssignToEnumCreator assignToEnumCreator;

        public IssueSnapshotObjectiveConverter(
            IConverter<Issue, ObjectiveExternalDto> converterToDto,
            TypeSubtypeEnumCreator subtypeEnumCreator,
            RootCauseEnumCreator rootCauseEnumCreator,
            LocationEnumCreator locationEnumCreator,
            AssignToEnumCreator assignToEnumCreator)
        {
            this.converterToDto = converterToDto;
            this.subtypeEnumCreator = subtypeEnumCreator;
            this.rootCauseEnumCreator = rootCauseEnumCreator;
            this.locationEnumCreator = locationEnumCreator;
            this.assignToEnumCreator = assignToEnumCreator;
        }

        public async Task<ObjectiveExternalDto> Convert(IssueSnapshot snapshot)
        {
            var parsedToDto = await converterToDto.Convert(snapshot.Entity);
            var typeField = DynamicFieldUtilities.CreateField(
                snapshot.ProjectSnapshot.IssueTypes[snapshot.Entity.Attributes.NgIssueSubtypeID].ID,
                subtypeEnumCreator);
            var rootCause = snapshot.Entity.Attributes.RootCauseID == null
                ? DynamicFieldUtilities.CreateField(rootCauseEnumCreator.NullID, rootCauseEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.RootCauses[snapshot.Entity.Attributes.RootCauseID].ID,
                    rootCauseEnumCreator);
            var locations = snapshot.Entity.Attributes.Location == null
                ? DynamicFieldUtilities.CreateField(locationEnumCreator.NullID, locationEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.Locations[snapshot.Entity.Attributes.Location].ID,
                    locationEnumCreator);
            var assignedTo = snapshot.Entity.Attributes.AssignedTo == null
                ? DynamicFieldUtilities.CreateField(assignToEnumCreator.NullID, assignToEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.AssignToVariants.ContainsKey(snapshot.Entity.Attributes.AssignedTo)
                        ? snapshot.ProjectSnapshot.AssignToVariants[snapshot.Entity.Attributes.AssignedTo].ID
                        : assignToEnumCreator.NullID,
                    assignToEnumCreator);
            parsedToDto.DynamicFields.Add(typeField);
            parsedToDto.DynamicFields.Add(rootCause);
            parsedToDto.DynamicFields.Add(locations);
            parsedToDto.DynamicFields.Add(assignedTo);
            parsedToDto.ProjectExternalID = snapshot.ProjectSnapshot.Entity.ID;

            if (snapshot.Items != null)
            {
                parsedToDto.Items ??= new List<ItemExternalDto>();

                foreach (var attachment in snapshot.Items.Values)
                    parsedToDto.Items.Add(attachment.Entity.ToDto());
            }

            if (parsedToDto.Location != null &&
                snapshot.Entity.Attributes.TargetUrn != null &&
                snapshot.ProjectSnapshot.Items.TryGetValue(snapshot.Entity.Attributes.TargetUrn, out var target))
            {
                if (!TryRedirect(snapshot, parsedToDto))
                    parsedToDto.Location.Item = target.Entity.ToDto();
            }

            return parsedToDto;
        }

        private bool TryRedirect(IssueSnapshot snapshot, ObjectiveExternalDto parsedToDto)
        {
            var otherInfo = snapshot.Entity.GetOtherInfo();
            var linkedInfo = otherInfo?.OriginalModelInfo;

            if (linkedInfo == null ||
                !snapshot.ProjectSnapshot.Items.TryGetValue(otherInfo.OriginalModelInfo.Urn, out var originalTarget))
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
