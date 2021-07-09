using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueSnapshotObjectiveConverter : IConverter<IssueSnapshot, ObjectiveExternalDto>
    {
        private readonly IConverter<Issue, ObjectiveExternalDto> converterToDto;
        private readonly IConverter<IssueTypeSnapshot, DynamicFieldExternalDto> typeConverter;
        private readonly ItemsService itemsService;

        public IssueSnapshotObjectiveConverter(
            IConverter<Issue, ObjectiveExternalDto> converterToDto,
            IConverter<IssueTypeSnapshot, DynamicFieldExternalDto> typeConverter,
            ItemsService itemsService)
        {
            this.converterToDto = converterToDto;
            this.typeConverter = typeConverter;
            this.itemsService = itemsService;
        }

        public async Task<ObjectiveExternalDto> Convert(IssueSnapshot snapshot)
        {
            var parsedToDto = await converterToDto.Convert(snapshot.Entity);
            var typeField = await typeConverter.Convert(
                snapshot.ProjectSnapshot.IssueTypes[snapshot.Entity.Attributes.NgIssueSubtypeID]);
            parsedToDto.DynamicFields.Add(typeField);
            parsedToDto.ProjectExternalID = snapshot.ProjectSnapshot.Entity.ID;

            if (snapshot.Items != null)
            {
                parsedToDto.Items ??= new List<ItemExternalDto>();

                foreach (var attachment in snapshot.Items)
                    parsedToDto.Items.Add(attachment.Entity.ToDto());
            }

            if (parsedToDto.Location != null && snapshot.Entity.Attributes.TargetUrn != null)
            {
                var target = await itemsService.GetAsync(
                    snapshot.ProjectSnapshot.ID,
                    snapshot.Entity.Attributes.TargetUrn);
                parsedToDto.Location.Item = target.item.ToDto();
            }

            return parsedToDto;
        }
    }
}
