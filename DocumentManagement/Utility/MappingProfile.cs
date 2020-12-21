using AutoMapper;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Utility
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<Project, ProjectDto>();
            CreateMap<Item, ItemDto>();
            CreateMap<ObjectiveType, ObjectiveTypeDto>();
            CreateMap<Objective, ObjectiveToListDto>();
            CreateMap<Objective, ObjectiveDto>();
            CreateMap<ObjectiveToCreateDto, Objective>()
                .ForMember(d => d.ObjectiveTypeID, s=>s.MapFrom(x => (int)x.ObjectiveTypeID))
                .ForMember(d => d.Status, s=>s.MapFrom(x=> (int)x.Status))
                .ForMember(d => d.ProjectID, s=>s.MapFrom(x=> (int)x.ProjectID))
                .ForMember(d => d.AuthorID, s => s.MapFrom(x => x.AuthorID.HasValue ? new int?((int)x.AuthorID) : null))
                .ForMember(d => d.ParentObjectiveID, s => s
                           .MapFrom(x => x.ParentObjectiveID.HasValue && x.ParentObjectiveID.Value.IsValid ? new int?((int)x.ParentObjectiveID) : null))
                .ForMember(d => d.DynamicFields, opt => opt.Ignore())
                .ForMember(d => d.BimElements, opt => opt.Ignore());
                //.ForMember(d => d.ProjectID, o => o.MapFrom(x => x.Project.ID))
                //    .ForMember(d => d.ParentObjectiveID, o => o.MapFrom(x => x.Parent.ID))
                //    .ForMember(d => d.BimElements, o => o.MapFrom(x => x.BimElements.Select(el => el.ToDto())))
                //.ForMember(d => d.DynamicFields, o => o.MapFrom(x => x.DynamicFields.Select(f => f.ToDto())));

        }
    }
}