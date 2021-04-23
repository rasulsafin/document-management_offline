using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveExternalDtoObjectiveTypeResolver : IValueResolver<ObjectiveExternalDto, Objective, ObjectiveType>
    {
        private readonly DMContext dbContext;

        public ObjectiveExternalDtoObjectiveTypeResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ObjectiveType Resolve(ObjectiveExternalDto source, Objective destination, ObjectiveType destMember, ResolutionContext context)
        {
            var objectiveType = dbContext.ObjectiveTypes
               .FirstOrDefault(x => x.Name == source.ObjectiveType.Name || x.ExternalId == source.ExternalID);
            return objectiveType;
        }
    }
}
