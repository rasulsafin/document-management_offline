using AutoMapper;
using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using System;
using System.Linq;
using System.Net;

namespace DocumentManagement.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<UserDb, User>();
            CreateMap<User, UserDb>();

            CreateMap<Project, ProjectDb>()
             .ForMember(dest => dest.Items,
                 opt => opt.Ignore())
             .ForMember(dest => dest.Tasks,
                opt => opt.Ignore());
            CreateMap<ProjectDb, Project>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.Items.Select(t => t.Item)));

            CreateMap<Item, ProjectItems>()
                .ForMember(dest => dest.Item, opt => opt.MapFrom(i => i));
            CreateMap<Item, TaskItems>()
               .ForMember(dest => dest.Item, opt => opt.MapFrom(i => i));

            CreateMap<TaskDmDb, TaskDm>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.Items.Select(t => t.Item)))
                .ForMember(dest => dest.Location,
                opt => opt.MapFrom(src =>
                    new ValueTuple<float, float, float>(src.LocationX, src.LocationY, src.LocationZ)));

            CreateMap<TaskDm, TaskDmDb>()
              .ForMember(dest => dest.Author,
                 opt => opt.MapFrom(src => src.Author))
              .ForMember(dest => dest.LocationX,
                opt => opt.MapFrom(src => src.Location.Item1))
              .ForMember(dest => dest.LocationY,
                opt => opt.MapFrom(src => src.Location.Item2))
              .ForMember(dest => dest.LocationZ,
                opt => opt.MapFrom(src => src.Location.Item3))
              .ForMember(dest => dest.Items,
                 opt => opt.Ignore())
              .ForMember(dest => dest.Tasks,
                opt => opt.Ignore());

            CreateMap<ItemDb, Item>();
            CreateMap<Item, ItemDb>();

        }
    }
   
}
