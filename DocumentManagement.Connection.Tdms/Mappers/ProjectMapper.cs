﻿using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Mappers
{
    internal class ProjectMapper : IModelMapper<ProjectExternalDto, TDMSObject>
    {
        private readonly ItemHelper itemHelper = new ItemHelper();

        public ProjectExternalDto ToDto(TDMSObject tdmsObject)
        {
            var projectDto = new ProjectExternalDto()
            {
                Title = tdmsObject.Description,
                Items = itemHelper.GetItems(tdmsObject),
                ExternalID = tdmsObject.GUID,
            };

            return projectDto;
        }

        public TDMSObject ToModel(ProjectExternalDto objectDto, TDMSObject model)
        {
            // Can only modify files inside project
            itemHelper.SetItems(model, objectDto.Items);

            return model;
        }
    }
}