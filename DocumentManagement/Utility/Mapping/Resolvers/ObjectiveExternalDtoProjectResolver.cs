using System.Linq;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveExternalDtoProjectResolver : IValueResolver<ObjectiveExternalDto, Objective, Project>
    {
        private readonly DMContext dbContext;

        public ObjectiveExternalDtoProjectResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Project Resolve(ObjectiveExternalDto source, Objective destination, Project destMember, ResolutionContext context)
        {
            var project = dbContext.Projects.Synchronized().FirstOrDefault(x => x.ExternalID == source.ProjectExternalID);
            return project;
        }
    }
}