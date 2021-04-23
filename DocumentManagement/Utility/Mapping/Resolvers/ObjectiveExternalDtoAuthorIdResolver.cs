using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveExternalDtoAuthorIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int?>
    {
        private readonly DMContext dbContext;

        public ObjectiveExternalDtoAuthorIdResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public int? Resolve(ObjectiveExternalDto source, Objective destination, int? destMember, ResolutionContext context)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.ExternalID == source.AuthorExternalID);
            return user?.ID;
        }
    }
}