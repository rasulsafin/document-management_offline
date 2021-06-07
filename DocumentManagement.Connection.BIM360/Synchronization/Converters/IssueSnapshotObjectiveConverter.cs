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
        private readonly IssuesService issuesService;
        private readonly ConverterAsync<Issue, ObjectiveExternalDto> convertToDtoAsync;
        private readonly ConverterAsync<IssueType, DynamicFieldExternalDto> convertTypeAsync;
        private readonly ItemsService itemsService;

        public IssueSnapshotObjectiveConverter(
            IssuesService issuesService,
            ConverterAsync<Issue, ObjectiveExternalDto> convertToDtoAsync,
            ConverterAsync<IssueType, DynamicFieldExternalDto> convertTypeAsync,
            ItemsService itemsService)
        {
            this.issuesService = issuesService;
            this.convertToDtoAsync = convertToDtoAsync;
            this.convertTypeAsync = convertTypeAsync;
            this.itemsService = itemsService;
        }

        public async Task<ObjectiveExternalDto> Convert(IssueSnapshot snapshot)
        {
            var types = await issuesService.GetIssueTypesAsync(snapshot.ProjectSnapshot.IssueContainer);
            var parsedToDto = await convertToDtoAsync(snapshot.Entity);
            var typeField = await convertTypeAsync(types.First(x => x.ID == snapshot.Entity.Attributes.NgIssueTypeID));
            parsedToDto.DynamicFields.Add(typeField);
            parsedToDto.ProjectExternalID = snapshot.ProjectSnapshot.Entity.ID;
            parsedToDto.Items ??= new List<ItemExternalDto>();

            foreach (var attachment in snapshot.Items)
                parsedToDto.Items.Add(attachment.Entity.ToDto());

            if (parsedToDto.Location != null)
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
