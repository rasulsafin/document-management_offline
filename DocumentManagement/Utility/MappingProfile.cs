using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMapIntToID();
            CreateMapIDToInt();
            CreateMapToDto();
            CreateMapToModel();
        }

        private void CreateMapToDto()
        {
            CreateMap<User, UserDto>();
            CreateMap<Project, ProjectDto>();
            CreateMap<Item, ItemDto>();
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Objective, ObjectiveToListDto>();
            CreateMap<Objective, ObjectiveDto>();
        }
        private void CreateMapToModel()
        {
            CreateMap<ObjectiveToCreateDto, Objective>()
                .ForMember(d => d.AuthorID, s => s.MapFrom(x => x.AuthorID.HasValue ? new int?((int)x.AuthorID) : null))
                .ForMember(d => d.ParentObjectiveID, s => s
                           .MapFrom(x => x.ParentObjectiveID.HasValue && x.ParentObjectiveID.Value.IsValid ? new int?((int)x.ParentObjectiveID) : null))
                .ForMember(d => d.DynamicFields, opt => opt.Ignore())
                .ForMember(d => d.BimElements, opt => opt.Ignore());
        }

        private void CreateMapIDToInt()
        {
            CreateMap<ID<ItemDto>, int>().ConvertUsing<IDTypeConverter<ItemDto>>();
            CreateMap<ID<ObjectiveDto>, int>().ConvertUsing<IDTypeConverter<ObjectiveDto>>();
            CreateMap<ID<ProjectDto>, int>().ConvertUsing<IDTypeConverter<ProjectDto>>();
            CreateMap<ID<UserDto>, int>().ConvertUsing<IDTypeConverter<UserDto>>();
            CreateMap<ID<BimElementDto>, int>().ConvertUsing<IDTypeConverter<BimElementDto>>();
            CreateMap<ID<DynamicFieldDto>, int>().ConvertUsing<IDTypeConverter<DynamicFieldDto>>();
            CreateMap<ID<ObjectiveTypeDto>, int>().ConvertUsing<IDTypeConverter<ObjectiveTypeDto>>();
            CreateMap<ID<RemoteConnectionInfoDto>, int>()
                    .ConvertUsing<IDTypeConverter<RemoteConnectionInfoDto>>();
        }

        private void CreateMapIntToID()
        {
            CreateMap<int, ID<ItemDto>>().ConvertUsing<IntIDTypeConverter<ItemDto>>();
            CreateMap<int, ID<ObjectiveDto>>().ConvertUsing<IntIDTypeConverter<ObjectiveDto>>();
            CreateMap<int, ID<ProjectDto>>().ConvertUsing<IntIDTypeConverter<ProjectDto>>();
            CreateMap<int, ID<UserDto>>().ConvertUsing<IntIDTypeConverter<UserDto>>();
            CreateMap<int, ID<BimElementDto>>().ConvertUsing<IntIDTypeConverter<BimElementDto>>();
            CreateMap<int, ID<DynamicFieldDto>>().ConvertUsing<IntIDTypeConverter<DynamicFieldDto>>();
            CreateMap<int, ID<ObjectiveTypeDto>>().ConvertUsing<IntIDTypeConverter<ObjectiveTypeDto>>();
            CreateMap<int, ID<RemoteConnectionInfoDto>>()
                    .ConvertUsing<IntIDTypeConverter<RemoteConnectionInfoDto>>();
        }
    }
}