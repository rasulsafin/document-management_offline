using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveObjectiveTypeResolver : IValueResolver<Objective, ObjectiveExternalDto, ObjectiveTypeExternalDto>
    {
        public ObjectiveTypeExternalDto Resolve(Objective source, ObjectiveExternalDto destination, ObjectiveTypeExternalDto destMember, ResolutionContext context)
        {
            var objectiveTypeExternal = new ObjectiveTypeExternalDto() { Name = source.ObjectiveType.Name };
            return objectiveTypeExternal;
        }
    }
}
