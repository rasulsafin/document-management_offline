using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    internal class ProjectMapper : IModelMapper<ProjectExternalDto, TDMSObject>
    {
        private readonly ItemMapper mapper = new ItemMapper();

        public ProjectExternalDto ToDto(TDMSObject tdmsObject)
        {
            var projectDto = new ProjectExternalDto()
            {
                Title = tdmsObject.Description,
                Items = GetItems(tdmsObject),
                ExternalID = tdmsObject.GUID,
            };

            return projectDto;
        }

        public TDMSObject ToModel(ProjectExternalDto objectDto, TDMSObject model)
        {
            ///TODO: Swap MAIN_OBJECT to OBJECT_OBJECT?
            model.Attributes[AttributeID.NAME].Value = objectDto.Title;
            model.Attributes[AttributeID.START_DATE].Value = DateTime.Now;
            model.Attributes[AttributeID.DUE_DATE].Value = DateTime.Now;

            var modelsChild = model.Objects.Create(ObjectTypeID.OBJECT);
            model.Update();
            modelsChild.Update();

            ///TODO: OBJECT_OBJECT?
            modelsChild.Attributes[AttributeID.OBJECT_NAME].Value = objectDto.Title;
            modelsChild.Update();

            // TODO: Add Items too.
            return model;
        }

        private IEnumerable<ItemExternalDto> GetItems(TDMSObject tdmsObject) => tdmsObject.Files.Cast<TDMSFile>().Select(x => mapper.ToDto(x));

    }
}
