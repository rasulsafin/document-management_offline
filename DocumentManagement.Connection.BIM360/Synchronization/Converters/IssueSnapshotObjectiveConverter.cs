using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
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
        private readonly IfcConfigUtilities ifcConfigUtilities;

        public IssueSnapshotObjectiveConverter(
            IConverter<Issue, ObjectiveExternalDto> converterToDto,
            IfcConfigUtilities ifcConfigUtilities)
        {
            this.converterToDto = converterToDto;
            this.ifcConfigUtilities = ifcConfigUtilities;
        }

        public async Task<ObjectiveExternalDto> Convert(IssueSnapshot snapshot)
        {
            var parsedToDto = await converterToDto.Convert(snapshot.Entity);
            var typeField = DynamicFieldUtilities.CreateField(
                snapshot.ProjectSnapshot.IssueTypes[snapshot.Entity.Attributes.NgIssueSubtypeID].ID,
                new TypeSubtypeEnumCreator());
            var rootCauseEnumCreator = new RootCauseEnumCreator();
            var rootCause = snapshot.Entity.Attributes.RootCauseID == null
                ? DynamicFieldUtilities.CreateField(rootCauseEnumCreator.NullID, rootCauseEnumCreator)
                : DynamicFieldUtilities.CreateField(
                    snapshot.ProjectSnapshot.RootCauses[snapshot.Entity.Attributes.RootCauseID].ID,
                    rootCauseEnumCreator);
            parsedToDto.DynamicFields.Add(typeField);
            parsedToDto.DynamicFields.Add(rootCause);
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
                var otherInfo = snapshot.Entity.GetOtherInfo();

                if (otherInfo?.OriginalTargetUrn != null && snapshot.ProjectSnapshot.Items.TryGetValue(
                    otherInfo.OriginalTargetUrn,
                    out var originalTarget))
                {
                    parsedToDto.Location.Item = originalTarget.Entity.ToDto();
                    var config = await ifcConfigUtilities.GetConfig(
                        parsedToDto,
                        snapshot.ProjectSnapshot,
                        originalTarget);
                    var location = parsedToDto.Location.Location.ToVector();
                    var camera = parsedToDto.Location.CameraPosition.ToVector();
                    location += config.RedirectTo.Offset;
                    camera += config.RedirectTo.Offset;
                    parsedToDto.Location.Location = location.ToTuple();
                    parsedToDto.Location.CameraPosition = camera.ToTuple();
                }
                else
                {
                    parsedToDto.Location.Item = target.Entity.ToDto();
                }
            }

            return parsedToDto;
        }
    }
}
