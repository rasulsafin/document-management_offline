using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveExternalDtoAuthorResolver : IValueResolver<ObjectiveExternalDto, Objective, User>
    {
        private readonly DMContext dbContext;

        public ObjectiveExternalDtoAuthorResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public User Resolve(ObjectiveExternalDto source, Objective destination, User destMember, ResolutionContext context)
        {
            var project = dbContext.Users.FirstOrDefault(x => x.ExternalID == source.AuthorExternalID);
            return project;
        }
    }
}