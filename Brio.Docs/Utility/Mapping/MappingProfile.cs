using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi.Project;
using Brio.Docs.Client.Dtos.ForApi.Projects;
using Brio.Docs.Client.Dtos.ForApi;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Utility.Mapping.Converters;
using Brio.Docs.Utility.Mapping.Resolvers;

namespace Brio.Docs.Utility.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMapToDto();
            CreateMapToModel();
            CreateMapForExternal();
            CreateMapForProjectApi();
        }

        private void CreateMapToDto()
        {
            CreateObjectiveMapToDto();

            CreateMap<User, UserDto>()
                .ForMember(d => d.ConnectionName, o => o.MapFrom(s => GetName(s)));

            CreateMap<Project, ProjectDto>();
            CreateMap<Project, ProjectToListDto>();

            CreateMap<Item, ItemDto>();

            CreateMap<ConnectionType, ConnectionTypeDto>()
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => s.AuthFieldNames.Select(x => x.Name)))
                .ForMember(d => d.AppProperties, o => o.MapFrom<ConnectionTypeAppPropertiesResolver>());
            CreateMap<ConnectionInfo, ConnectionInfoDto>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoAuthFieldValuesResolver>())
                .ForMember(d => d.EnumerationTypes, o => o.Ignore());

            CreateMap<ObjectiveType, ObjectiveTypeDto>();

            CreateMap<BimElement, BimElementDto>();

            CreateMap<EnumerationType, EnumerationTypeDto>();
            CreateMap<EnumerationValue, EnumerationValueDto>();

            CreateMap<DynamicField, DynamicFieldDto>()
               .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldModelToDtoValueResolver>())
               .ForMember(d => d.Key, o => o.MapFrom(x => x.ExternalID));

            CreateMap<DynamicFieldInfo, DynamicFieldDto>()
                .ForMember(d => d.ID, o => o.MapFrom(s => new ID<DynamicFieldDto>()))
                .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldModelToDtoValueResolver>())
                .ForMember(d => d.Key, o => o.MapFrom(x => x.ExternalID));

            CreateMap<Location, LocationDto>()
               .ForMember(
                    d => d.Position,
                    o => o.MapFrom(
                        s => new Tuple<double, double, double>(s.PositionX, s.PositionY, s.PositionZ).ToValueTuple()))
               .ForMember(
                    d => d.CameraPosition,
                    o => o.MapFrom(
                        s => new Tuple<double, double, double>(s.CameraPositionX, s.CameraPositionY, s.CameraPositionZ)
                           .ToValueTuple()));
        }

        private void CreateObjectiveMapToDto()
        {
            CreateMap<Objective, ObjectiveToListDto>()
               .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<Objective, ObjectiveToSelectionDto>()
               .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<Objective, ObjectiveDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)))
                .ForMember(d => d.DynamicFields, o => o.ConvertUsing<DynamicFieldModelToDtoConverter, ICollection<DynamicField>>());
            CreateMap<Objective, ObjectiveToLocationDto>();
            CreateMap<Objective, SubobjectiveDto>();
        }

        private void CreateMapToModel()
        {
            CreateObjectiveMapToModel();

            CreateMap<ProjectDto, Project>()
                .ForMember(d => d.Items, o => o.Ignore());
            CreateMap<ProjectToCreateDto, Project>()
                .ForMember(d => d.Items, opt => opt.Ignore());

            CreateMap<BimElementDto, BimElement>();

            CreateMap<ObjectiveTypeDto, ObjectiveType>();
            CreateMap<string, ObjectiveType>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s));

            CreateMap<UserToCreateDto, User>();

            CreateMap<ConnectionTypeDto, ConnectionType>()
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => s.AuthFieldNames.Select(name => new AuthFieldName() { Name = name })))
                .ForMember(d => d.AppProperties, o => o.MapFrom<ConnectionTypeDtoAppPropertiesResolver>())
                .ForMember(d => d.EnumerationTypes, opt => opt.Ignore());

            CreateMap<ItemDto, Item>();

            CreateMap<ConnectionInfoDto, ConnectionInfo>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoDtoAuthFieldValuesResolver>())
                .ForMember(d => d.EnumerationTypes, opt => opt.Ignore());
            CreateMap<ConnectionInfoToCreateDto, ConnectionInfo>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoDtoAuthFieldValuesResolver>());

            CreateMap<EnumerationTypeDto, EnumerationType>()
                .ForMember(d => d.EnumerationValues, opt => opt.Ignore());
            CreateMap<EnumerationValueDto, EnumerationValue>();

            CreateMap<DynamicFieldDto, DynamicField>()
                .ForMember(d => d.ExternalID, o => o.MapFrom(s => s.Key))
                .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldDtoToModelValueResolver>());

            CreateMap<DynamicFieldDto, DynamicFieldInfo>()
                .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldDtoToModelValueResolver>());

            CreateMap<LocationDto, Location>()
               .ForMember(d => d.PositionX, o => o.MapFrom(s => s.Position.x))
               .ForMember(d => d.PositionY, o => o.MapFrom(s => s.Position.y))
               .ForMember(d => d.PositionZ, o => o.MapFrom(s => s.Position.z))
               .ForMember(d => d.CameraPositionX, o => o.MapFrom(s => s.CameraPosition.x))
               .ForMember(d => d.CameraPositionY, o => o.MapFrom(s => s.CameraPosition.y))
               .ForMember(d => d.CameraPositionZ, o => o.MapFrom(s => s.CameraPosition.z))
               .ForMember(d => d.Item, o => o.Ignore());
        }

        private void CreateObjectiveMapToModel()
        {
            CreateMap<ID<ObjectiveDto>?, int?>()
                .ConvertUsing<IDNullableIntTypeConverter<ObjectiveDto>>();
            CreateMap<ObjectiveToCreateDto, Objective>()
               .ForMember(d => d.DynamicFields, opt => opt.Ignore())
               .ForMember(d => d.BimElements, opt => opt.Ignore())
               .ForMember(d => d.Items, o => o.Ignore())
               .ForMember(d => d.ParentObjectiveID, o => o.MapFrom(s => (int?)s.ParentObjectiveID))
               .ForMember(d => d.Location, o => o.Ignore());
            CreateMap<ObjectiveDto, Objective>()
               .ForMember(d => d.DynamicFields, opt => opt.Ignore())
               .ForMember(d => d.BimElements, opt => opt.Ignore())
               .ForMember(d => d.Items, o => o.Ignore())
               .ForMember(d => d.ParentObjectiveID, o => o.MapFrom(s => (int?)s.ParentObjectiveID));
        }

        private void CreateMapForExternal()
        {
            CreateMap<ProjectExternalDto, Project>();
            CreateMap<Project, ProjectExternalDto>();

            CreateMap<ObjectiveExternalDto, Objective>()
               .ForMember(x => x.ProjectID, o => o.MapFrom<ObjectiveExternalDtoProjectIdResolver>())
               .ForMember(x => x.Project, o => o.Ignore())
               .ForMember(x => x.ObjectiveTypeID, o => o.MapFrom<ObjectiveExternalDtoObjectiveTypeIdResolver>())
               .ForMember(d => d.AuthorID, с => с.MapFrom<ObjectiveExternalDtoAuthorIdResolver>())
               .ForMember(d => d.Author, с => с.Ignore());
            CreateMap<Objective, ObjectiveExternalDto>()
               .ForMember(x => x.ProjectExternalID, o => o.MapFrom<ObjectiveProjectIDResolver>())
               .ForMember(x => x.ObjectiveType, o => o.MapFrom<ObjectiveObjectiveTypeResolver>())
               .ForMember(x => x.Items, o => o.MapFrom(ex => ex.Items == null ? null : ex.Items.Select(x => x.Item)))
               .ForMember(x => x.BimElements, o => o.MapFrom(ex => ex.BimElements.Select(x => x.BimElement)))
               .ForMember(x => x.AuthorExternalID, с => с.MapFrom(d => d.Author.ExternalID));

            CreateMap<Item, ItemExternalDto>()
                .ForMember(x => x.ProjectDirectory, o => o.MapFrom<ItemProjectDirectoryResolver>());
            CreateMap<ItemExternalDto, Item>();
            CreateMap<ItemExternalDto, ObjectiveItem>()
               .ForMember(x => x.Item, o => o.MapFrom(x => x));

            CreateMap<BimElement, BimElementExternalDto>();
            CreateMap<BimElementExternalDto, BimElementObjective>()
               .ConvertUsing<BimElementObjectiveTypeConverter>();
            CreateMap<BimElementExternalDto, BimElement>()
               .ForMember(x => x.ID, o => o.Ignore())
               .ForMember(x => x.Objectives, o => o.Ignore());

            CreateMap<DynamicField, DynamicFieldExternalDto>()
                .ForMember(x => x.Value, o => o.MapFrom<DynamicFieldModelToExternalValueResolver>());
            CreateMap<DynamicFieldExternalDto, DynamicField>()
                .ForMember(x => x.Value, o => o.MapFrom<DynamicFieldExternalToModelValueResolver>());

            CreateMap<DynamicFieldInfo, DynamicFieldExternalDto>()
                  .ForMember(x => x.Value, o => o.MapFrom<DynamicFieldModelToExternalValueResolver>());
            CreateMap<DynamicFieldExternalDto, DynamicFieldInfo>()
                  .ForMember(x => x.Value, o => o.MapFrom<DynamicFieldExternalToModelValueResolver>());

            CreateMap<ConnectionInfo, ConnectionInfoExternalDto>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoAuthFieldValuesResolver>())
                .ForMember(d => d.EnumerationTypes, o => o.MapFrom(s => s.EnumerationTypes.Select(x => x.EnumerationType)));
            CreateMap<ConnectionInfoExternalDto, ConnectionInfo>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoDtoAuthFieldValuesResolver>())
                .ForMember(d => d.EnumerationTypes, o => o.Ignore())
                .ForMember(d => d.ConnectionType, o => o.Ignore());

            CreateMap<ConnectionType, ConnectionTypeExternalDto>()
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => s.AuthFieldNames.Select(x => x.Name)))
                .ForMember(d => d.AppProperties, o => o.MapFrom<ConnectionTypeAppPropertiesResolver>());
            CreateMap<ConnectionTypeExternalDto, ConnectionType>()
             .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => s.AuthFieldNames.Select(name => new AuthFieldName() { Name = name })))
             .ForMember(d => d.AppProperties, o => o.MapFrom<ConnectionTypeDtoAppPropertiesResolver>())
             .ForMember(d => d.EnumerationTypes, o => o.Ignore())
             .ForMember(d => d.ObjectiveTypes, o => o.Ignore());

            CreateMap<ObjectiveType, ObjectiveTypeExternalDto>();
            CreateMap<ObjectiveTypeExternalDto, ObjectiveType>()
               .ForMember(d => d.ID, o => o.Ignore());

            CreateMap<EnumerationType, EnumerationTypeExternalDto>();
            CreateMap<EnumerationTypeExternalDto, EnumerationType>()
               .ForMember(d => d.ID, o => o.Ignore())
               .ForMember(d => d.EnumerationValues, o => o.Ignore());

            CreateMap<EnumerationValue, EnumerationValueExternalDto>();
            CreateMap<EnumerationValueExternalDto, EnumerationValue>();

            CreateMap<Location, LocationExternalDto>()
               .ForMember(
                    d => d.Location,
                    o => o.MapFrom(
                        e => new Tuple<double, double, double>(e.PositionX, e.PositionY, e.PositionZ).ToValueTuple()))
               .ForMember(
                    d => d.CameraPosition,
                    o => o.MapFrom(
                        e => new Tuple<double, double, double>(e.CameraPositionX, e.CameraPositionY, e.CameraPositionZ)
                           .ToValueTuple()))
               .ForMember(d => d.Guid, o => o.MapFrom(s => s.Guid));
            CreateMap<LocationExternalDto, Location>()
               .ForMember(d => d.PositionX, o => o.MapFrom(s => s.Location.x))
               .ForMember(d => d.PositionY, o => o.MapFrom(s => s.Location.y))
               .ForMember(d => d.PositionZ, o => o.MapFrom(s => s.Location.z))
               .ForMember(d => d.CameraPositionX, o => o.MapFrom(s => s.CameraPosition.x))
               .ForMember(d => d.CameraPositionY, o => o.MapFrom(s => s.CameraPosition.y))
               .ForMember(d => d.CameraPositionZ, o => o.MapFrom(s => s.CameraPosition.z))
               .ForMember(d => d.Guid, o => o.MapFrom(s => s.Guid));
        }

        private void CreateMapForProjectApi()
        {
            // TODO: CreatedById, UpdatedById ant etc.
            CreateMap<ItemDto, ItemForApiDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (long)src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RelativePath))
                .ForMember(dest => dest.RelativePath, opt => opt.MapFrom(src => src.RelativePath));

            CreateMap<ItemForApiDto, ItemDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => (ID<ItemDto>)src.Id))
                .ForMember(dest => dest.RelativePath, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.RelativePath, opt => opt.MapFrom(src => src.RelativePath));

            CreateMap<ProjectDto, ProjectToUpdateForApi>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (long)src.ID));

            // TODO Add ProjectId logic somehow.

            CreateMap<ProjectToCreateDto, ProjectToCreateForApiDto>()
                .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => (long)src.AuthorID))
                .ForMember(dest => dest.UpdatedById, opt => opt.MapFrom(src => (long)src.AuthorID))
                ;

            CreateMap<ProjectToListDto, ProjectToReadForApiDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (long)src.ID));

            CreateMap<ProjectToListDto, ProjectToReadForApiDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID));
        }

        private string GetName(User s)
        {
            var name = s.ConnectionInfo?.ConnectionType?.Name;
            return name;
        }
    }
}
