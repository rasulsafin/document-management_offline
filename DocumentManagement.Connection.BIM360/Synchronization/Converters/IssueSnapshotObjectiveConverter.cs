using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueSnapshotObjectiveConverter : IConverter<IssueSnapshot, ObjectiveExternalDto>
    {
        private readonly IConverter<Issue, ObjectiveExternalDto> converterToDto;

        public IssueSnapshotObjectiveConverter(
            IConverter<Issue, ObjectiveExternalDto> converterToDto)
        {
            this.converterToDto = converterToDto;
        }

        public async Task<ObjectiveExternalDto> Convert(IssueSnapshot snapshot)
        {
            var parsedToDto = await converterToDto.Convert(snapshot.Entity);
            var typeField = DynamicFieldUtilities.CreateField(
                snapshot.ProjectSnapshot.IssueTypes[snapshot.Entity.Attributes.NgIssueSubtypeID].ID,
                new TypeSubtypeEnumCreator());
            var rootCause = DynamicFieldUtilities.CreateField(
                snapshot.ProjectSnapshot.RootCauses[snapshot.Entity.Attributes.RootCauseID].ID,
                new RootCauseEnumCreator());
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
                parsedToDto.Location.Item = target.Entity.ToDto();

            return parsedToDto;
        }
    }
}
