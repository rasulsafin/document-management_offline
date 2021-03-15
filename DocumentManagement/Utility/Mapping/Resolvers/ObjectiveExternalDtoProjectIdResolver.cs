using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveExternalDtoProjectIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int>
    {
        private readonly DMContext dbContext;

        public ObjectiveExternalDtoProjectIdResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public int Resolve(ObjectiveExternalDto source, Objective destination, int destMember, ResolutionContext context)
        {
            var project = dbContext.Projects.Synchronized().FirstOrDefault(x => x.ExternalID == source.ProjectExternalID);
            return project?.ID ?? 0;
        }
    }
}
