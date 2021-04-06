using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMapToDto();
            CreateMapToModel();
            CreateMapForExternal();
        }

        private void CreateMapToDto()
        {
            CreateObjectiveMapToDto();

            CreateMap<User, UserDto>();

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
        }

        private void CreateObjectiveMapToDto()
        {
            CreateMap<Objective, ObjectiveToListDto>()
               .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<Objective, ObjectiveDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)))
                .ForMember(d => d.DynamicFields, o => o.Ignore());
            CreateMap<Objective, ObjectiveToReportDto>()
                .ForMember(d => d.ID, opt => opt.Ignore())
                .ForMember(d => d.Author, o => o.MapFrom(s => s.Author.Name))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)
                                                                    .Where(x => x.ItemType == (int)ItemType.Media
                                                                                && !x.RelativePath.EndsWith(".mp4"))))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
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
        }

        private void CreateObjectiveMapToModel()
        {
            CreateMap<ID<ObjectiveDto>?, int?>()
                .ConvertUsing<IDNullableIntTypeConverter<ObjectiveDto>>();
            CreateMap<ObjectiveToCreateDto, Objective>()
               .ForMember(d => d.DynamicFields, opt => opt.Ignore())
               .ForMember(d => d.BimElements, opt => opt.Ignore())
               .ForMember(d => d.Items, o => o.Ignore())
               .ForMember(d => d.ParentObjectiveID, o => o.MapFrom(s => (int?)s.ParentObjectiveID));
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
               .ForMember(x => x.Project, o => o.MapFrom<ObjectiveExternalDtoProjectResolver>())
               .ForMember(x => x.ObjectiveTypeID, o => o.MapFrom<ObjectiveExternalDtoObjectiveTypeIdResolver>())
               .ForMember(d => d.AuthorID, с => с.MapFrom<ObjectiveExternalDtoAuthorIdResolver>())
               .ForMember(d => d.Author, с => с.MapFrom<ObjectiveExternalDtoAuthorResolver>());
            CreateMap<Objective, ObjectiveExternalDto>()
               .ForMember(x => x.ProjectExternalID, o => o.MapFrom<ObjectiveProjectIDResolver>())
               .ForMember(x => x.ObjectiveType, o => o.MapFrom<ObjectiveObjectiveTypeResolver>())
               .ForMember(x => x.Items, o => o.MapFrom(ex => ex.Items == null ? null : ex.Items.Select(x => x.Item)))
               .ForMember(x => x.BimElements, o => o.MapFrom(ex => ex.BimElements.Select(x => x.BimElement)))
               .ForMember(x => x.AuthorExternalID, с => с.MapFrom(d => d.Author.ExternalID));

            CreateMap<Item, ItemExternalDto>()
                .ForMember(x => x.FileName, o => o.MapFrom<ItemFileNameResolver>())
                .ForMember(x => x.FullPath, o => o.MapFrom<ItemFullPathResolver>());
            CreateMap<ItemExternalDto, Item>()
               .ForMember(x => x.RelativePath, o => o.MapFrom<ItemExternalDtoRelativePathResolver>());
            CreateMap<ItemExternalDto, ObjectiveItem>()
               .ForMember(x => x.Item, o => o.MapFrom(x => x));

            CreateMap<BimElement, BimElementExternalDto>();
            CreateMap<BimElementExternalDto, BimElementObjective>()
               .ConvertUsing<BimElementObjectiveTypeConverter>();
            CreateMap<BimElementExternalDto, BimElement>();

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
                .ForMember(d => d.ID, o => o.Ignore());

            CreateMap<EnumerationValue, EnumerationValueExternalDto>();
            CreateMap<EnumerationValueExternalDto, EnumerationValue>();
        }
    }
}
