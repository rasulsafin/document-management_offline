using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        }

        private static T Deserialize<T>(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return default;

            return JsonSerializer.Deserialize<T>(encoded);
        }

        private static string Serialize<T>(T obj)
        {
            if (obj == null)
                return string.Empty;

            return JsonSerializer.Serialize(obj);
        }

        private void CreateMapToDto()
        {
            CreateMap<User, UserDto>();
            CreateMap<Project, ProjectDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)));
            CreateMap<Project, ProjectToListDto>();
            CreateMap<Item, ItemDto>();
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Objective, ObjectiveToListDto>()
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<Objective, ObjectiveDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<Objective, ObjectiveToReportDto>()
                .ForMember(d => d.ID, opt => opt.Ignore())
                .ForMember(d => d.Author, o => o.MapFrom(s => s.Author.Name))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<ConnectionInfo, ConnectionInfoDto>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom(s => Deserialize<Dictionary<string, string>>(s.AuthFieldValues)));
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<User, UserDto>();
            CreateMap<DynamicField, DynamicFieldDto>();
            CreateMap<BimElement, BimElementDto>();
            CreateMap<ConnectionType, ConnectionTypeDto>()
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => Deserialize<List<string>>(s.AuthFieldNames)))
                .ForMember(d => d.AppProperty, o => o.MapFrom(s => Deserialize<Dictionary<string, string>>(s.AppProperty)));
        }

        private void CreateMapToModel()
        {
            CreateMap<ProjectDto, Project>()
                .ForMember(d => d.Items, o => o.Ignore());
            CreateMap<ProjectToCreateDto, Project>()
                .ForMember(d => d.Items, opt => opt.Ignore());
            CreateMap<ID<ObjectiveDto>?, int?>()
                .ConvertUsing<IDNullableIntTypeConverter<ObjectiveDto>>();
            CreateMap<ObjectiveToCreateDto, Objective>()
                .ForMember(d => d.DynamicFields, opt => opt.Ignore())
                .ForMember(d => d.BimElements, opt => opt.Ignore())
                .ForMember(d => d.Items, o => o.Ignore());
            CreateMap<ObjectiveDto, Objective>()
                .ForMember(d => d.DynamicFields, opt => opt.Ignore())
                .ForMember(d => d.BimElements, opt => opt.Ignore())
                .ForMember(d => d.Items, o => o.Ignore());
            CreateMap<BimElementDto, BimElement>();
            CreateMap<ObjectiveTypeDto, ObjectiveType>();
            CreateMap<DynamicFieldToCreateDto, DynamicField>();
            CreateMap<DynamicFieldDto, DynamicField>();
            CreateMap<UserToCreateDto, User>();
            CreateMap<ConnectionTypeDto, ConnectionType>()
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(s => Serialize(s.AuthFieldNames)))
                .ForMember(d => d.AppProperty, o => o.MapFrom(s => Serialize(s.AppProperty)));
            CreateMap<ItemDto, Item>();
            CreateMap<ConnectionInfoDto, ConnectionInfo>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom(s => Serialize(s.AuthFieldValues)));
            CreateMap<ConnectionInfoToCreateDto, ConnectionInfo>()
                .ForMember(d => d.AuthFieldValues, o => o.MapFrom(s => Serialize(s.AuthData)));
        }
    }
}
