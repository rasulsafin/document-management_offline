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
            CreateDynamicFieldMapToDto();

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

            CreateMap<EnumerationType, EnumerationTypeDto>()
                .ForMember(d => d.EnumerationValues, opt => opt.Ignore());
            CreateMap<EnumerationValue, EnumerationValueDto>();
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
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
        }

        private void CreateDynamicFieldMapToDto()
        {
            CreateMap<DynamicField, IDynamicFieldDto>()
                .ConvertUsing<DynamicFieldDtoTypeConverter>();

            CreateMap<DynamicField, DynamicFieldDto>();
            CreateMap<DynamicField, BoolFieldDto>();
            CreateMap<DynamicField, StringFieldDto>();
            CreateMap<DynamicField, IntFieldDto>();
            CreateMap<DynamicField, FloatFieldDto>();
            CreateMap<DynamicField, DateFieldDto>();
            CreateMap<DynamicField, EnumerationFieldDto>()
                 .ForMember(d => d.Value, o => o.MapFrom<DynamicFieldValuePropertyResolver>())
                 .ForMember(d => d.EnumerationType, o => o.MapFrom<DynamicFieldEnumerationTypePropertyResolver>());
        }

        private void CreateMapToModel()
        {
            CreateObjectiveMapToModel();
            CreateDynamicFieldToModel();

            CreateMap<ProjectDto, Project>()
                .ForMember(d => d.Items, o => o.Ignore());
            CreateMap<ProjectToCreateDto, Project>()
                .ForMember(d => d.Items, opt => opt.Ignore());

            CreateMap<BimElementDto, BimElement>();

            CreateMap<ObjectiveTypeDto, ObjectiveType>();

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
        }

        private void CreateObjectiveMapToModel()
        {
            CreateMap<ID<ObjectiveDto>?, int?>()
                .ConvertUsing<IDNullableIntTypeConverter<ObjectiveDto>>();
            CreateMap<ObjectiveToCreateDto, Objective>()
               .ForMember(d => d.DynamicFields, opt => opt.Ignore())
               .ForMember(d => d.BimElements, opt => opt.Ignore())
               .ForMember(d => d.Items, o => o.Ignore())
               .ForMember(d => d.ParentObjectiveID, opt => opt.Ignore());
            CreateMap<ObjectiveDto, Objective>()
                .ForMember(d => d.DynamicFields, opt => opt.Ignore())
                .ForMember(d => d.BimElements, opt => opt.Ignore())
                .ForMember(d => d.ParentObjectiveID, opt => opt.Ignore())
                .ForMember(d => d.Items, o => o.Ignore());
        }

        private void CreateDynamicFieldToModel()
        {
            CreateMap<IDynamicFieldDto, DynamicField>()
                   .ConvertUsing<DynamicFieldTypeConverter>();

            CreateMap<DynamicFieldDto, DynamicField>()
                .ForMember(d => d.Value, o => o.Ignore());
            CreateMap<BoolFieldDto, DynamicField>();
            CreateMap<StringFieldDto, DynamicField>();
            CreateMap<IntFieldDto, DynamicField>();
            CreateMap<FloatFieldDto, DynamicField>();
            CreateMap<DateFieldDto, DynamicField>();
            CreateMap<EnumerationFieldDto, DynamicField>()
                .ForMember(d => d.Value, o => o.MapFrom(s => s.Value.ID));
        }

        private void CreateMapForExternal()
        {
            CreateMap<ProjectExternalDto, Project>();
            CreateMap<Project, ProjectExternalDto>();

            CreateMap<ObjectiveExternalDto, Objective>()
               .ForMember(x => x.ProjectID, o => o.MapFrom<ObjectiveExternalDtoProjectIdResolver>())
               .ForMember(x => x.Project, o => o.MapFrom<ObjectiveExternalDtoProjectResolver>())
               .ForMember(x => x.ObjectiveTypeID, o => o.MapFrom<ObjectiveExternalDtoObjectiveTypeIDResolver>());
            CreateMap<Objective, ObjectiveExternalDto>()
               .ForMember(x => x.ProjectExternalID, o => o.MapFrom<ObjectiveProjectIDResolver>())
               .ForMember(x => x.ObjectiveType, o => o.MapFrom<ObjectiveObjectiveTypeResolver>())
               .ForMember(x => x.Items, o => o.MapFrom(ex => ex.Items.Select(x => x.Item)))
               .ForMember(x => x.BimElements, o => o.MapFrom(ex => ex.BimElements.Select(x => x.BimElement)));

            CreateMap<Item, ItemExternalDto>();
            CreateMap<ItemExternalDto, Item>()
               .ForMember(x => x.RelativePath, o => o.MapFrom(x => x.FileName));
            CreateMap<ItemExternalDto, ObjectiveItem>()
               .ForMember(x => x.Item, o => o.MapFrom(x => x));

            CreateMap<BimElement, BimElementExternalDto>();
            CreateMap<BimElementExternalDto, BimElementObjective>()
               .ConvertUsing<BimElementObjectiveTypeConverter>();
            CreateMap<BimElementExternalDto, BimElement>();

            CreateMap<DynamicField, DynamicFieldExternalDto>()
                .ForMember(x => x.Value, o => o.MapFrom<DynamicFieldValueResolver>());
            CreateMap<DynamicFieldExternalDto, DynamicField>()
                .ForMember(x => x.Value, o => o.MapFrom<DynamicFieldExternalDtoValueResolver>());

            CreateMap<ConnectionInfo, ConnectionInfoExternalDto>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoAuthFieldValuesResolver>())
                .ForMember(d => d.EnumerationTypes, o => o.MapFrom(s => s.EnumerationTypes.Select(x => x.EnumerationType)));
            CreateMap<ConnectionInfoExternalDto, ConnectionInfo>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom<ConnectionInfoDtoAuthFieldValuesResolver>())
                .ForMember(d => d.EnumerationTypes, o => o.Ignore());

            CreateMap<ConnectionType, ConnectionTypeExternalDto>()
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => s.AuthFieldNames.Select(x => x.Name)))
                .ForMember(d => d.AppProperties, o => o.MapFrom<ConnectionTypeAppPropertiesResolver>());
            CreateMap<ConnectionTypeExternalDto, ConnectionType>()
             .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => s.AuthFieldNames.Select(name => new AuthFieldName() { Name = name })))
             .ForMember(d => d.AppProperties, o => o.MapFrom<ConnectionTypeDtoAppPropertiesResolver>());

            CreateMap<ObjectiveType, ObjectiveTypeExternalDto>();
            CreateMap<ObjectiveTypeExternalDto, ObjectiveType>();

            CreateMap<EnumerationType, EnumerationTypeExternalDto>();
            CreateMap<EnumerationTypeExternalDto, EnumerationType>();

            CreateMap<EnumerationValue, EnumerationValueExternalDto>();
            CreateMap<EnumerationValueExternalDto, EnumerationValue>();
        }
    }
}
