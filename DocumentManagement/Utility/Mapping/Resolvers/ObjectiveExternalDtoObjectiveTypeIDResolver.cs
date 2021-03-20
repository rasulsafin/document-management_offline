using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveExternalDtoObjectiveTypeIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int>
    {
        private readonly DMContext dbContext;

        public ObjectiveExternalDtoObjectiveTypeIdResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public int Resolve(ObjectiveExternalDto source, Objective destination, int destMember, ResolutionContext context)
        {
            var objectiveTypeID = dbContext.ObjectiveTypes.FirstOrDefault(x => x.ExternalId == source.ObjectiveType.ExternalId).ID;
            return objectiveTypeID;
        }
    }
}