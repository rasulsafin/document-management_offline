using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class ObjectiveProjectIDResolver : IValueResolver<Objective, ObjectiveExternalDto, string>
    {
        private readonly DMContext dbContext;

        public ObjectiveProjectIDResolver(DMContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Resolve(Objective source, ObjectiveExternalDto destination, string destMember, ResolutionContext context)
        {
            var project = dbContext.Projects.Find(source.ProjectID);
            return project.ExternalID;
        }
    }
}