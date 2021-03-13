using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveObjectiveTypeResolver : IValueResolver<Objective, ObjectiveExternalDto, ObjectiveTypeExternalDto>
    {
        private readonly DMContext dbContext;

        public ObjectiveObjectiveTypeResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ObjectiveTypeExternalDto Resolve(Objective source, ObjectiveExternalDto destination, ObjectiveTypeExternalDto destMember, ResolutionContext context)
        {
            var type = dbContext.ObjectiveTypes.Find(source.ObjectiveTypeID);
            var objectiveTypeExternal = new ObjectiveTypeExternalDto
            {
                Name = type.Name,
                ExternalId = type.ExternalId,
            };

            return objectiveTypeExternal;
        }
    }
}
