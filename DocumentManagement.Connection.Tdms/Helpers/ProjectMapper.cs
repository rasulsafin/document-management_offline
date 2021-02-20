using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    internal class ProjectMapper : IModelMapper<ProjectDto, TDMSObject>
    {
        private readonly ItemMapper mapper = new ItemMapper();

        public ProjectDto ToDto(TDMSObject tdmsObject)
        {
            var projectDto = new ProjectDto()
            {
                Title = tdmsObject.Description,
                Items = GetItems(tdmsObject),
            };

            return projectDto;
        }

        public TDMSObject ToModel(ProjectDto objectDto, TDMSObject model)
        {
            model.Attributes[AttributeID.NAME].Value = objectDto.Title;
            model.Attributes[AttributeID.START_DATE].Value = DateTime.Now;
            model.Attributes[AttributeID.DUE_DATE].Value = DateTime.Now;

            var modelsChild = model.Objects.Create(ObjectTypeID.OBJECT);
            model.Update();
            modelsChild.Update();

            modelsChild.Attributes[AttributeID.OBJECT_NAME].Value = objectDto.Title;
            modelsChild.Update();

            // TODO: Add Items too.
            return model;
        }

        private IEnumerable<ItemDto> GetItems(TDMSObject tdmsObject) => tdmsObject.Files.Cast<TDMSFile>().Select(x => mapper.ToDto(x));

    }
}
