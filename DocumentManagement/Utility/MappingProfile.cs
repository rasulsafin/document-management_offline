using System.Collections.Generic;
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
        }

        private void CreateMapToDto()
        {
            CreateMap<User, UserDto>();
            CreateMap<Project, ProjectDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)));
            CreateMap<Project, ProjectToListDto>();
               // .ForMember(d => d.ID, a => a.MapFrom(x => x.ID));
            CreateMap<Item, ItemDto>();
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Objective, ObjectiveToListDto>();
            CreateMap<Objective, ObjectiveDto>()
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.Select(i => i.Item)))
                .ForMember(d => d.BimElements, o => o.MapFrom(s => s.BimElements.Select(i => i.BimElement)));
            CreateMap<ConnectionInfo, RemoteConnectionInfoDto>()
                .ForMember(d => d.ServiceName, o => o.MapFrom(x => x.Name))
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(x => DecodeAuthFieldNames(x.AuthFieldNames)));
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            // CreateMap<Project, ProjectDto>();
            CreateMap<User, UserDto>();
            CreateMap<DynamicField, DynamicFieldDto>();
            CreateMap<BimElement, BimElementDto>();
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
            CreateMap<ItemDto, Item>();           
        }

        private static List<string> DecodeAuthFieldNames(string encoded)
        {
            var names = new List<string>();
            if (!string.IsNullOrEmpty(encoded))
                names = System.Text.Json.JsonSerializer.Deserialize<List<string>>(encoded);
            return names;
        }
    }
}