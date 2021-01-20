using System.Collections.Generic;
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
            CreateMap<Project, ProjectDto>();
            CreateMap<Project, ProjectToListDto>();
               // .ForMember(d => d.ID, a => a.MapFrom(x => x.ID));
            CreateMap<Item, ItemDto>();
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Objective, ObjectiveToListDto>();
            CreateMap<Objective, ObjectiveDto>();
            CreateMap<ConnectionInfo, RemoteConnectionInfoDto>()
                .ForMember(d => d.ServiceName, o => o.MapFrom(x => x.Name))
                .ForMember(d => d.AuthFieldNames, o => o.MapFrom(x => DecodeAuthFieldNames(x.AuthFieldNames)));
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Project, ProjectDto>();
            CreateMap<User, UserDto>();
            CreateMap<DynamicField, DynamicFieldDto>();
        }
        private void CreateMapToModel()
        {
            CreateMap<ProjectDto, Project>();
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
            CreateMap<DynamicFieldToCreateDto, DynamicField>();
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