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
                //.ForMember(d => d.ProjectID, o => o.MapFrom(x => x.Project.ID))
                //    .ForMember(d => d.ParentObjectiveID, o => o.MapFrom(x => x.Parent.ID))
                //    .ForMember(d => d.BimElements, o => o.MapFrom(x => x.BimElements.Select(el => el.ToDto())))
                //.ForMember(d => d.DynamicFields, o => o.MapFrom(x => x.DynamicFields.Select(f => f.ToDto())));

        }
    }
}

        //ID = (ID<ObjectiveDto>)ob.ID,
        //        Author = new UserDto((ID<UserDto>)ob.Author.ID, ob.Author.Login, ob.Author.Name),
        //        CreationDate = ob.CreationDate,
        //        Description = ob.Description,
        //        Status = (ObjectiveStatus)ob.Status,
        //        ProjectID = (ID<ProjectDto>)ob.ProjectID,
        //        ParentObjectiveID = ob.ParentObjectiveID.HasValue
        //            ? ((ID<ObjectiveDto>?)ob.ParentObjectiveID)
        //            : null,
        //        DueDate = ob.DueDate,
        //        ObjectiveType = new ObjectiveTypeDto()
        //        {
        //            ID = (ID<ObjectiveTypeDto>)ob.ObjectiveType.ID,
        //            Name = ob.ObjectiveType.Name
        //        },
        //        Title = ob.Title,
        //        DynamicFields = ob.DynamicFields
        //            .Select(x => new DynamicFieldDto()
        //            {
        //                ID = (ID<DynamicFieldDto>)x.ID,
        //                Key = x.Key,
        //                Type = x.Type,
        //                Value = x.Value
        //            }).ToList(),
        //        BimElements = ob.BimElements
        //            .Select(x => new BimElementDto()
        //            {
        //                ItemID = (ID<ItemDto>)x.BimElement.ItemID,
        //                GlobalID = x.BimElement.GlobalID
        //            }).ToList()